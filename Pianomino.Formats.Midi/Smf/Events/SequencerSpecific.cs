using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class SequencerSpecific : MetaEvent
{
    public ImmutableArray<byte> Data { get; }

    public SequencerSpecific(ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException(message: null, paramName: nameof(data));
        this.Data = data;
    }

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.SequencerSpecific;
    public ManufacturerId? ManufacturerId => Midi.ManufacturerId.TryFromData(Data.AsSpan());

    public override RawEvent ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("SequencerSpecific(");
        AppendBytesString(stringBuilder, Data);
        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }
}
