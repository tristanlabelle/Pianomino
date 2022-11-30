using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class UnknownMetaEvent : MetaEvent
{
    private readonly MetaEventTypeByte type;
    public ImmutableArray<byte> Payload { get; }

    public UnknownMetaEvent(MetaEventTypeByte type, ImmutableArray<byte> payload)
    {
        this.type = type;
        this.Payload = payload;
    }

    public override MetaEventTypeByte MetaType => type;

    public override RawEvent ToRaw(Encoding encoding) => RawEvent.CreateMeta(type, Payload);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Meta.");
        stringBuilder.AppendFormat("{0:X2}", (byte)type);
        if (Payload.Length > 0)
        {
            stringBuilder.Append('(');
            AppendBytesString(stringBuilder, Payload);
            stringBuilder.Append(')');
        }

        return stringBuilder.ToString();
    }
}
