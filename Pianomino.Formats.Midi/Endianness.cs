using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

internal static class Endianness
{
    public static ushort SwapBytes(ushort value)
    {
        return unchecked((ushort)((value << 8) | (value >> 8)));
    }

    public static uint SwapBytes(uint value)
    {
        return (value << 24) | ((value & 0xFF00) << 8) | ((value & 0xFF0000) >> 8) | (value >> 24);
    }
}
