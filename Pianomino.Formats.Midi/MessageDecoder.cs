using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pianomino.Formats.Midi;

public sealed class MessageDecoder
{
    public enum Problem
    {
        IncompleteMessage,
        MissingEndOfSysEx,
        MissingStatusByte,
        UnknownStatusByte
    }

    private readonly List<byte> bufferedData = new();
    private readonly Queue<RawMessage> messages = new();
    private RunningStatus runningStatus;
    private StatusByte currentStatus;
    private bool ignoreExpectedEndOfSysEx;

    public MessageDecoder(bool ignoreExpectedEndOfSysEx = true)
    {
        this.ignoreExpectedEndOfSysEx = ignoreExpectedEndOfSysEx;
    }

    public event Action<Problem>? ProblemEncountered;

    public StatusByte? RunningStatus => runningStatus.Current;
    public int DecodedCount => messages.Count;
    public bool IsPartial => currentStatus != 0;

    public RawMessage Dequeue() => messages.Dequeue();

    public void Feed(byte @byte)
    {
        var asStatusByte = (StatusByte)@byte;

        // Handle real-time messages
        if (asStatusByte.IsSystemRealTimeMessage())
        {
            // System real-time messages can happen at any time, even within other messages
            // MIDI 1.0 spec, 4.2: "Nothing is done to the buffer during reception of real time messages."
            if (asStatusByte is StatusByte.UndefinedF9 or StatusByte.UndefinedFD)
                ProblemEncountered?.Invoke(Problem.UnknownStatusByte);

            messages.Enqueue(RawMessage.Create(asStatusByte));
            return;
        }

        if (asStatusByte.IsValid())
        {
            // A new message must be starting
            // Handle interruption of prior message
            if (currentStatus == StatusByte.SystemExclusive)
            {
                // MIDI 1.0 spec
                // "Exclusive messages can contain any number of Data bytes, and can be
                // terminated either by an End of Exclusive(EOX) or any other Status byte(except
                // Real Time messages).An EOX should always be sent at the end of a System
                // Exclusive message."
                if (asStatusByte != StatusByte.EndOfExclusive)
                    ProblemEncountered?.Invoke(Problem.MissingEndOfSysEx);

                messages.Enqueue(RawMessage.Create(currentStatus, CollectionsMarshal.AsSpan(bufferedData)));
                bufferedData.Clear();
                currentStatus = 0;

                if (asStatusByte == StatusByte.EndOfExclusive && ignoreExpectedEndOfSysEx) return;
            }
            else if (currentStatus != 0)
            {
                ProblemEncountered?.Invoke(Problem.IncompleteMessage);
                bufferedData.Clear();
                currentStatus = 0;
            }

            // Handle new message
            runningStatus.OnNewStatus(asStatusByte);
            if (asStatusByte.GetPayloadLength() == 0)
                messages.Enqueue(RawMessage.Create(asStatusByte));
            else
            {
                if (asStatusByte is StatusByte.UndefinedF4 or StatusByte.UndefinedF5)
                    ProblemEncountered?.Invoke(Problem.UnknownStatusByte);
                currentStatus = asStatusByte;
            }
        }
        else
        {
            // Handle non-status byte
            if (currentStatus == 0)
            {
                // Handle running status
                if (!runningStatus.IsValid)
                {
                    ProblemEncountered?.Invoke(Problem.MissingStatusByte);
                    return;
                }

                currentStatus = runningStatus.Current!.Value;
            }

            // Handle message continuation
            bufferedData.Add(@byte);
            if (bufferedData.Count != currentStatus.GetPayloadLength()) return;

            messages.Enqueue(RawMessage.Create(currentStatus, CollectionsMarshal.AsSpan(bufferedData)));
            bufferedData.Clear();
            currentStatus = 0;
        }
    }

    public void Feed(StatusByte @byte) => Feed((byte)@byte);

    public void Reset()
    {
        bufferedData.Clear();
        messages.Clear();
        runningStatus.Clear();
        currentStatus = 0;
    }
}
