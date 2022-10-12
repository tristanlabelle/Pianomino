using System;

namespace Pianomino.Theory;

/// <summary>
/// Represents any of the combinations of thirds, fifths and sevenths forming a seventh chord.
/// </summary>
[Flags]
public enum TertianSeventhChordQuality : byte
{
    // 77_55_3, 1 or 2 bit signed ints
    Third_Mask = 0b00_00_1,
    Third_Major = 0b00_00_0,
    Third_Minor = 0b00_00_1,

    Fifth_Mask = 0b00_11_0,
    Fifth_Diminished = 0b00_11_0,
    Fifth_Perfect = 0b00_00_0,
    Fifth_Augmented = 0b00_01_0,

    Seventh_Mask = 0b11_00_0,
    Seventh_Diminished = 0b10_00_0,
    Seventh_Minor = 0b11_00_0,
    Seventh_Major = 0b00_00_0,

    Major = Third_Major | Fifth_Perfect | Seventh_Major,
    Dominant = Third_Major | Fifth_Perfect | Seventh_Minor,
    Minor = Third_Minor | Fifth_Perfect | Seventh_Minor,
    HalfDiminished = Third_Minor | Fifth_Diminished | Seventh_Minor,
    Diminished = Third_Minor | Fifth_Diminished | Seventh_Diminished,

    MinorMajor = Third_Minor | Fifth_Perfect | Seventh_Major,
    Augmented = Third_Major | Fifth_Augmented | Seventh_Minor,
    AugmentedMajor = Third_Major | Fifth_Augmented | Seventh_Major,
}

public static class TertianSeventhChordQualityEnum
{
    public static bool IsThirdMajor(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Third_Mask) == TertianSeventhChordQuality.Third_Major;
    public static bool IsThirdMinor(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Third_Mask) == TertianSeventhChordQuality.Third_Minor;
    public static MajorOrMinor GetThirdMajorOrMinor(this TertianSeventhChordQuality value)
        => IsThirdMajor(value) ? MajorOrMinor.Major : MajorOrMinor.Minor;
    public static IntervalQuality GetThirdQuality(this TertianSeventhChordQuality value)
        => IsThirdMajor(value) ? IntervalQuality.Major : IntervalQuality.Minor;
    public static Alteration GetThirdAlteration(this TertianSeventhChordQuality value)
        => IsThirdMajor(value) ? Alteration.Natural : Alteration.Flat;
    public static ChromaticDegree GetThirdChromaticDegree(this TertianSeventhChordQuality value)
        => IsThirdMajor(value) ? ChromaticDegree.M3 : ChromaticDegree.m3;

    public static bool IsFifthDiminished(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) == TertianSeventhChordQuality.Fifth_Diminished;
    public static bool IsFifthPerfect(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) == TertianSeventhChordQuality.Fifth_Perfect;
    public static bool IsFifthAugmented(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) == TertianSeventhChordQuality.Fifth_Augmented;
    public static IntervalQuality GetFifthQuality(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) switch
        {
            TertianSeventhChordQuality.Fifth_Diminished => IntervalQuality.Diminished,
            TertianSeventhChordQuality.Fifth_Perfect => IntervalQuality.Perfect,
            TertianSeventhChordQuality.Fifth_Augmented => IntervalQuality.Augmented,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    public static Alteration GetFifthAlteration(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) switch
        {
            TertianSeventhChordQuality.Fifth_Diminished => Alteration.Flat,
            TertianSeventhChordQuality.Fifth_Perfect => Alteration.Natural,
            TertianSeventhChordQuality.Fifth_Augmented => Alteration.Sharp,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    public static ChromaticDegree GetFifthChromaticDegree(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Fifth_Mask) switch
        {
            TertianSeventhChordQuality.Fifth_Diminished => ChromaticDegree.TT,
            TertianSeventhChordQuality.Fifth_Perfect => ChromaticDegree.P5,
            TertianSeventhChordQuality.Fifth_Augmented => ChromaticDegree.m6,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

    public static bool IsSeventhDiminished(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) == TertianSeventhChordQuality.Seventh_Diminished;
    public static bool IsSeventhMinor(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) == TertianSeventhChordQuality.Seventh_Minor;
    public static bool IsSeventhMajor(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) == TertianSeventhChordQuality.Seventh_Major;
    public static IntervalQuality GetSeventhQuality(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) switch
        {
            TertianSeventhChordQuality.Seventh_Diminished => IntervalQuality.Diminished,
            TertianSeventhChordQuality.Seventh_Minor => IntervalQuality.Minor,
            TertianSeventhChordQuality.Seventh_Major => IntervalQuality.Major,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    public static Alteration GetSeventhAlteration(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) switch
        {
            TertianSeventhChordQuality.Seventh_Diminished => Alteration.DoubleFlat,
            TertianSeventhChordQuality.Seventh_Minor => Alteration.Flat,
            TertianSeventhChordQuality.Seventh_Major => Alteration.Natural,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    public static ChromaticDegree GetSeventhChromaticDegree(this TertianSeventhChordQuality value)
        => (value & TertianSeventhChordQuality.Seventh_Mask) switch
        {
            TertianSeventhChordQuality.Seventh_Diminished => ChromaticDegree.M6,
            TertianSeventhChordQuality.Seventh_Minor => ChromaticDegree.m7,
            TertianSeventhChordQuality.Seventh_Major => ChromaticDegree.M7,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

    public static (Interval Third, Interval Fifth, Interval Seventh) GetIntervals(this TertianSeventhChordQuality value)
        => (Interval.FromDiatonicDelta(2, GetThirdAlteration(value)),
        Interval.FromDiatonicDelta(4, GetFifthAlteration(value)),
        Interval.FromDiatonicDelta(6, GetSeventhAlteration(value)));

    public static string? TryGetSymbol(this TertianSeventhChordQuality value)
        => value switch
        {
            TertianSeventhChordQuality.Major => "M7",
            TertianSeventhChordQuality.Dominant => "7",
            TertianSeventhChordQuality.Minor => "m7",
            TertianSeventhChordQuality.HalfDiminished => "m7(b5)",
            TertianSeventhChordQuality.Diminished => "dim7",
            TertianSeventhChordQuality.MinorMajor => "mM7",
            TertianSeventhChordQuality.Augmented => "aug7",
            TertianSeventhChordQuality.AugmentedMajor => "augM7",
            _ => null
        };

    public static TertianSeventhChordQuality? ToNegativeHarmony(this TertianSeventhChordQuality value)
        => value switch
        {
            TertianSeventhChordQuality.Major => TertianSeventhChordQuality.Major,
            TertianSeventhChordQuality.Dominant => TertianSeventhChordQuality.HalfDiminished,
            TertianSeventhChordQuality.Minor => TertianSeventhChordQuality.Minor,
            TertianSeventhChordQuality.HalfDiminished => TertianSeventhChordQuality.Dominant,
            TertianSeventhChordQuality.Diminished => TertianSeventhChordQuality.Diminished,
            TertianSeventhChordQuality.MinorMajor => TertianSeventhChordQuality.AugmentedMajor,
            TertianSeventhChordQuality.Augmented => null, // Becomes II+/I
                TertianSeventhChordQuality.AugmentedMajor => TertianSeventhChordQuality.MinorMajor,
            _ => null
        };

    public static bool Contains(this TertianSeventhChordQuality value, ChromaticDegree interval)
        => interval == ChromaticDegree.P1 || interval == GetThirdChromaticDegree(value)
        || interval == GetFifthChromaticDegree(value) || interval == GetSeventhChromaticDegree(value);

    public static TertianChordSpelling ToChordSpelling(this TertianSeventhChordQuality value)
        => new(third: GetThirdMajorOrMinor(value), fifth: GetFifthQuality(value), seventh: GetSeventhQuality(value));

    public static ToneSet ToToneSet(this TertianSeventhChordQuality value)
        => ToneSet.RootOnly.With(GetThirdChromaticDegree(value))
            .With(GetFifthChromaticDegree(value))
            .With(GetSeventhChromaticDegree(value));
}
