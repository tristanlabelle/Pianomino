using System;
using System.Collections.Immutable;
using System.IO;

namespace Pianomino.Formats.Midi.Smf;

public enum FileReaderState
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
public sealed class FileReader
{
    private enum RunningStatusState : byte
    {
        None = 0,
        Valid = 1,
        Invalidated = 2
    }

    private readonly BinaryReader binaryReader;
    private readonly FileValidationErrorHandler? validationErrorHandler;
    private readonly FileFormat.HeaderChunk headerChunk;
    public bool IsRmidFile { get; }
    private int trackIndex = -1;
    private long trackEndPosition;
    private long timeInTicks;
    private int eventDeltaTime; // iff State == Event
    private RawEvent eventMessage; // iff State == Event
    private StatusByte runningStatus;
    private bool isRunningStatusInvalidated;
    private bool hasEndOfTrackMetaEvent;
    public FileReaderState State { get; private set; }

    public FileReader(Stream stream, bool allowRiffHeader = true, FileValidationErrorHandler? validationErrorHandler = null)
    {
        this.binaryReader = new BinaryReader(stream);
        this.validationErrorHandler = validationErrorHandler;

        var firstChunkTypeDword = binaryReader.ReadBigEndianUInt32();

        // The SMF file can be wrapped in an IFF file container, with RIFF+RMID header
        if (allowRiffHeader && firstChunkTypeDword == FileFormat.RmidGroupID)
        {
            var length = binaryReader.ReadBigEndianUInt32();
            var rmid = binaryReader.ReadBigEndianUInt32();
            if (rmid != FileFormat.RmidTypeID) throw new FormatException();

            IsRmidFile = true;
            firstChunkTypeDword = binaryReader.ReadBigEndianUInt32();
        }

        var chunkHeader = new FileFormat.ChunkHeader
        {
            Type = (FileFormat.ChunkType)firstChunkTypeDword,
            Length = binaryReader.ReadBigEndianUInt32()
        };

        while (true)
        {
            if (chunkHeader.Type == FileFormat.ChunkType.Header)
            {
                if (chunkHeader.Length < FileFormat.HeaderChunk.Length) throw new FormatException();
                headerChunk = new FileFormat.HeaderChunk
                {
                    TrackFormat = (TrackFormat)binaryReader.ReadBigEndianUInt16(),
                    TrackCount = binaryReader.ReadBigEndianUInt16(),
                    TimeDivision = TimeDivision.FromRawValue(binaryReader.ReadBigEndianUInt16())
                };

                // Skip unknown additional header bytes
                if (chunkHeader.Length > FileFormat.HeaderChunk.Length)
                    binaryReader.BaseStream.Seek(chunkHeader.Length - FileFormat.HeaderChunk.Length, SeekOrigin.Current);

                if (validationErrorHandler is not null)
                {
                    if ((int)headerChunk.TrackFormat >= 3)
                        validationErrorHandler(FileValidationError.InvalidMThdFormat);

                    if (headerChunk.TrackFormat == TrackFormat.Single && headerChunk.TrackCount != 1)
                        validationErrorHandler(FileValidationError.ExtraTracks);

                    bool isTimeDivisionValid = headerChunk.TimeDivision.IsTicksPerQuarterNote
                        ? headerChunk.TimeDivision.TicksPerQuarterNote > 0
                        : (headerChunk.TimeDivision.SmpteFormat.IsDefined() && headerChunk.TimeDivision.SmpteTicksPerFrame > 0);
                    if (!isTimeDivisionValid)
                        validationErrorHandler(FileValidationError.InvalidMThdTimeDivision);
                }
                break;
            }
            else if (chunkHeader.Type == FileFormat.ChunkType.Track)
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

    public TrackFormat TrackFormat => headerChunk.TrackFormat;
    public int TrackCount => headerChunk.TrackCount;
    public TimeDivision TimeDivision => headerChunk.TimeDivision;

    public bool IsInTrack => State is FileReaderState.StartOfTrack or FileReaderState.Event;

    public int GetTrackIndex() => State is FileReaderState.StartOfTrack or FileReaderState.Event or FileReaderState.EndOfTrack
        ? trackIndex : throw new InvalidOperationException();

    public StatusByte? RunningStatus => runningStatus != 0 && !isRunningStatusInvalidated
        ? runningStatus : null;

    public long GetTimeInTicks() => IsInTrack ? timeInTicks : throw new InvalidOperationException();
    public int GetEventTimeDelta() => State == FileReaderState.Event ? eventDeltaTime : throw new InvalidOperationException();
    public RawEvent GetEvent() => State == FileReaderState.Event ? eventMessage : throw new InvalidOperationException();

    public FileReaderState Read()
    {
        if (State == FileReaderState.EndOfFile) return State;

        if ((State == FileReaderState.StartOfTrack || State == FileReaderState.Event)
            && binaryReader.BaseStream.Position != trackEndPosition)
        {
            if (hasEndOfTrackMetaEvent)
            {
                validationErrorHandler?.Invoke(FileValidationError.InvalidTrackTermination);
                hasEndOfTrackMetaEvent = false;
            }

            // Read next event
            (eventDeltaTime, eventMessage) = ReadEvent();
            timeInTicks += eventDeltaTime;
            return State = FileReaderState.Event;
        }
        else
        {
            // Read to next track

            // Clean up previous track
            if (State == FileReaderState.StartOfTrack || State == FileReaderState.Event)
            {
                if (!hasEndOfTrackMetaEvent)
                    validationErrorHandler?.Invoke(FileValidationError.InvalidTrackTermination);

                runningStatus = 0;
                isRunningStatusInvalidated = true;
                timeInTicks = 0;
                trackEndPosition = 0;
                hasEndOfTrackMetaEvent = false;

                return State = FileReaderState.EndOfTrack;
            }

            if (trackIndex == TrackCount - 1)
            {
                trackIndex++;
                return State = FileReaderState.EndOfFile;
            }

            while (true)
            {
                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
                {
                    return State = FileReaderState.EndOfFile;
                }

                var chunkHeader = ReadChunkHeader(binaryReader);
                if (chunkHeader.Type == FileFormat.ChunkType.Track)
                {
                    if (trackIndex + 1 >= TrackCount)
                    {
                        // More tracks than expected
                        validationErrorHandler?.Invoke(FileValidationError.ExtraTracks);
                    }

                    trackEndPosition = binaryReader.BaseStream.Position + chunkHeader.Length;
                    trackIndex++;
                    return State = FileReaderState.StartOfTrack;
                }

                if (chunkHeader.Type == FileFormat.ChunkType.Header)
                    validationErrorHandler?.Invoke(FileValidationError.MultipleMThdChunks);

                // Skip the chunk
                binaryReader.BaseStream.Seek(chunkHeader.Length, SeekOrigin.Current);
            }
        }
    }

    private (int DeltaTime, RawEvent Message) ReadEvent()
    {
        var deltaTime = ReadVariableLengthInt(binaryReader);
        var message = ReadMessage();

        var eventEndPosition = binaryReader.BaseStream.Position;
        if (eventEndPosition > trackEndPosition) throw new FormatException();

        return (deltaTime, message);
    }

    private RawEvent ReadMessage()
    {
        var firstByte = binaryReader.ReadByte();
        return firstByte < 0xF0
            ? RawEvent.FromMessage(ReadChannelMessage(firstByte))
            : ReadNonChannelMessage((EventHeaderByte)firstByte);
    }

    private RawMessage ReadChannelMessage(byte firstByte)
    {
        StatusByte status;
        byte firstDataByte;
        if (RawMessage.IsValidPayloadByte(firstByte))
        {
            // Data without status, must be using running status
            if (runningStatus == 0) throw new FormatException();
            if (isRunningStatusInvalidated)
            {
                // SysEx and Meta events should invalidate the running status,
                // but some files do not follow this rule
                validationErrorHandler?.Invoke(FileValidationError.RunningStatusInvalidated);
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
            validationErrorHandler?.Invoke(FileValidationError.InvalidMessageData);
            firstDataByte &= 0x7F;
        }

        bool hasOneDataByte = status.GetPayloadLengthType() == ShortPayloadLengthType.OneByte;
        if (hasOneDataByte) return RawMessage.Create(status, firstDataByte);

        byte secondDataByte = binaryReader.ReadByte();
        if (secondDataByte >= 0x80)
        {
            validationErrorHandler?.Invoke(FileValidationError.InvalidMessageData);
            secondDataByte &= 0x7F;
        }

        return RawMessage.Create(status, firstDataByte, secondDataByte);
    }

    private RawEvent ReadNonChannelMessage(EventHeaderByte headerByte)
    {
        isRunningStatusInvalidated = true;

        if (headerByte == EventHeaderByte.Escape_SysEx || headerByte == EventHeaderByte.Escape)
        {
            int length = ReadVariableLengthInt(binaryReader);
            var bytes = binaryReader.ReadBytes(length);
            return RawEvent.CreateEscape(sysExPrefix: headerByte == EventHeaderByte.Escape_SysEx, bytes.ToImmutableArray());
        }
        else if (headerByte == EventHeaderByte.Meta)
        {
            var metaEventType = (MetaEventTypeByte)binaryReader.ReadByte();
            int length = ReadVariableLengthInt(binaryReader);
            var data = binaryReader.ReadBytes(length).ToImmutableArray();

            if (TrackFormat != TrackFormat.Independent && metaEventType.IsFirstTrackOnly() && trackIndex != 0)
                validationErrorHandler?.Invoke(FileValidationError.InvalidTrackForMetaEvent);

            if (metaEventType == MetaEventTypeByte.EndOfTrack)
                hasEndOfTrackMetaEvent = true;

            return RawEvent.CreateMeta(metaEventType, data);
        }
        else
        {
            throw new FormatException();
        }
    }

    public static void Strict(FileValidationError validationError)
    {
        throw new FormatException($"Midi file validation failed: {validationError}");
    }

    public static void ReadToSink(Stream stream, IFileSink sink, bool allowRiffHeader = true, FileValidationErrorHandler? validationErrorHandler = null)
    {
        var reader = new FileReader(stream, allowRiffHeader, validationErrorHandler);
        sink.Begin(reader.TrackFormat, reader.TimeDivision);

        bool isInTrack = false;
        while (true)
        {
            reader.Read();
            if (isInTrack && reader.State != FileReaderState.Event)
            {
                sink.EndTrack();
                isInTrack = false;
            }

            if (reader.State == FileReaderState.EndOfFile) break;

            if (reader.State == FileReaderState.StartOfTrack)
            {
                if (isInTrack) throw new UnreachableException();
                sink.BeginTrack();
                isInTrack = true;
            }
            else if (reader.State == FileReaderState.Event)
            {
                sink.AddEvent((uint)reader.GetEventTimeDelta(), reader.GetEvent());
            }
        }

        sink.End();
    }

    public static FileModel Read(Stream stream, bool allowRiffHeader = true, FileValidationErrorHandler? validationErrorHandler = null)
    {
        var builderSink = new FileModel.BuilderSink();
        ReadToSink(stream, builderSink, allowRiffHeader, validationErrorHandler);
        if (builderSink is null) throw new UnreachableException();

        return builderSink.Build();
    }

    private static FileFormat.ChunkHeader ReadChunkHeader(BinaryReader binaryReader) => new()
    {
        Type = (FileFormat.ChunkType)binaryReader.ReadBigEndianUInt32(),
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
