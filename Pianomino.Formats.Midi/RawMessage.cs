using Pianomino.Theory;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Pianomino.Formats.Midi;

/// <summary>
/// A byte-level representation of a MIDI message (channel or system) as transmitted over the wire.
/// </summary>
/// <remarks>
/// Implementation goals:
/// - Immutable
/// - Compact value type
/// - Default value is valid
/// - Allocations only for system exclusive messages
/// - Round-trippable representation from byte stream
/// - Uniform data byte access (allocation or no allocation)
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplayString()}")]
public readonly struct RawMessage
{
    public static RawMessage EndOfExclusive => new(StatusByte.EndOfExclusive);
    public static RawMessage Clock => new(StatusByte.Clock);
    public static RawMessage Start => new(StatusByte.Start);
    public static RawMessage Continue => new(StatusByte.Continue);
    public static RawMessage Stop => new(StatusByte.Stop);
    public static RawMessage ActiveSensing => new(StatusByte.ActiveSensing);
    public static RawMessage Reset => new(StatusByte.Reset);

    internal readonly ImmutableArray<byte> variableLengthPayload;
    internal readonly byte status_binaryNot; // Binary not so it defaults to 0xFF (System Real Time Reset, data length 0)
    internal readonly ushort firstTwoPayloadBytes;

    private RawMessage(StatusByte status, ushort firstTwoPayloadBytes = 0)
    {
        this.status_binaryNot = (byte)~status;
        this.firstTwoPayloadBytes = firstTwoPayloadBytes;
        this.variableLengthPayload = default;
    }

    private RawMessage(StatusByte status, ImmutableArray<byte> payload)
    {
        this.status_binaryNot = (byte)~status;
        this.variableLengthPayload = payload;
        this.firstTwoPayloadBytes = ShortPayload.PackFirstTwoBytes(payload);
    }

    internal RawMessage(StatusByte status, ImmutableArray<byte> payload, ushort firstTwoPayloadBytes)
    {
        this.status_binaryNot = (byte)~status;
        this.variableLengthPayload = payload;
        this.firstTwoPayloadBytes = firstTwoPayloadBytes;
    }

    public StatusByte Status => (StatusByte)~status_binaryNot;
    public bool IsChannelMessage => Status.IsChannelMessage();
    public bool IsNoteMessage => Status.IsNoteMessage();
    public bool IsNoteOnOffMessage => Status.IsNoteOnOffMessage();
    public bool IsEffectiveNoteOffMessage => Status.IsNoteOffMessage() || (Status.IsNoteOnMessage() && Payload.SecondByteOrZero == 0);
    public bool IsEffectiveNoteOnMessage => Status.IsNoteOnMessage() && Payload.SecondByteOrZero > 0;
    public bool IsSystemExclusive => Status == StatusByte.SystemExclusive;
    public ShortPayload Payload => new(Status.GetPayloadLengthType(), firstTwoPayloadBytes, variableLengthPayload);

    public bool AsChannelMessage(out ChannelMessageType type, out Channel channel)
        => Status.AsChannelMessage(out type, out channel);

    public NoteKey GetNoteKey() => IsNoteMessage ? (NoteKey)Payload[0] : throw new InvalidOperationException();
    public Velocity GetNoteVelocityOrPressure() => IsNoteMessage ? (Velocity)Payload[1] : throw new InvalidOperationException();

    internal string GetDebuggerDisplayString()
    {
        if (Status != StatusByte.SystemExclusive)
        {
            return Messages.Message.Create(this, encoding: Encoding.UTF8).ToString();
        }

        var stringBuilder = new StringBuilder();
        if (AsChannelMessage(out var type, out var channel))
        {
            stringBuilder.Append(type.ToString())
                .Append("(ch")
                .Append(channel.ToNumber())
                .Append(", ");
        }
        else
        {
            stringBuilder.Append(Status.ToString()).Append('(');
        }

        if (Status == StatusByte.SystemExclusive || Payload.Length > 2)
        {
            stringBuilder.Append(Payload.Length).Append(" bytes");
        }
        else
        {
            for (int i = 0; i < Payload.Length; ++i)
            {
                if (i > 0) stringBuilder.Append(", ");
                stringBuilder.Append(Payload[i]);
            }
        }

        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }

    public static bool IsValidPayloadByte(byte value) => value < 0x80;

    public static bool AreValidPayloadBytes(byte firstByte, byte secondByte)
        => (firstByte | secondByte) < 0x80;

    public static bool AreValidPayloadBytes(ShortPayload bytes)
        => bytes.Length switch
        {
            0 => true,
            1 => IsValidPayloadByte(bytes[0]),
            2 => AreValidPayloadBytes(bytes[0], bytes[1]),
            _ => AreValidPayloadBytes(bytes.AsSpan())
        };

    public static bool AreValidPayloadBytes(ReadOnlySpan<byte> bytes)
    {
        foreach (var b in bytes)
            if (!IsValidPayloadByte(b))
                return false;
        return true;
    }

    #region Factory Methods
    public static RawMessage Create(StatusByte status, ShortPayload data)
    {
        var lengthType = status.GetPayloadLengthType();
        if (lengthType != ShortPayloadLengthType.Variable && (int)lengthType != data.Length)
            throw new ArgumentException();
        return new RawMessage(status, data.byteArray, data.firstTwoBytes);
    }

    public static RawMessage Create(StatusByte status, ReadOnlySpan<byte> data)
    {
        return Create(status, new ShortPayload(data));
    }

    public static RawMessage Create(ChannelMessageType type, Channel channel, byte data)
        => Create(type.GetStatusByte(channel), data);

    public static RawMessage Create(ChannelMessageType type, Channel channel, byte firstDataByte, byte secondDataByte)
        => Create(type.GetStatusByte(channel), firstDataByte, secondDataByte);

    public static RawMessage Create(StatusByte status)
    {
        if (status.GetPayloadLengthType() != ShortPayloadLengthType.ZeroBytes) throw new ArgumentOutOfRangeException();
        return new RawMessage(status);
    }

    public static RawMessage Create(StatusByte status, byte data)
    {
        if (status.GetPayloadLengthType() != ShortPayloadLengthType.OneByte) throw new ArgumentOutOfRangeException();
        if (!IsValidPayloadByte(data)) throw new ArgumentOutOfRangeException();
        return new RawMessage(status, data);
    }

    public static RawMessage Create(StatusByte status, byte firstDataByte, byte secondDataByte)
    {
        if (status.GetPayloadLengthType() != ShortPayloadLengthType.TwoBytes)
            throw new ArgumentOutOfRangeException();
        if (!AreValidPayloadBytes(firstDataByte, secondDataByte))
            throw new ArgumentOutOfRangeException();
        return new RawMessage(status, (ushort)((uint)firstDataByte | ((uint)secondDataByte << 8)));
    }

    public static RawMessage Create(StatusByte status, ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException();
        if ((status.GetPayloadLength() ?? data.Length) != data.Length)
            throw new ArgumentOutOfRangeException();
        if (!AreValidPayloadBytes(data)) throw new ArgumentException();
        return new RawMessage(status, data);
    }

    public static RawMessage CreateNoteOn(NoteKey key, Velocity velocity, Channel channel = Channel._1)
        => Create(StatusByteEnum.Create(ChannelMessageType.NoteOn, channel), key.Number, velocity);

    public static RawMessage CreateNoteOffAsNoteOn(NoteKey key, Channel channel = Channel._1)
        => CreateNoteOn(key, Velocity.Off, channel);

    public static RawMessage CreateSysEx(ImmutableArray<byte> data)
    {
        if (!AreValidPayloadBytes(data)) throw new ArgumentException();
        return new RawMessage(StatusByte.SystemExclusive, data);
    }
    #endregion
}
