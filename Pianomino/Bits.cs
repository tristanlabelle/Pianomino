using System;

namespace Pianomino;

internal static class Bits
{
    private static readonly int[] oneCounts = { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

    public static bool IsSingle(uint value)
        => value != 0 && (value & (value - 1)) == 0;
    public static bool IsSingle(int value) => IsSingle((uint)value);

    public static int CountOnes(byte value) => oneCounts[value & 0xF] + oneCounts[value >> 4];
    public static int CountOnes(sbyte value) => CountOnes((byte)value);
    public static int CountOnes(ushort value) => CountOnes((byte)value) + CountOnes((byte)(value >> 8));
    public static int CountOnes(short value) => CountOnes((ushort)value);
    public static int CountOnes(uint value) => CountOnes((ushort)value) + CountOnes((ushort)(value >> 16));
    public static int CountOnes(int value) => CountOnes((uint)value);
    public static int CountOnes(ulong value) => CountOnes((uint)value) + CountOnes((uint)(value >> 32));
    public static int CountOnes(long value) => CountOnes((ulong)value);

    public static int CountTrailingZeroes(uint value)
    {
        if (value == 0) return 32;
        if ((value & 1) == 1) return 0;

        // Bisection method
        int count = 0;
        int shift = 32 >> 1;
        uint mask = uint.MaxValue >> shift;
        while (shift != 0)
        {
            if ((value & mask) == 0)
            {
                value >>= shift;
                count |= shift;
            }
            shift >>= 1;
            mask >>= shift;
        }
        return count;
    }

    public static int CountTrailingZeroes(int value) => CountTrailingZeroes((uint)value);

    public static int IndexOfMostSignificant(uint value)
    {
        if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

        int index = 0;
        while (true)
        {
            value >>= 1;
            if (value == 0) return index;
            index++;
        }
    }
    public static int IndexOfMostSignificant(int value) => IndexOfMostSignificant((uint)value);
}
