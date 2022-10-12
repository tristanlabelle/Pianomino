using System;

namespace Pianomino.Theory;

/// <summary>
/// A mathematical abstraction for diatonic notes and intervals,
/// defined from a number of diatonic steps and a number of semitones.
/// Comparisons are lexicographical on the (steps, semitones) tuple.
/// </summary>
public readonly struct Interval : IEquatable<Interval>, IComparable<Interval>
{
    public static readonly Interval Unison = new(0, 0);
    public static readonly Interval MinorSecond = new(1, 1);
    public static readonly Interval MajorSecond = new(1, 2);
    public static readonly Interval AugmentedSecond = new(1, 3);
    public static readonly Interval MinorThird = new(2, 3);
    public static readonly Interval MajorThird = new(2, 4);
    public static readonly Interval PerfectFourth = new(3, 5);
    public static readonly Interval DiminishedFourth = new(3, 5);
    public static readonly Interval AugmentedFourth = new(3, 6);
    public static readonly Interval DiminishedFifth = new(4, 6);
    public static readonly Interval PerfectFifth = new(4, 7);
    public static readonly Interval AugmentedFifth = new(4, 8);
    public static readonly Interval MinorSixth = new(5, 8);
    public static readonly Interval MajorSixth = new(5, 9);
    public static readonly Interval AugmentedSixth = new(5, 10);
    public static readonly Interval DiminishedSeventh = new(6, 9);
    public static readonly Interval MinorSeventh = new(6, 10);
    public static readonly Interval MajorSeventh = new(6, 11);
    public static readonly Interval Octave = new(7, 12);

    private readonly sbyte diatonicDelta; // TODO: Fix naming. Inaccurate for descending intervals.
    private readonly sbyte chromaticDelta;

    private Interval(sbyte diatonicDelta, sbyte chromaticDelta)
    {
        this.diatonicDelta = diatonicDelta;
        this.chromaticDelta = chromaticDelta;
    }

    public int DiatonicDelta => diatonicDelta;
    public int ChromaticDelta => chromaticDelta;
    public DiatonicDegree DiatonicDegree => DiatonicDegreeEnum.FromDelta(DiatonicDelta);
    public ChromaticDegree ChromaticDegree => ChromaticDegreeEnum.FromDelta(ChromaticDelta);

    public bool IsAscending => diatonicDelta >= 0 || (diatonicDelta == 0 && chromaticDelta >= 0);
    public Alteration Alteration => AlterationEnum.FromChromaticDelta(IsAscending
        ? chromaticDelta - DiatonicToChromatic(diatonicDelta)
        : -DiatonicToChromatic(-diatonicDelta) - chromaticDelta);
    public IntervalQuality Quality => DiatonicDegree.IsPerfect()
        ? IntervalQualityEnum.FromPerfectAlteration(Alteration)
        : IntervalQualityEnum.FromMajorAlteration(Alteration);

    public IntervalClass Class
    {
        get
        {
            var degree = DiatonicDegreeEnum.FromDelta(DiatonicDelta, out int octave);
            return new(degree, AlterationEnum.FromChromaticDelta(
                (ChromaticDelta - octave * ChromaticDegreeEnum.Count) - degree.ToChromatic().ToDelta()));
        }
    }

    public string ShortName
    {
        get
        {
            string result = Quality.GetPrefix() + (Math.Abs(DiatonicDelta) + 1);
            return IsAscending ? result : '-' + result;
        }
    }

    public bool Equals(Interval other) => diatonicDelta == other.diatonicDelta && chromaticDelta == other.chromaticDelta;
    public override bool Equals(object? obj) => obj is Interval other && Equals(other);
    public static bool Equals(Interval lhs, Interval rhs) => lhs.Equals(rhs);
    public static bool operator ==(Interval lhs, Interval rhs) => Equals(lhs, rhs);
    public static bool operator !=(Interval lhs, Interval rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => ((int)diatonicDelta << 8) ^ chromaticDelta;
    public override string ToString() => ShortName;

    public int CompareTo(Interval other) => diatonicDelta == other.diatonicDelta ? chromaticDelta.CompareTo(other.chromaticDelta) : diatonicDelta.CompareTo(other.diatonicDelta);
    public static int Compare(Interval lhs, Interval rhs) => lhs.CompareTo(rhs);
    public static bool operator <(Interval lhs, Interval rhs) => Compare(lhs, rhs) < 0;
    public static bool operator <=(Interval lhs, Interval rhs) => Compare(lhs, rhs) <= 0;
    public static bool operator >(Interval lhs, Interval rhs) => Compare(lhs, rhs) > 0;
    public static bool operator >=(Interval lhs, Interval rhs) => Compare(lhs, rhs) >= 0;

    public static int DiatonicToChromatic(int value)
    {
        var diatonicDegree = DiatonicDegreeEnum.FromDelta(value, out int octave);
        return octave * ChromaticDegreeEnum.Count + diatonicDegree.ToChromatic().ToDelta();
    }

    public static Interval FromDiatonicChromaticDeltas(int diatonic, int chromatic)
        => checked(new((sbyte)diatonic, (sbyte)chromatic));

    public static Interval FromChromaticDelta(int value, bool preferSharp)
    {
        var chromaticDegree = ChromaticDegreeEnum.FromDelta(value, out int octave);
        var steps = octave * DiatonicDegreeEnum.Count
            + (preferSharp ? chromaticDegree.DiatonicStepFloor() : chromaticDegree.DiatonicStepCeiling()).ToDelta();
        return FromDiatonicChromaticDeltas(steps, value);
    }

    public static Interval FromChromaticDeltaWithDefaultSpelling(int value, bool augmentedFourthOverDiminishedFifth = true)
    {
        var chromaticDegree = ChromaticDegreeEnum.FromDelta(value, out int octave);
        int steps = octave * DiatonicDegreeEnum.Count + (chromaticDegree switch
        {
            ChromaticDegree.TT => augmentedFourthOverDiminishedFifth ? 3 : 4,
            var i => i.DiatonicStepCeiling().ToDelta()
        });
        return FromDiatonicChromaticDeltas(steps, value);
    }

    public static Interval FromDiatonicDelta(int value)
        => FromDiatonicChromaticDeltas(value, DiatonicToChromatic(value));

    public static Interval FromDiatonicDelta(int value, Alteration alteration)
    {
        var result = FromDiatonicChromaticDeltas(value, DiatonicToChromatic(value) + alteration.ToChromaticDelta());
        if (value < 0) result = -result;
        return result;
    }

    public static Interval SimplifyEnharmonically(Interval interval)
    {
        int diatonicSemitones = DiatonicToChromatic(interval.diatonicDelta);
        return FromChromaticDelta(interval.chromaticDelta, preferSharp: interval.chromaticDelta > diatonicSemitones);
    }

    public static Interval ModOctave(Interval value)
    {
        var result = IntMath.EuclidianDivMod(value.DiatonicDelta, DiatonicDegreeEnum.Count);
        return FromDiatonicChromaticDeltas(result.Remainder, value.ChromaticDelta - result.Quotient * ChromaticDegreeEnum.Count);
    }

    public static Interval Negate(Interval value) => FromDiatonicChromaticDeltas(-value.DiatonicDelta, -value.ChromaticDelta);
    public static Interval Add(Interval lhs, Interval rhs)
        => FromDiatonicChromaticDeltas(lhs.DiatonicDelta + rhs.DiatonicDelta, lhs.ChromaticDelta + rhs.ChromaticDelta);
    public static Interval Subtract(Interval lhs, Interval rhs)
        => FromDiatonicChromaticDeltas(lhs.DiatonicDelta - rhs.DiatonicDelta, lhs.ChromaticDelta - rhs.ChromaticDelta);
    public static Interval Multiply(Interval lhs, int rhs)
        => checked(FromDiatonicChromaticDeltas(lhs.DiatonicDelta * rhs, lhs.ChromaticDelta * rhs));
    public static Interval Multiply(int lhs, Interval rhs)
        => checked(FromDiatonicChromaticDeltas(lhs * rhs.DiatonicDelta, lhs * rhs.ChromaticDelta));

    public static Interval operator -(Interval value) => Negate(value);
    public static Interval operator +(Interval lhs, Interval rhs) => Add(lhs, rhs);
    public static Interval operator -(Interval lhs, Interval rhs) => Subtract(lhs, rhs);
    public static Interval operator *(Interval lhs, int rhs) => Multiply(lhs, rhs);
    public static Interval operator *(int lhs, Interval rhs) => Multiply(lhs, rhs);
}
