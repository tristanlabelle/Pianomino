using Pianomino.Formats.Midi.Messages;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Messages;

public abstract class MetaMessage : SmfMessage
{
    public override byte Status => 0xFF;
    public new MetaMessageTypeByte Type => GetMetaEventType();
    public override sealed bool IsWireCompatible => false;

    public override sealed Message ToWireMessage() => throw new NotSupportedException();

    protected abstract MetaMessageTypeByte GetMetaEventType();

    protected override SmfMessageType GetMessageType() => SmfMessageType.Meta;

    public static MetaMessage? TryCreateKnown(MetaMessageTypeByte type, ImmutableArray<byte> data, Encoding? encoding)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));
        return type switch
        {
            MetaMessageTypeByte.SequenceNumber when SequenceNumber.TryFromData(data) is SequenceNumber message => message,
            MetaMessageTypeByte.ChannelPrefix when data.Length == 1 => new ChannelPrefix((Channel)data[0]),
            MetaMessageTypeByte.Port when data.Length == 1 => new Port(data[0]),
            MetaMessageTypeByte.EndOfTrack => EndOfTrack.Instance,
            MetaMessageTypeByte.SetTempo when data.Length == SetTempo.DataLength
                => SetTempo.FromBytes(data[0], data[1], data[2]),
            MetaMessageTypeByte.SmpteOffset when data.Length == SmpteOffset.DataLength
                => new SmpteOffset(TimeCode.FromBytes(data[0], data[1], data[2], data[3], data[4])),
            MetaMessageTypeByte.TimeSignature when data.Length == TimeSignature.DataLength
                => TimeSignature.FromBytes(data[0], data[1], data[2], data[3]),
            MetaMessageTypeByte.KeySignature when data.Length == KeySignature.DataLength
                => KeySignature.FromBytes(data[0], data[1]),
            MetaMessageTypeByte.SequencerSpecific => new SequencerSpecific(data),
            _ when type.IsKnownTextMessage() => TryCreateTextEvent(type, data, encoding),
            _ => null
        };
    }

    private static TextEvent? TryCreateTextEvent(MetaMessageTypeByte type, ImmutableArray<byte> data, Encoding? encoding)
        => encoding is null ? null : new TextEvent(type, encoding.GetString(data.AsSpan()));
}

public delegate MetaMessage? MetaMessageFactory(MetaMessageTypeByte type, ImmutableArray<byte> data, Encoding? encoding);
