using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SequencerSpecific : MetaMessage
{
    public ImmutableArray<byte> Data { get; }

    public SequencerSpecific(ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));
        this.Data = data;
    }

    public ManufacturerId? ManufacturerId => Midi.ManufacturerId.TryFromData(Data.AsSpan());

    public override RawSmfMessage ToRaw(Encoding encoding) => throw new NotImplementedException();

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.SequencerSpecific;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("SequencerSpecific(");
        AppendBytesString(stringBuilder, Data);
        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }
}
