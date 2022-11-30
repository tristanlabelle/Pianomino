using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class Port : MetaEvent
{
    public const int PayloadLength = 1;

    public byte Value { get; }

    public Port(byte value)
    {
        this.Value = value;
    }

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.Port;

    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateMeta(MetaEventTypeByte.Port, ImmutableArray.Create(Value));

    public override string ToString() => $"Port({Value})";
}
