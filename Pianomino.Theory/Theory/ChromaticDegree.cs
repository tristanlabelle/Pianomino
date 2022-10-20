using System;

namespace Pianomino.Theory;

/// <summary>
/// A degree of the 12-note chromatic scale.
/// </summary>
public enum ChromaticDegree : byte
{
    P1, m2, M2, m3, M3, P4, TT, P5, m6, M6, m7, M7
}

public static class ChromaticDegreeEnum
{
    private static readonly byte[] diatonicFloor = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };
    private static readonly byte[] diatonicCeiling = { 0, 1, 1, 2, 2, 3, 4, 4, 5, 5, 6, 6 };
    private static readonly bool[] diatonicLookup = { true, false, true, false, true, true, false, true, false, true, false, true };

    public const int Count = 12;

    public static ChromaticDegree FromDelta(int value)
        => (ChromaticDegree)IntMath.EuclidianMod(value, Count);

    public static ChromaticDegree FromDelta(int value, out int octave)
    {
        var result = IntMath.EuclidianDivMod(value, Count);
        octave = result.Quotient;
        return (ChromaticDegree)result.Remainder;
    }

    public static int ToDelta(this ChromaticDegree value) => (int)value;

    public static bool IsDiatonic(this ChromaticDegree value) => diatonicLookup[(int)value];

    public static ChromaticDegree DiatonicFloor(this ChromaticDegree value)
        => IsDiatonic(value) ? value : (ChromaticDegree)((int)value - 1);

    public static ChromaticDegree DiatonicCeiling(this ChromaticDegree value)
        => IsDiatonic(value) ? value : (ChromaticDegree)((int)value + 1);

    public static DiatonicDegree DiatonicStepFloor(this ChromaticDegree value)
        => (DiatonicDegree)diatonicFloor[(int)value];

    public static DiatonicDegree DiatonicStepCeiling(this ChromaticDegree value)
        => (DiatonicDegree)diatonicCeiling[(int)value];

    public static ChromaticDegree Add(this ChromaticDegree value, ChromaticDegree delta) => Add(value, (int)delta);

    public static ChromaticDegree Subtract(this ChromaticDegree value, ChromaticDegree delta) => Subtract(value, (int)delta);

    public static ChromaticDegree Add(this ChromaticDegree value, int semitones)
        => (ChromaticDegree)IntMath.EuclidianMod((int)value + semitones, Count);

    public static ChromaticDegree Subtract(this ChromaticDegree value, int semitones)
        => (ChromaticDegree)IntMath.EuclidianMod((int)value - semitones, Count);

    public static ChromaticDegree ToNegativeHarmony(this ChromaticDegree value)
        => ChromaticDegree.P5.Subtract(value);
}
