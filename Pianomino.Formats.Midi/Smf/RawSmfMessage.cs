using System;
using System.Collections.Immutable;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// A MIDI message as appears in a standard MIDI file.
/// Can be a channel message, a system exclusive message or a meta message.
/// </summary>
public readonly struct RawSmfMessage
{
    public const byte Status_SysEx = 0xF0;
    public const byte Status_Escape = 0xF7;
    public const byte Status_Meta = 0xFF;

    public static RawSmfMessage EndOfTrack => new(MetaMessageTypeByte.EndOfTrack, ImmutableArray<byte>.Empty);

    private readonly ImmutableArray<byte> variableLengthData;
    private readonly byte status;
    private readonly byte fileMessageTypeOrMetaType;
    private readonly ushort firstTwoDataBytes;

    private readonly struct ChannelTag { }
    private RawSmfMessage(ChannelTag _, StatusByte status, ushort firstTwoDataBytes, ImmutableArray<byte> data = default)
    {
        this.variableLengthData = data;
        this.status = (byte)status;
        this.fileMessageTypeOrMetaType = (byte)SmfMessageType.Channel;
        this.firstTwoDataBytes = firstTwoDataBytes;
    }

    private RawSmfMessage(MetaMessageTypeByte metaType, ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));

        this.variableLengthData = data;
        this.status = Status_Meta;
        this.fileMessageTypeOrMetaType = (byte)metaType;
        this.firstTwoDataBytes = MessageData.PackFirstTwoBytes(data);
    }

    private readonly struct SysExEscapeTag { }
    private RawSmfMessage(SysExEscapeTag _, SmfMessageType type, ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));

        this.variableLengthData = data;
        this.status = type == SmfMessageType.SysEx_Unit || type == SmfMessageType.SysEx_BeginPacket
            ? Status_SysEx : Status_Escape;
        this.fileMessageTypeOrMetaType = (byte)type;
        this.firstTwoDataBytes = MessageData.PackFirstTwoBytes(data);
    }

    public SmfMessageType Type => status == Status_Meta ? SmfMessageType.Meta : (SmfMessageType)fileMessageTypeOrMetaType;
    public bool IsChannel => Type == SmfMessageType.Channel;
    public bool IsMeta => Type == SmfMessageType.Meta;

    public bool IsWireCompatible => Type.IsWireCompatible();
    public byte Status => status;
    public MessageData Data => IsChannel
        ? new(((StatusByte)status).GetDataLengthType(), firstTwoDataBytes, variableLengthData)
        : new(variableLengthData);
    public ImmutableArray<byte> DataArray => Data.ToImmutableArray();

    public MetaMessageTypeByte GetMetaType() => IsMeta
        ? (MetaMessageTypeByte)fileMessageTypeOrMetaType
        : throw new InvalidOperationException();
    public StatusByte GetWireStatus() => IsWireCompatible
        ? (StatusByte)status : throw new InvalidOperationException();

    public RawMessage ToWireMessage() => IsWireCompatible
        ? new((StatusByte)status, variableLengthData, firstTwoDataBytes)
        : throw new NotImplementedException();

    public static RawSmfMessage FromWireMessage(in RawMessage wireMessage)
    {
        if (wireMessage.IsChannelMessage)
            return new(default(ChannelTag), wireMessage.Status, wireMessage.firstTwoDataBytes, wireMessage.variableLengthData);
        else if (wireMessage.IsSystemExclusive)
            return new(default(SysExEscapeTag), SmfMessageType.SysEx_Unit, wireMessage.variableLengthData);
        else
            throw new ArgumentException(message: null, paramName: nameof(wireMessage));
    }

    public static RawSmfMessage CreateChannel(StatusByte status, byte firstDataByte, byte secondDataByte = 0)
    {
        if (!status.IsChannelMessage()) throw new ArgumentOutOfRangeException(nameof(status));
        if (!RawMessage.IsValidDataByte(firstDataByte)) throw new ArgumentOutOfRangeException(nameof(firstDataByte));
        if (status.GetDataLengthType() == MessageDataLengthType.TwoBytes && !RawMessage.IsValidDataByte(secondDataByte))
            throw new ArgumentOutOfRangeException(nameof(secondDataByte));

        return new(default(ChannelTag), status, MessageData.PackFirstTwoBytes(firstDataByte, secondDataByte));
    }

    public static RawSmfMessage CreateSystemExclusive(ImmutableArray<byte> data, bool first, bool last)
        => new(default(SysExEscapeTag), SmfMessageTypeEnum.GetSysEx(first, last), data);

    public static RawSmfMessage CreateEscape(ImmutableArray<byte> data)
        => new(default(SysExEscapeTag), SmfMessageType.Escape, data);

    public static RawSmfMessage CreateMeta(MetaMessageTypeByte type, ImmutableArray<byte> data)
        => new(type, data);
}
