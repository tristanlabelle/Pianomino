using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum Channel : byte
{
    _1,
    _2,
    _3,
    _4,
    _5,
    _6,
    _7,
    _8,
    _9,
    _10,
    _11,
    _12,
    _13,
    _14,
    _15,
    _16,
}

public static class ChannelEnum
{
    public const int Count = 16;
    public const Channel Percussion = Channel._10;

    public static bool IsValid(this Channel value) => (byte)value < 0x10;
    public static bool IsPercussion(this Channel value) => value == Percussion;

    public static int ToNumber(this Channel value) => (int)value + 1;
}
