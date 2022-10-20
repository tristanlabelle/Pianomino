using System;

namespace Pianomino.Theory;

public enum MajorOrMinor : byte
{
    Major,
    Minor
}

public static class MajorOrMinorEnum
{
    public static Alteration GetDegreeAlteration(this MajorOrMinor value, DiatonicDegree degree)
        => ToMode(value).GetDegreeAlteration(degree);

    public static Alteration GetDegreeAlteration(this MajorOrMinor value, IntervalClass note)
        => ToMode(value).GetDegreeAlteration(note);

    public static string GetRomanNumeral(this MajorOrMinor value, IntervalClass root,
        RomanNumeralFlags romanNumeralFlags, AccidentalStringFlags accidentalFlags)
        => ToMode(value).GetRomanNumeral(root, romanNumeralFlags, accidentalFlags);

    public static DiatonicMode ToMode(this MajorOrMinor value)
        => value is MajorOrMinor.Major ? DiatonicMode.Ionian : DiatonicMode.Aeolian;
}
