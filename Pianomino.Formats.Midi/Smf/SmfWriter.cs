using System;
using System.IO;
using System.Text;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// Writes a standard midi file (SMF) element by element.
/// </summary>
public sealed class SmfWriter : ISmfSink, IDisposable
{
    private readonly BinaryWriter binaryWriter;
    private readonly long headerChunkPosition;
    private bool isSingleTrack;
    private int trackCount;
    private long currentTrackChunkHeaderPosition;
    private RunningStatus runningStatus;
    public SmfSinkState State { get; private set; }

    public SmfWriter(Stream stream, bool transferOwnership)
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

    public SmfWriter(Stream stream, bool transferOwnership, SmfTrackFormat trackFormat, TimeDivision timeDivision)
        : this(stream, transferOwnership)
    {
        Begin(trackFormat, timeDivision);
    }

    public void Begin(SmfTrackFormat trackFormat, TimeDivision timeDivision)
    {
        if (State != SmfSinkState.Initial) throw new InvalidOperationException();

        isSingleTrack = trackFormat == SmfTrackFormat.Single;

        Write(new SmfFormat.ChunkHeader { Type = SmfFormat.ChunkType.Header, Length = SmfFormat.HeaderChunk.Length });
        binaryWriter.WriteBigEndian((ushort)trackFormat);
        binaryWriter.WriteBigEndian((ushort)0); // TrackCount, to be overwritten
        binaryWriter.WriteBigEndian(timeDivision.RawValue);

        State = SmfSinkState.BetweenTracks;
    }

    public void BeginTrack()
    {
        if (State != SmfSinkState.BetweenTracks) throw new InvalidOperationException();
        if (trackCount == 1 && isSingleTrack)
            throw new InvalidOperationException();

        binaryWriter.Flush();
        currentTrackChunkHeaderPosition = binaryWriter.BaseStream.Position;
        trackCount++;
        Write(new SmfFormat.ChunkHeader { Type = SmfFormat.ChunkType.Track, Length = 0 });
        State = SmfSinkState.InTrack;
    }

    public void EndTrack()
    {
        if (State == SmfSinkState.InTrack)
        {
            WriteMeta(0, MetaEventTypeByte.EndOfTrack, data: ReadOnlySpan<byte>.Empty);
            State = SmfSinkState.AtEndOfTrackEvent;
        }

        if (State != SmfSinkState.AtEndOfTrackEvent)
            throw new InvalidOperationException();

        binaryWriter.Flush();
        long trackEndPosition = binaryWriter.BaseStream.Position;
        binaryWriter.BaseStream.Position = currentTrackChunkHeaderPosition;

        // Overwrite chunk header with final length
        if (trackEndPosition != 0) // Ignore null stream
        {
            var chunkLength = checked((uint)(trackEndPosition - currentTrackChunkHeaderPosition - SmfFormat.ChunkHeader.SizeInBytes));
            Write(new SmfFormat.ChunkHeader { Type = SmfFormat.ChunkType.Track, Length = chunkLength });
        }

        binaryWriter.Flush();
        binaryWriter.BaseStream.Position = trackEndPosition;
        currentTrackChunkHeaderPosition = 0;

        State = SmfSinkState.BetweenTracks;
    }

    public void ResetRunningStatus() => runningStatus.Reset();

    public void WriteChannel(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0)
    {
        if (State != SmfSinkState.InTrack) throw new InvalidOperationException();
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
        if (State != SmfSinkState.InTrack) throw new InvalidOperationException();
        binaryWriter.Write7BitEncodedInt(checked((int)timeDelta));
        binaryWriter.Write(sysExPrefix ? (byte)EventHeaderByte.Escape_SysEx : (byte)EventHeaderByte.Escape);
        binaryWriter.Write(data);
    }

    public void WriteMeta(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
    {
        if (State != SmfSinkState.InTrack) throw new InvalidOperationException();

        binaryWriter.Write7BitEncodedInt(checked((int)timeDelta));
        binaryWriter.Write((byte)EventHeaderByte.Meta);
        binaryWriter.Write((byte)type);
        binaryWriter.Write7BitEncodedInt(data.Length);
        binaryWriter.Write(data);

        runningStatus.Reset();

        if (type == MetaEventTypeByte.EndOfTrack)
            State = SmfSinkState.AtEndOfTrackEvent;
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
        if (State == SmfSinkState.Ended) return;

        if (State is SmfSinkState.InTrack or SmfSinkState.AtEndOfTrackEvent) EndTrack();

        binaryWriter.Flush();
        var endPosition = binaryWriter.BaseStream.Position;

        // Overwrite track count in header
        binaryWriter.BaseStream.Position = headerChunkPosition + SmfFormat.ChunkHeader.SizeInBytes + 2;
        binaryWriter.WriteBigEndian((ushort)trackCount);

        binaryWriter.Flush();
        binaryWriter.BaseStream.Position = endPosition;

        binaryWriter.Dispose();

        State = SmfSinkState.Ended;
    }

    public void Dispose() => End();

    private void Write(SmfFormat.ChunkHeader chunkHeader)
    {
        binaryWriter.WriteBigEndian((uint)chunkHeader.Type);
        binaryWriter.WriteBigEndian(chunkHeader.Length);
    }

    void ISmfSink.AddEvent(uint timeDelta, in RawEvent message)
        => Write(timeDelta, in message);

    void ISmfSink.AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte)
        => WriteChannel(timeDelta, status, firstDataByte, secondDataByte);

    void ISmfSink.AddEscapeEvent(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data)
        => WriteEscape(timeDelta, sysExPrefix, data);

    void ISmfSink.AddMetaEvent(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
        => WriteMeta(timeDelta, type, data);
}
