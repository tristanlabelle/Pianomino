using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class TextEvent : MetaMessage
{
    public new MetaMessageTypeByte Type { get; }
    public string Text { get; }

    public TextEvent(MetaMessageTypeByte type, string text)
    {
        if (!type.IsKnownTextMessage()) throw new ArgumentException();
        this.Type = type;
        this.Text = text;
    }

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(Type, encoding.GetBytes(Text).ToImmutableArray());

    public override string ToString() => $"{Type}(\"{Text}\")";

    protected override MetaMessageTypeByte GetMetaEventType() => Type;
}
