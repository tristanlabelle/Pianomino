using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SmpteOffset : MetaMessage
{
    public const int DataLength = 5;

    public TimeCode TimeCode { get; }

    public SmpteOffset(TimeCode timeCode) => this.TimeCode = timeCode;

    public override RawSmfMessage ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString() => $"SmpteOffset({TimeCode})";

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.SmpteOffset;
}
