using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum MessageDataLengthType : byte
{
    ZeroBytes,
    OneByte,
    TwoBytes,
    Variable
}

public static class MessageDataLengthTypeEnum
{
    public static int? ToNullableByteCount(this MessageDataLengthType type)
        => type switch
        {
            <= MessageDataLengthType.TwoBytes => (int)type,
            MessageDataLengthType.Variable => null,
            _ => throw new ArgumentOutOfRangeException()
        };
}
