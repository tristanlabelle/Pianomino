using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pianomino.Formats.Midi.Messages;

namespace Pianomino.Formats.Midi.Smf.Messages;

public abstract class SmfMessage
{
    public abstract byte Status { get; }
    public SmfMessageType Type => GetMessageType();
    public abstract bool IsWireCompatible { get; }

    public abstract Message ToWireMessage();
    public abstract RawSmfMessage ToRaw(Encoding encoding);

    public RawSmfMessage ToRaw() => ToRaw(Encoding.UTF8);

    public abstract override string ToString();

    protected abstract SmfMessageType GetMessageType();

    public static SmfMessage? TryFromWireMessage(Message message) => message switch
    {
        ChannelMessage channelMessage => new SmfChannelMessage(channelMessage),
        SysExMessage sysExMessage => new SysExUnit(sysExMessage),
        _ => null
    };

    public static SmfMessage Create(in RawSmfMessage message,
        SysExMessageFactory sysExFactory, MetaMessageFactory metaFactory, Encoding encoding)
    {
        return message.Type switch
        {
            SmfMessageType.Channel => new SmfChannelMessage(
                (ChannelMessage)Message.Create(message.ToWireMessage(), sysExFactory, encoding)),
            SmfMessageType.SysEx_Unit => CreateSysExUnit(message.DataArray, sysExFactory, encoding),
            SmfMessageType.Meta => CreateMeta(message.GetMetaType(), message.DataArray, metaFactory, encoding),
            _ => throw new NotImplementedException("Packeted SysEx & Escapes")
        };
    }

    public static SmfMessage Create(in RawSmfMessage message, Encoding encoding)
        => Create(message, SysExMessage.TryCreateKnown, MetaMessage.TryCreateKnown, encoding);

    private static SysExUnit CreateSysExUnit(ImmutableArray<byte> data, SysExMessageFactory factory, Encoding encoding)
    {
        var wireMessage = factory(ManufacturerId.TryFromData(data.AsSpan()), data, encoding) ?? new UnknownSysExMessage(data);
        return new SysExUnit(wireMessage);
    }

    private static MetaMessage CreateMeta(MetaMessageTypeByte metaType, ImmutableArray<byte> data, MetaMessageFactory factory, Encoding encoding)
    {
        return factory(metaType, data, encoding) ?? new UnknownMetaMessage(metaType, data);
    }

    protected static void AppendBytesString(StringBuilder stringBuilder, ImmutableArray<byte> bytes)
    {
        bool first = true;
        foreach (var b in bytes)
        {
            if (!first) stringBuilder.Append(' ');
            stringBuilder.AppendFormat("{0:X2}", b);
            first = false;
        }
    }
}
