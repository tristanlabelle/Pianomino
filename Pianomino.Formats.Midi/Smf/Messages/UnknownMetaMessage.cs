using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class UnknownMetaMessage : MetaMessage
{
    public new MetaMessageTypeByte Type { get; }
    public ImmutableArray<byte> Data { get; }

    public UnknownMetaMessage(MetaMessageTypeByte type, ImmutableArray<byte> data)
    {
        this.Type = type;
        this.Data = data;
    }

    public override RawSmfMessage ToRaw(Encoding encoding) => RawSmfMessage.CreateMeta(Type, Data);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Meta.");
        stringBuilder.AppendFormat("{0:X2}", (byte)Type);
        if (Data.Length > 0)
        {
            stringBuilder.Append('(');
            AppendBytesString(stringBuilder, Data);
            stringBuilder.Append(')');
        }

        return stringBuilder.ToString();
    }

    protected override MetaMessageTypeByte GetMetaEventType() => Type;
}
