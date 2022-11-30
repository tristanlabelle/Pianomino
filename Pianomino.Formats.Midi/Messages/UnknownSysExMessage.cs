using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Messages;

public sealed class UnknownSysExMessage : SysExMessage
{
    public ImmutableArray<byte> Data { get; }

    public UnknownSysExMessage(ImmutableArray<byte> data)
    {
        if (!RawMessage.AreValidPayloadBytes(data)) throw new ArgumentException(message: null, paramName: nameof(data));
        this.Data = data;
    }

    public override ManufacturerId? TryGetManufacturerId() => Midi.ManufacturerId.TryFromData(Data.AsSpan());
    public override RawMessage ToRaw(Encoding encoding) => RawMessage.CreateSysEx(Data);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("SysEx");
        stringBuilder.Append('(');
        AppendBytesString(stringBuilder, Data);
        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }
}
