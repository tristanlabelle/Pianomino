using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public sealed class MessageDecoder
{
    public enum Warning
    {
        IncompleteMessage,
        MissingStatusByte
    }

    private RunningStatus runningStatus;
    private StatusByte bufferedStatus;
    private int remainingDataByteCount;
    private readonly List<byte> bufferedData = new();

    public StatusByte? RunningStatus
    {
        get => runningStatus.Current;
        set => runningStatus.Current = value;
    }

    public RawMessage? Feed(byte @byte, out Warning? warning)
    {
        warning = default;

        var asStatusByte = (StatusByte)@byte;
        if (asStatusByte.IsSystemRealTimeMessage())
        {
            // System real-time messages can happen at any time, even within other messages
            runningStatus.OnNewStatus(asStatusByte);
            return RawMessage.Create(asStatusByte);
        }

        if (bufferedStatus != 0 && asStatusByte.IsValid()) // Unexpected new message case
        {
            if (bufferedStatus == StatusByte.SystemExclusive)
            {
                // Any new message end a system exclusive message
                // although normally it should be an end-of-system-exclusive message
                if (asStatusByte != StatusByte.EndOfExclusive)
                {
                    // Could produce two messages :/
                    throw new NotImplementedException();
                }

                var message = RawMessage.CreateSysEx(ImmutableArray.CreateRange(bufferedData));
                bufferedStatus = 0;
                remainingDataByteCount = 0;
                bufferedData.Clear();
                return message;
            }

            bufferedStatus = 0;
            remainingDataByteCount = 0;
            bufferedData.Clear();
            warning = Warning.IncompleteMessage; // Could produce two errors :/
        }

        if (bufferedStatus == 0) // New message case
        {
            if (asStatusByte.IsValid()) // New status case
            {
                runningStatus.OnNewStatus(asStatusByte);

                remainingDataByteCount = asStatusByte.GetPayloadLength() ?? int.MaxValue;
                if (remainingDataByteCount == 0) return RawMessage.Create(asStatusByte);

                bufferedStatus = asStatusByte;
                return null;
            }
            else // Running status case
            {
                if (!runningStatus.IsValid)
                {
                    warning = Warning.MissingStatusByte;
                    return null;
                }

                var status = runningStatus.Current!.Value; // Not null by test above
                if (status.GetPayloadLengthType() == ShortPayloadLengthType.OneByte)
                    return RawMessage.Create(status, @byte);

                // Two-byte channel message
                bufferedStatus = status;
                bufferedData.Add(@byte);
                remainingDataByteCount = 1;
                return null;
            }
        }
        else // Message continuation case
        {
            Debug.Assert(!asStatusByte.IsValid()); // Case already handled

            bufferedData.Add(@byte);
            remainingDataByteCount--;
            if (remainingDataByteCount > 0) return null;

            var message = bufferedData.Count switch
            {
                1 => RawMessage.Create(bufferedStatus, bufferedData[0]),
                2 => RawMessage.Create(bufferedStatus, bufferedData[0], bufferedData[1]),
                _ => RawMessage.Create(bufferedStatus, ImmutableArray.CreateRange(bufferedData)),
            };

            bufferedStatus = 0;
            bufferedData.Clear();
            return message;
        }
    }
}
