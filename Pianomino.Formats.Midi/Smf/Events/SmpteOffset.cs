using System;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class SmpteOffset : MetaEvent
{
    public const int PayloadLength = 5;

    public TimeCode TimeCode { get; }

    public SmpteOffset(TimeCode timeCode) => this.TimeCode = timeCode;

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.SmpteOffset;

    public override RawEvent ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString() => $"SmpteOffset({TimeCode})";
}
