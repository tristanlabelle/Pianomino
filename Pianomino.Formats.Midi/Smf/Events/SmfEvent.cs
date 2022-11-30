using System;
using System.Collections.Immutable;
using System.Text;
using Pianomino.Formats.Midi.Messages;

namespace Pianomino.Formats.Midi.Smf.Events;

/// <summary>
/// Base class for immutable standard midi file events, without the timestamp.
/// </summary>
public abstract class SmfEvent
{
    public abstract EventHeaderByte HeaderByte { get; }
    public abstract bool IsWireCompatible { get; }

    public abstract Message ToMessage();
    public abstract RawEvent ToRaw(Encoding encoding);

    public RawEvent ToRaw() => ToRaw(Encoding.UTF8);

    public abstract override string ToString();

    public static SmfEvent? TryFromWireMessage(Message message) => message switch
    {
        ChannelMessage channelMessage => new ChannelEvent(channelMessage),
        SysExMessage sysExMessage => new SysExEvent(sysExMessage),
        _ => null
    };

    public static SmfEvent Create(in RawEvent @event,
        SysExMessageFactory sysExFactory, MetaMessageFactory metaFactory, Encoding encoding)
    {
        if (@event.HeaderByte.IsChannelStatus())
            return new ChannelEvent((ChannelMessage)Message.Create(@event.ToMessage(), sysExFactory, encoding));
        if (@event.HeaderByte.IsEscape())
            return new EscapeEvent(sysExPrefix: @event.HeaderByte == EventHeaderByte.Escape_SysEx, @event.Payload);
        if (@event.HeaderByte == EventHeaderByte.Meta)
            return CreateMeta(@event.GetMetaType(), @event.Payload, metaFactory, encoding);
        throw new UnreachableException();
    }

    public static SmfEvent Create(in RawEvent message, Encoding encoding)
        => Create(message, SysExMessage.TryCreateKnown, MetaEvent.TryCreateKnown, encoding);

    private static MetaEvent CreateMeta(MetaEventTypeByte metaType, ImmutableArray<byte> payload, MetaMessageFactory factory, Encoding encoding)
    {
        return factory(metaType, payload, encoding) ?? new UnknownMetaEvent(metaType, payload);
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
