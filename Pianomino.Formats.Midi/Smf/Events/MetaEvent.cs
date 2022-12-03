using Pianomino.Formats.Midi.Messages;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public abstract class MetaEvent : Event
{
    public override EventHeaderByte HeaderByte => EventHeaderByte.Meta;
    public abstract MetaEventTypeByte MetaType { get; }
    public override sealed bool IsWireCompatible => false;

    public override sealed Message ToMessage() => throw new NotSupportedException();

    public static MetaEvent? TryCreateKnown(MetaEventTypeByte type, ImmutableArray<byte> data, Encoding? encoding)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));
        return type switch
        {
            MetaEventTypeByte.SequenceNumber when SequenceNumber.TryFromData(data) is SequenceNumber message => message,
            MetaEventTypeByte.ChannelPrefix when data.Length == 1 => new ChannelPrefix((Channel)data[0]),
            MetaEventTypeByte.Port when data.Length == 1 => new Port(data[0]),
            MetaEventTypeByte.EndOfTrack => EndOfTrack.Instance,
            MetaEventTypeByte.SetTempo when data.Length == SetTempo.PayloadLength
                => SetTempo.FromBytes(data[0], data[1], data[2]),
            MetaEventTypeByte.SmpteOffset when data.Length == SmpteOffset.PayloadLength
                => new SmpteOffset(TimeCode.FromBytes(data[0], data[1], data[2], data[3], data[4])),
            MetaEventTypeByte.TimeSignature when data.Length == TimeSignature.PayloadLength
                => TimeSignature.FromBytes(data[0], data[1], data[2], data[3]),
            MetaEventTypeByte.KeySignature when data.Length == KeySignature.PayloadLength
                => KeySignature.FromBytes(data[0], data[1]),
            MetaEventTypeByte.SequencerSpecific => new SequencerSpecific(data),
            _ when type.IsKnownTextMessage() => TryCreateTextEvent(type, data, encoding),
            _ => null
        };
    }

    private static TextEvent? TryCreateTextEvent(MetaEventTypeByte type, ImmutableArray<byte> data, Encoding? encoding)
        => encoding is null ? null : new TextEvent(type, encoding.GetString(data.AsSpan()));
}

public delegate MetaEvent? MetaMessageFactory(MetaEventTypeByte type, ImmutableArray<byte> data, Encoding? encoding);
