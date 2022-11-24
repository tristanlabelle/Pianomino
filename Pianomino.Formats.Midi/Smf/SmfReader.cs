using System;
using System.Collections.Immutable;
using System.IO;

namespace Pianomino.Formats.Midi.Smf;

public enum SmfReaderState
{
    PostHeader,
    StartOfTrack,
    Event,
    EndOfTrack,
    EndOfFile
}

/// <summary>
/// Reads Standard MIDI Files (SMF format) from a data stream, element by element.
/// </summary>
public sealed class SmfReader
{
    private enum RunningStatusState : byte
    {
        None = 0,
        Valid = 1,
        Invalidated = 2
    }

    private readonly BinaryReader binaryReader;
    private readonly SmfValidationErrorHandler? validationErrorHandler;
    private readonly SmfFormat.HeaderChunk headerChunk;
    public bool IsRmidFile { get; }
    private int trackIndex = -1;
    private long trackEndPosition;
    private long timeInTicks;
    private int eventDeltaTime; // iff State == Event
    private RawSmfMessage eventMessage; // iff State == Event
    private StatusByte runningStatus;
    private bool isRunningStatusInvalidated;
    private bool isInPacketedSysEx;
    private bool hasEndOfTrackMetaEvent;
    public SmfReaderState State { get; private set; }

    public SmfReader(Stream stream, bool allowRiffHeader = true, SmfValidationErrorHandler? validationErrorHandler = null)
    {
        this.binaryReader = new BinaryReader(stream);
        this.validationErrorHandler = validationErrorHandler;

        var firstChunkTypeDword = binaryReader.ReadBigEndianUInt32();

        // The SMF file can be wrapped in an IFF file container, with RIFF+RMID header
        if (allowRiffHeader && firstChunkTypeDword == SmfFormat.RmidGroupID)
        {
            var length = binaryReader.ReadBigEndianUInt32();
            var rmid = binaryReader.ReadBigEndianUInt32();
            if (rmid != SmfFormat.RmidTypeID) throw new FormatException();

            IsRmidFile = true;
            firstChunkTypeDword = binaryReader.ReadBigEndianUInt32();
        }

        var chunkHeader = new SmfFormat.ChunkHeader
        {
            Type = (SmfFormat.ChunkType)firstChunkTypeDword,
            Length = binaryReader.ReadBigEndianUInt32()
        };

        while (true)
        {
            if (chunkHeader.Type == SmfFormat.ChunkType.Header)
            {
                if (chunkHeader.Length < SmfFormat.HeaderChunk.Length) throw new FormatException();
                headerChunk = new SmfFormat.HeaderChunk
                {
                    TrackFormat = (SmfTrackFormat)binaryReader.ReadBigEndianUInt16(),
                    TrackCount = binaryReader.ReadBigEndianUInt16(),
                    TimeDivision = TimeDivision.FromRawValue(binaryReader.ReadBigEndianUInt16())
                };

                // Skip unknown additional header bytes
                if (chunkHeader.Length > SmfFormat.HeaderChunk.Length)
                    binaryReader.BaseStream.Seek(chunkHeader.Length - SmfFormat.HeaderChunk.Length, SeekOrigin.Current);

                if (validationErrorHandler is object)
                {
                    if ((int)headerChunk.TrackFormat >= 3)
                        validationErrorHandler(SmfValidationError.InvalidMThdFormat);

                    if (headerChunk.TrackFormat == SmfTrackFormat.Single && headerChunk.TrackCount != 1)
                        validationErrorHandler(SmfValidationError.ExtraTracks);

                    bool isTimeDivisionValid = headerChunk.TimeDivision.IsTicksPerQuarterNote
                        ? headerChunk.TimeDivision.TicksPerQuarterNote > 0
                        : (headerChunk.TimeDivision.SmpteFormat.IsDefined() && headerChunk.TimeDivision.SmpteTicksPerFrame > 0);
                    if (!isTimeDivisionValid)
                        validationErrorHandler(SmfValidationError.InvalidMThdTimeDivision);
                }
                break;
            }
            else if (chunkHeader.Type == SmfFormat.ChunkType.Track)
            {
                throw new FormatException("Invalid midi file, track chunk without header chunk");
            }
            else
            {
                var dword = (uint)chunkHeader.Type;
                if (!IsPrintableAscii((byte)(dword >> 24)) || !IsPrintableAscii((byte)(dword >> 16))
                    || !IsPrintableAscii((byte)(dword >> 8)) || !IsPrintableAscii((byte)dword))
                {
                    throw new FormatException("Invalid midi file, chunk expected");
                }

                // Assumption: binaryReader doesn't buffer more than for a single read operation,
                // so seeking is ok.
                binaryReader.BaseStream.Seek(chunkHeader.Length, SeekOrigin.Current);
                chunkHeader = ReadChunkHeader(binaryReader);
            }
        }
    }

    public SmfTrackFormat TrackFormat => headerChunk.TrackFormat;
    public int TrackCount => headerChunk.TrackCount;
    public TimeDivision TimeDivision => headerChunk.TimeDivision;

    public bool IsInTrack => State is SmfReaderState.StartOfTrack or SmfReaderState.Event;

    public int GetTrackIndex() => State is SmfReaderState.StartOfTrack or SmfReaderState.Event or SmfReaderState.EndOfTrack
        ? trackIndex : throw new InvalidOperationException();

    public StatusByte? RunningStatus => runningStatus != 0 && !isRunningStatusInvalidated
        ? runningStatus : null;

    public long GetTimeInTicks() => IsInTrack ? timeInTicks : throw new InvalidOperationException();
    public int GetEventTimeDelta() => State == SmfReaderState.Event ? eventDeltaTime : throw new InvalidOperationException();
    public RawSmfMessage GetEventMessage() => State == SmfReaderState.Event ? eventMessage : throw new InvalidOperationException();

    public SmfReaderState Read()
    {
        if (State == SmfReaderState.EndOfFile) return State;

        if ((State == SmfReaderState.StartOfTrack || State == SmfReaderState.Event)
            && binaryReader.BaseStream.Position != trackEndPosition)
        {
            if (hasEndOfTrackMetaEvent)
            {
                validationErrorHandler?.Invoke(SmfValidationError.InvalidTrackTermination);
                hasEndOfTrackMetaEvent = false;
            }

            // Read next event
            (eventDeltaTime, eventMessage) = ReadEvent();
            timeInTicks += eventDeltaTime;
            return State = SmfReaderState.Event;
        }
        else
        {
            // Read to next track

            // Clean up previous track
            if (State == SmfReaderState.StartOfTrack || State == SmfReaderState.Event)
            {
                if (!hasEndOfTrackMetaEvent)
                    validationErrorHandler?.Invoke(SmfValidationError.InvalidTrackTermination);

                runningStatus = 0;
                isRunningStatusInvalidated = true;
                timeInTicks = 0;
                trackEndPosition = 0;
                hasEndOfTrackMetaEvent = false;

                return State = SmfReaderState.EndOfTrack;
            }

            if (trackIndex == TrackCount - 1)
            {
                trackIndex++;
                return State = SmfReaderState.EndOfFile;
            }

            while (true)
            {
                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
                {
                    return State = SmfReaderState.EndOfFile;
                }

                var chunkHeader = ReadChunkHeader(binaryReader);
                if (chunkHeader.Type == SmfFormat.ChunkType.Track)
                {
                    if (trackIndex + 1 >= TrackCount)
                    {
                        // More tracks than expected
                        validationErrorHandler?.Invoke(SmfValidationError.ExtraTracks);
                    }

                    trackEndPosition = binaryReader.BaseStream.Position + chunkHeader.Length;
                    trackIndex++;
                    return State = SmfReaderState.StartOfTrack;
                }

                if (chunkHeader.Type == SmfFormat.ChunkType.Header)
                    validationErrorHandler?.Invoke(SmfValidationError.MultipleMThdChunks);

                // Skip the chunk
                binaryReader.BaseStream.Seek(chunkHeader.Length, SeekOrigin.Current);
            }
        }
    }

    private (int DeltaTime, RawSmfMessage Message) ReadEvent()
    {
        var deltaTime = ReadVariableLengthInt(binaryReader);
        var message = ReadMessage();

        var eventEndPosition = binaryReader.BaseStream.Position;
        if (eventEndPosition > trackEndPosition) throw new FormatException();

        return (deltaTime, message);
    }

    private RawSmfMessage ReadMessage()
    {
        var firstByte = binaryReader.ReadByte();
        return firstByte < 0xF0
            ? RawSmfMessage.FromWireMessage(ReadChannelMessage(firstByte))
            : ReadNonChannelMessage(firstByte);
    }

    private RawMessage ReadChannelMessage(byte firstByte)
    {
        CancelSysEx();

        StatusByte status;
        byte firstDataByte;
        if (RawMessage.IsValidDataByte(firstByte))
        {
            // Data without status, must be using running status
            if (runningStatus == 0) throw new FormatException();
            if (isRunningStatusInvalidated)
            {
                // SysEx and Meta events should invalidate the running status,
                // but some files do not follow this rule
                validationErrorHandler?.Invoke(SmfValidationError.RunningStatusInvalidated);
                isRunningStatusInvalidated = false;
            }

            status = runningStatus;
            firstDataByte = firstByte;
        }
        else
        {
            status = (StatusByte)firstByte;
            runningStatus = status;
            isRunningStatusInvalidated = false;
            firstDataByte = binaryReader.ReadByte();
        }

        if (firstDataByte >= 0x80)
        {
            validationErrorHandler?.Invoke(SmfValidationError.InvalidMessageData);
            firstDataByte &= 0x7F;
        }

        bool hasOneDataByte = status.GetDataLengthType() == MessageDataLengthType.OneByte;
        if (hasOneDataByte) return RawMessage.Create(status, firstDataByte);

        byte secondDataByte = binaryReader.ReadByte();
        if (secondDataByte >= 0x80)
        {
            validationErrorHandler?.Invoke(SmfValidationError.InvalidMessageData);
            secondDataByte &= 0x7F;
        }

        return RawMessage.Create(status, firstDataByte, secondDataByte);
    }

    private RawSmfMessage ReadNonChannelMessage(byte status)
    {
        isRunningStatusInvalidated = true;

        if (status == RawSmfMessage.Status_SysEx || status == RawSmfMessage.Status_Escape)
        {
            int length = ReadVariableLengthInt(binaryReader);
            var bytes = binaryReader.ReadBytes(length);
            if (status == RawSmfMessage.Status_Escape && !isInPacketedSysEx)
            {
                return RawSmfMessage.CreateEscape(bytes.ToImmutableArray());
            }
            else
            {
                if (status == RawSmfMessage.Status_SysEx)
                    CancelSysEx();

                bool terminated = bytes.Length > 0 && bytes[^1] == 0xF7;
                var data = ImmutableArray.Create(bytes, start: 0, length: terminated ? bytes.Length - 1 : bytes.Length);
                isInPacketedSysEx = !terminated;
                return RawSmfMessage.CreateSystemExclusive(data, first: status == RawSmfMessage.Status_SysEx, last: terminated);
            }
        }
        else if (status == RawSmfMessage.Status_Meta)
        {
            CancelSysEx();

            var metaEventType = (MetaMessageTypeByte)binaryReader.ReadByte();
            int length = ReadVariableLengthInt(binaryReader);
            var data = binaryReader.ReadBytes(length).ToImmutableArray();

            if (TrackFormat != SmfTrackFormat.Independent && metaEventType.IsFirstTrackOnly() && trackIndex != 0)
                validationErrorHandler?.Invoke(SmfValidationError.InvalidTrackForMetaEvent);

            if (metaEventType == MetaMessageTypeByte.EndOfTrack)
                hasEndOfTrackMetaEvent = true;

            return RawSmfMessage.CreateMeta(metaEventType, data);
        }
        else
        {
            throw new FormatException();
        }
    }

    private void CancelSysEx()
    {
        if (isInPacketedSysEx)
        {
            validationErrorHandler?.Invoke(SmfValidationError.UnterminatedSysEx);
            isInPacketedSysEx = false;
        }
    }

    public static void Strict(SmfValidationError validationError)
    {
        throw new FormatException($"Midi file validation failed: {validationError}");
    }

    public static void ReadToSink(Stream stream, ISmfSink sink, bool allowRiffHeader = true, SmfValidationErrorHandler? validationErrorHandler = null)
    {
        var reader = new SmfReader(stream, allowRiffHeader, validationErrorHandler);
        sink.Begin(reader.TrackFormat, reader.TimeDivision);

        bool isInTrack = false;
        while (true)
        {
            reader.Read();
            if (isInTrack && reader.State != SmfReaderState.Event)
            {
                sink.EndTrack();
                isInTrack = false;
            }

            if (reader.State == SmfReaderState.EndOfFile) break;

            if (reader.State == SmfReaderState.StartOfTrack)
            {
                if (isInTrack) throw new UnreachableException();
                sink.BeginTrack();
                isInTrack = true;
            }
            else if (reader.State == SmfReaderState.Event)
            {
                sink.AddEvent((uint)reader.GetEventTimeDelta(), reader.GetEventMessage());
            }
        }

        sink.End();
    }

    public static SmfData Read(Stream stream, bool allowRiffHeader = true, SmfValidationErrorHandler? validationErrorHandler = null)
    {
        var builderSink = new SmfData.BuilderSink();
        ReadToSink(stream, builderSink, allowRiffHeader, validationErrorHandler);
        if (builderSink is null) throw new UnreachableException();

        return builderSink.Build();
    }

    private static SmfFormat.ChunkHeader ReadChunkHeader(BinaryReader binaryReader) => new SmfFormat.ChunkHeader
    {
        Type = (SmfFormat.ChunkType)binaryReader.ReadBigEndianUInt32(),
        Length = binaryReader.ReadBigEndianUInt32()
    };

    private static bool IsPrintableAscii(byte b) => b >= 0x20 && b < 0x7F; // 7F == DEL

    private static int ReadVariableLengthInt(BinaryReader binaryReader)
    {
        int value = 0;
        int byteCount = 0;
        while (true)
        {
            var b = binaryReader.ReadByte();
            byteCount++;
            value <<= 7;
            value |= b & 0x7F;
            if ((b & 0x80) == 0) return value;
            if (byteCount == 4) throw new FormatException();
        }
    }
}
