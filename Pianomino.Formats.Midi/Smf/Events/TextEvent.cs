using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class TextEvent : MetaEvent
{
    private readonly MetaEventTypeByte type;
    public string Text { get; }

    public TextEvent(MetaEventTypeByte type, string text)
    {
        if (!type.IsKnownTextMessage()) throw new ArgumentException();
        this.type = type;
        this.Text = text;
    }

    public override MetaEventTypeByte MetaType => type;

    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateMeta(type, encoding.GetBytes(Text).ToImmutableArray());

    public override string ToString() => $"{type}(\"{Text}\")";
}
