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

    internal readonly ImmutableArray<byte> variableLengthData;
    internal readonly byte binaryNotStatus; // Binary not so it defaults to 0xFF (System Real Time Reset, data length 0)
    internal readonly ushort firstTwoDataBytes;

    private RawMessage(StatusByte status, ushort firstTwoDataBytes = 0)
    {
        this.binaryNotStatus = (byte)~status;
        this.firstTwoDataBytes = firstTwoDataBytes;
        this.variableLengthData = default;
    }

    private RawMessage(StatusByte status, ImmutableArray<byte> data)
    {
        this.binaryNotStatus = (byte)~status;
        this.variableLengthData = data;
        this.firstTwoDataBytes = MessageData.PackFirstTwoBytes(data);
    }

    internal RawMessage(StatusByte status, ImmutableArray<byte> data, ushort firstTwoDataBytes)
    {
        this.binaryNotStatus = (byte)~status;
        this.variableLengthData = data;
        this.firstTwoDataBytes = firstTwoDataBytes;
    }

    public StatusByte Status => (StatusByte)~binaryNotStatus;
    public bool IsChannelMessage => Status.IsChannelMessage();
    public bool IsNoteMessage => Status.IsNoteMessage();
    public bool IsNoteOnOffMessage => Status.IsNoteOnOffMessage();
    public bool IsEffectiveNoteOffMessage => Status.IsNoteOffMessage() || (Status.IsNoteOnMessage() && Data.SecondByteOrZero == 0);
    public bool IsEffectiveNoteOnMessage => Status.IsNoteOnMessage() && Data.SecondByteOrZero > 0;
    public bool IsSystemExclusive => Status == StatusByte.SystemExclusive;
    public MessageData Data => new(Status.GetDataLengthType(), firstTwoDataBytes, variableLengthData);
    public ImmutableArray<byte> DataArray => Data.ToImmutableArray();

    public bool AsChannelMessage(out ChannelMessageType type, out Channel channel)
        => Status.AsChannelMessage(out type, out channel);

    public NoteKey GetNoteKey() => IsNoteMessage ? (NoteKey)Data[0] : throw new InvalidOperationException();
    public Velocity GetNoteVelocityOrPressure() => IsNoteMessage ? (Velocity)Data[1] : throw new InvalidOperationException();

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

        if (Status == StatusByte.SystemExclusive || Data.Length > 2)
        {
            stringBuilder.Append(Data.Length).Append(" bytes");
        }
        else
        {
            for (int i = 0; i < Data.Length; ++i)
            {
                if (i > 0) stringBuilder.Append(", ");
                stringBuilder.Append(Data[i]);
            }
        }

        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }

    public static bool IsValidDataByte(byte value) => value < 0x80;

    public static bool AreValidDataBytes(byte firstByte, byte secondByte)
        => (firstByte | secondByte) < 0x80;

    public static bool AreValidDataBytes(MessageData bytes)
        => bytes.Length switch
        {
            0 => true,
            1 => IsValidDataByte(bytes[0]),
            2 => AreValidDataBytes(bytes[0], bytes[1]),
            _ => AreValidDataBytes(bytes.ToImmutableArray().AsSpan())
        };

    public static bool AreValidDataBytes(ReadOnlySpan<byte> bytes)
    {
        foreach (var b in bytes)
            if (!IsValidDataByte(b))
                return false;
        return true;
    }

    #region Factory Methods
    public static RawMessage Create(StatusByte status, MessageData data)
    {
        var lengthType = status.GetDataLengthType();
        if (lengthType != MessageDataLengthType.Variable && (int)lengthType != data.Length)
            throw new ArgumentException();
        return new RawMessage(status, data.byteArray, data.firstTwoBytes);
    }

    public static RawMessage Create(StatusByte status, ReadOnlySpan<byte> data)
    {
        return Create(status, new MessageData(data));
    }

    public static RawMessage Create(ChannelMessageType type, Channel channel, byte data)
        => Create(type.GetStatusByte(channel), data);

    public static RawMessage Create(ChannelMessageType type, Channel channel, byte firstDataByte, byte secondDataByte)
        => Create(type.GetStatusByte(channel), firstDataByte, secondDataByte);

    public static RawMessage Create(StatusByte status)
    {
        if (status.GetDataLengthType() != MessageDataLengthType.ZeroBytes) throw new ArgumentOutOfRangeException();
        return new RawMessage(status);
    }

    public static RawMessage Create(StatusByte status, byte data)
    {
        if (status.GetDataLengthType() != MessageDataLengthType.OneByte) throw new ArgumentOutOfRangeException();
        if (!IsValidDataByte(data)) throw new ArgumentOutOfRangeException();
        return new RawMessage(status, data);
    }

    public static RawMessage Create(StatusByte status, byte firstDataByte, byte secondDataByte)
    {
        if (status.GetDataLengthType() != MessageDataLengthType.TwoBytes)
            throw new ArgumentOutOfRangeException();
        if (!AreValidDataBytes(firstDataByte, secondDataByte))
            throw new ArgumentOutOfRangeException();
        return new RawMessage(status, (ushort)((uint)firstDataByte | ((uint)secondDataByte << 8)));
    }

    public static RawMessage Create(StatusByte status, ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException();
        if ((status.GetDataLength() ?? data.Length) != data.Length)
            throw new ArgumentOutOfRangeException();
        if (!AreValidDataBytes(data)) throw new ArgumentException();
        return new RawMessage(status, data);
    }

    public static RawMessage CreateNoteOn(NoteKey key, Velocity velocity, Channel channel = Channel._1)
        => Create(StatusByteEnum.Create(ChannelMessageType.NoteOn, channel), key.Number, velocity);

    public static RawMessage CreateNoteOffAsNoteOn(NoteKey key, Channel channel = Channel._1)
        => CreateNoteOn(key, Velocity.Off, channel);

    public static RawMessage CreateSysEx(ImmutableArray<byte> data)
    {
        if (!AreValidDataBytes(data)) throw new ArgumentException();
        return new RawMessage(StatusByte.SystemExclusive, data);
    }
    #endregion
}
