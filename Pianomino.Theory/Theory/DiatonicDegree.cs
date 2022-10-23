using System;

namespace Pianomino.Theory;

/// <summary>
/// A degree of a seven-note scale.
/// </summary>
public enum DiatonicDegree : byte
{
    First,
    Second,
    Third,
    Fourth,
    Fifth,
    Sixth,
    Seventh
}

public static class DiatonicDegreeEnum
{
    private static readonly byte[] toChromatic = { 0, 2, 4, 5, 7, 9, 11 };
    private static readonly bool[] perfect = { true, false, false, true, true, false, false };

    public const int Count = 7;

    public static bool IsValid(this DiatonicDegree value) => (uint)value < Count;

    public static DiatonicDegree FromDelta(int value)
        => (DiatonicDegree)IntMath.EuclidianMod(value, Count);

    public static DiatonicDegree FromDelta(int value, out int octave)
    {
        var result = IntMath.EuclidianDivMod(value, Count);
        octave = result.Quotient;
        return (DiatonicDegree)result.Remainder;
    }

    public static int ToDelta(this DiatonicDegree value) => (int)value;

    public static int ToNumber(this DiatonicDegree value) => (int)value + 1;

    public static ChromaticDegree ToChromatic(this DiatonicDegree value)
        => (ChromaticDegree)toChromatic[(int)value];

    public static DiatonicDegree? TryFromRomanNumeral(string str, out bool upperCase)
    {
        upperCase = false;
        DiatonicDegree? result = str switch
        {
            "I" or "i" => DiatonicDegree.First,
            "II" or "ii" => DiatonicDegree.Second,
            "III" or "iii" => DiatonicDegree.Third,
            "IV" or "iv" => DiatonicDegree.Fourth,
            "V" or "v" => DiatonicDegree.Fifth,
            "VI" or "vi" => DiatonicDegree.Sixth,
            "VII" or "vii" => DiatonicDegree.Seventh,
            _ => null
        };
        if (result is null) return null;
        upperCase = str[0] is 'I' or 'V';
        return result;
    }

    public static DiatonicDegree? TryFromRomanNumeral(string str) => TryFromRomanNumeral(str, upperCase: out _);

    public static bool IsPerfect(this DiatonicDegree value) => perfect[(int)value];

    public static string GetRomanNumeral(this DiatonicDegree value, RomanNumeralFlags flags = default)
        => RomanNumerals.Get(value.ToNumber(), flags);

    public static DiatonicDegree Add(this DiatonicDegree value, int delta)
        => (DiatonicDegree)IntMath.EuclidianMod((int)value + delta, Count);

    public static IntervalClass Natural(this DiatonicDegree value) => new(value);
    public static IntervalClass Flat(this DiatonicDegree value) => new(value, Alteration.Flat);
    public static IntervalClass Sharp(this DiatonicDegree value) => new(value, Alteration.Sharp);
    public static IntervalClass WithAlteration(this DiatonicDegree value, Alteration alteration) => new(value, alteration);
}
