using System;

namespace Pianomino.Formats.Smufl;

// Values are the log2 of the US name.
// Whole = 1 = 2^0
// Quarter = 1/4 = 2^-2
public enum NoteUnit : sbyte
{
    _1024th = -10,
    _512th = -9,
    _256th = -8,
    _128th = -7,
    _64th = -6,
    _32nd = -5,
    _16th = -4,
    _8th = -3,
    Quarter = -2,
    Half = -1,
    Whole = 0,
    DoubleWhole = 1,
}

public static class NoteUnitEnum
{
    public const NoteUnit Shortest = NoteUnit._1024th;
    public const NoteUnit Longest = NoteUnit.DoubleWhole;

    public static bool IsValid(this NoteUnit value) => value is >= Shortest and <= Longest;

    public static int GetNoteFlagCount(this NoteUnit value) => Math.Max(0, (int)NoteUnit.Quarter - (int)value);

    public static int GetLog2(this NoteUnit value) => (int)value;
}
