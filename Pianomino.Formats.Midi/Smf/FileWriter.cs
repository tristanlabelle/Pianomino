using System;
using System.IO;
using System.Text;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// Writes a standard midi file (SMF) element by element.
/// </summary>
public sealed class FileWriter : IFileSink, IDisposable
{
    private readonly BinaryWriter binaryWriter;
    private readonly long headerChunkPosition;
    private bool isSingleTrack;
    private int trackCount;
    private long currentTrackChunkHeaderPosition;
    private RunningStatus runningStatus;
    public FileSinkState State { get; private set; }

    public FileWriter(Stream stream, bool transferOwnership)
    {
        try
        {
            if (!stream.CanWrite || !stream.CanSeek) throw new ArgumentException(message: null, paramName: nameof(stream));

            headerChunkPosition = stream.Position;
            binaryWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: !transferOwnership);
        }
        catch (Exception) when (transferOwnership)
        {
            stream.Dispose();
            throw;
        }
    }

    public FileWriter(Stream stream, bool transferOwnership, TrackFormat trackFormat, TimeDivision timeDivision)
        : this(stream, transferOwnership)
    {
        Begin(trackFormat, timeDivision);
    }

    public void Begin(TrackFormat trackFormat, TimeDivision timeDivision)
    {
        if (State != FileSinkState.Initial) throw new InvalidOperationException();

        isSingleTrack = trackFormat == TrackFormat.Single;

        Write(new FileFormat.ChunkHeader { Type = FileFormat.ChunkType.Header, Length = FileFormat.HeaderChunk.Length });
        binaryWriter.WriteBigEndian((ushort)trackFormat);
        binaryWriter.WriteBigEndian((ushort)0); // TrackCount, to be overwritten
        binaryWriter.WriteBigEndian(timeDivision.RawValue);

        State = FileSinkState.BetweenTracks;
    }

    public void BeginTrack()
    {
        if (State != FileSinkState.BetweenTracks) throw new InvalidOperationException();
        if (trackCount == 1 && isSingleTrack)
            throw new InvalidOperationException();

        binaryWriter.Flush();
        currentTrackChunkHeaderPosition = binaryWriter.BaseStream.Position;
        trackCount++;
        Write(new FileFormat.ChunkHeader { Type = FileFormat.ChunkType.Track, Length = 0 });
        State = FileSinkState.InTrack;
    }

    public void EndTrack()
    {
        if (State == FileSinkState.InTrack)
        {
            WriteMeta(0, MetaEventTypeByte.EndOfTrack, data: ReadOnlySpan<byte>.Empty);
            State = FileSinkState.AtEndOfTrackEvent;
        }

        if (State != FileSinkState.AtEndOfTrackEvent)
            throw new InvalidOperationException();

        binaryWriter.Flush();
        long trackEndPosition = binaryWriter.BaseStream.Position;
        binaryWriter.BaseStream.Position = currentTrackChunkHeaderPosition;

        // Overwrite chunk header with final length
        if (trackEndPosition != 0) // Ignore null stream
        {
            var chunkLength = checked((uint)(trackEndPosition - currentTrackChunkHeaderPosition - FileFormat.ChunkHeader.SizeInBytes));
            Write(new FileFormat.ChunkHeader { Type = FileFormat.ChunkType.Track, Length = chunkLength });
        }

        binaryWriter.Flush();
        binaryWriter.BaseStream.Position = trackEndPosition;
        currentTrackChunkHeaderPosition = 0;

        State = FileSinkState.BetweenTracks;
    }

    public void ResetRunningStatus() => runningStatus.Reset();

    public void WriteChannel(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0)
    {
        if (State != FileSinkState.InTrack) throw new InvalidOperationException();
        if (!status.IsChannelMessage()) throw new ArgumentOutOfRangeException(nameof(status));
        if (!RawMessage.IsValidPayloadByte(firstDataByte)) throw new ArgumentOutOfRangeException(nameof(firstDataByte));
        if (!RawMessage.IsValidPayloadByte(secondDataByte)) throw new ArgumentOutOfRangeException(nameof(firstDataByte));

        binaryWriter.Write7BitEncodedInt(checked((int)timeDelta));
        if (runningStatus.Current != status)
        {
            binaryWriter.Write((byte)status);
            runningStatus.OnNewStatus(status);
        }

        binaryWriter.Write(firstDataByte);
        if (status.GetPayloadLengthType() == ShortPayloadLengthType.TwoBytes)
            binaryWriter.Write(secondDataByte);
    }

    public void WriteEscape(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data)
    {
        if (State != FileSinkState.InTrack) throw new InvalidOperationException();
        binaryWriter.Write7BitEncodedInt(checked((int)timeDelta));
        binaryWriter.Write(sysExPrefix ? (byte)EventHeaderByte.Escape_SysEx : (byte)EventHeaderByte.Escape);
        binaryWriter.Write(data);
    }

    public void WriteMeta(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
    {
        if (State != FileSinkState.InTrack) throw new InvalidOperationException();

        binaryWriter.Write7BitEncodedInt(checked((int)timeDelta));
        binaryWriter.Write((byte)EventHeaderByte.Meta);
        binaryWriter.Write((byte)type);
        binaryWriter.Write7BitEncodedInt(data.Length);
        binaryWriter.Write(data);

        runningStatus.Reset();

        if (type == MetaEventTypeByte.EndOfTrack)
            State = FileSinkState.AtEndOfTrackEvent;
    }

    public void Write(uint timeDelta, in RawEvent message)
    {
        if (message.IsChannel)
        {
            var wireMessage = message.ToMessage();
            WriteChannel(timeDelta, wireMessage.Status, wireMessage.Payload[0], wireMessage.Payload.Length == 2 ? wireMessage.Payload[1] : (byte)0);
        }
        else if (message.IsMeta)
        {
            WriteMeta(timeDelta, message.GetMetaType(), message.Payload.AsSpan());
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void End()
    {
        if (State == FileSinkState.Ended) return;

        if (State is FileSinkState.InTrack or FileSinkState.AtEndOfTrackEvent) EndTrack();

        binaryWriter.Flush();
        var endPosition = binaryWriter.BaseStream.Position;

        // Overwrite track count in header
        binaryWriter.BaseStream.Position = headerChunkPosition + FileFormat.ChunkHeader.SizeInBytes + 2;
        binaryWriter.WriteBigEndian((ushort)trackCount);

        binaryWriter.Flush();
        binaryWriter.BaseStream.Position = endPosition;

        binaryWriter.Dispose();

        State = FileSinkState.Ended;
    }

    public void Dispose() => End();

    private void Write(FileFormat.ChunkHeader chunkHeader)
    {
        binaryWriter.WriteBigEndian((uint)chunkHeader.Type);
        binaryWriter.WriteBigEndian(chunkHeader.Length);
    }

    void IFileSink.AddEvent(uint timeDelta, in RawEvent message)
        => Write(timeDelta, in message);

    void IFileSink.AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte)
        => WriteChannel(timeDelta, status, firstDataByte, secondDataByte);

    void IFileSink.AddEscapeEvent(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data)
        => WriteEscape(timeDelta, sysExPrefix, data);

    void IFileSink.AddMetaEvent(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
        => WriteMeta(timeDelta, type, data);
}
