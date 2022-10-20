using System;

namespace Pianomino;

internal static class IntMath
{
    public static int Clamp(int value, int min, int inclusiveMax)
    {
        if (value < min) return min;
        if (value > inclusiveMax) return inclusiveMax;
        return value;
    }

    #region Powers of two
    public static bool IsPowerOfTwo(uint value) => Bits.IsSingle(value);
    public static bool IsPowerOfTwo(int value) => value > 0 && IsPowerOfTwo((uint)value);

    public static int Log2(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        return Bits.IndexOfMostSignificant(value);
    }
    #endregion

    #region GCD/LCM
    public static uint GreatestCommonDivisor(uint first, uint second)
    {
        while (second != 0) (first, second) = (second, first % second);
        return first;
    }

    public static ulong GreatestCommonDivisor(ulong first, ulong second)
    {
        while (second != 0) (first, second) = (second, first % second);
        return first;
    }

    public static int GreatestCommonDivisor(int first, int second)
        => (int)GreatestCommonDivisor((uint)Math.Abs(first), (uint)Math.Abs(second));

    public static uint LeastCommonMultiple(uint first, uint second)
    {
        return (uint)((ulong)first * (ulong)second / GreatestCommonDivisor(first, second));
    }

    public static int LeastCommonMultiple(int first, int second)
    {
        if (first < 0) throw new ArgumentOutOfRangeException(nameof(first));
        if (second < 0) throw new ArgumentOutOfRangeException(nameof(second));
        return (int)LeastCommonMultiple((uint)first, (uint)second);
    }

    public static bool AreRelativelyPrime(int first, int second)
        => first > 1 && second > 1 && GreatestCommonDivisor(first, second) == 1;
    #endregion

    #region Euclidian division
    public static int EuclidianDiv(int dividend, int divisor)
    {
        if (divisor < 0) throw new NotImplementedException();
        return dividend >= 0
            ? dividend / divisor
            : (dividend + 1) / divisor - 1;
    }

    public static int EuclidianMod(int dividend, int divisor)
    {
        if (divisor < 0) throw new NotImplementedException();
        return dividend >= 0
            ? dividend % divisor
            : (divisor - 1) + (dividend + 1) % divisor;
    }

    public static (int Quotient, int Remainder) EuclidianDivMod(int dividend, int divisor)
        => (EuclidianDiv(dividend, divisor), EuclidianMod(dividend, divisor));

    public static int EuclidianModDistance(int dividend1, int dividend2, int divisor)
        => Math.Min(EuclidianMod(dividend1 - dividend2, divisor), EuclidianMod(dividend2 - dividend1, divisor));

    public static int EuclidianModDelta(int dividend1, int dividend2, int divisor)
    {
        var a = EuclidianMod(dividend1 - dividend2, divisor);
        var b = EuclidianMod(dividend2 - dividend1, divisor);
        return a < b ? -a : b;
    }
    #endregion

    public static int GetHashCode(int a, int b)
        => unchecked((int)(((uint)a + 23) * 31 + (uint)b));

    public static int GetHashCode(int a, int b, int c)
        => unchecked((int)((((uint)a * 23) * 31 + (uint)b) * 31 + (uint)c));

    public static int GetHashCode(int a, int b, int c, int d)
        => unchecked((int)(((((uint)a * 23) * 31 + (uint)b) * 31 + (uint)c) * 31 + (uint)d));
}
