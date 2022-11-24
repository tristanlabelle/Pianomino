using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class Port : MetaMessage
{
    public byte Value { get; }

    public Port(byte value)
    {
        this.Value = value;
    }

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.Port;

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(MetaMessageTypeByte.Port, ImmutableArray.Create(Value));

    public override string ToString() => $"Port({Value})";
}
