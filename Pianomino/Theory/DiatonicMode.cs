using System;

namespace Pianomino.Theory;

public enum DiatonicMode : byte
{
    Ionian,
    Dorian,
    Phrygian,
    Lydian,
    Mixolydian,
    Aeolian,
    Locrian
}

public static class DiatonicModeEnum
{
    private static readonly ToneSet[] scales =
    {
        ToneSet.Scales.Ionian,
        ToneSet.Scales.Dorian,
        ToneSet.Scales.Phrygian,
        ToneSet.Scales.Lydian,
        ToneSet.Scales.Mixolydian,
        ToneSet.Scales.Aeolian,
        ToneSet.Scales.Locrian,
    };

    private static readonly IntervalPattern[] intervalPatterns =
    {
        IntervalPattern.DiatonicModes.Ionian,
        IntervalPattern.DiatonicModes.Dorian,
        IntervalPattern.DiatonicModes.Phrygian,
        IntervalPattern.DiatonicModes.Lydian,
        IntervalPattern.DiatonicModes.Mixolydian,
        IntervalPattern.DiatonicModes.Aeolian,
        IntervalPattern.DiatonicModes.Locrian,
    };

    public const DiatonicMode Major = DiatonicMode.Ionian;
    public const DiatonicMode Minor = DiatonicMode.Aeolian;
    public const int Count = 7;

    public static Alteration GetDegreeAlteration(this DiatonicMode mode, DiatonicDegree degree)
        => ToIntervalPattern(mode).Intervals[(int)degree].Alteration;

    public static DiatonicDegree GetIonianRoot(this DiatonicMode mode) => (DiatonicDegree)mode;

    public static Alteration GetDegreeAlteration(this DiatonicMode mode, IntervalClass note)
        => (Alteration)(note.Alteration - GetDegreeAlteration(mode, note.DiatonicDegree));

    public static string GetRomanNumeral(this DiatonicMode mode, IntervalClass root,
        RomanNumeralFlags romanNumeralFlags, AccidentalStringFlags accidentalFlags)
    {
        return GetDegreeAlteration(mode, root).GetAccidentalString(accidentalFlags) + root.DiatonicDegree.GetRomanNumeral(romanNumeralFlags);
    }

    public static DiatonicMode Get(MajorOrMinor majorOrMinor)
        => majorOrMinor is MajorOrMinor.Major ? Major : Minor;
    public static bool HasMajorThird(this DiatonicMode mode)
        => mode is DiatonicMode.Ionian or DiatonicMode.Lydian or DiatonicMode.Mixolydian;
    public static bool HasMinorThird(this DiatonicMode mode) => !HasMajorThird(mode);
    public static MajorOrMinor GetThirdQuality(this DiatonicMode mode)
        => HasMajorThird(mode) ? MajorOrMinor.Major : MajorOrMinor.Minor;

    public static ToneSet ToScale(this DiatonicMode mode) => scales[(int)mode];
    public static IntervalPattern ToIntervalPattern(this DiatonicMode mode) => intervalPatterns[(int)mode];
}
