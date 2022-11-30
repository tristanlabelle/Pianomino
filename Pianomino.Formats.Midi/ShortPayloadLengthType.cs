using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum ShortPayloadLengthType : byte
{
    ZeroBytes, // Default value
    OneByte,
    TwoBytes,
    Variable
}

public static class ShortPayloadLengthTypeEnum
{
    public static int? ToNullableByteCount(this ShortPayloadLengthType type)
        => type < ShortPayloadLengthType.Variable ? (int)type : null;
}
