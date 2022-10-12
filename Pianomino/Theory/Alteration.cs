using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Theory;

/// <summary>
/// Represents the alteration of a note or interval,
/// ie the number of flats or sharps applied to the diatonic/natural value.
/// </summary>
public enum Alteration : sbyte
{
    DoubleFlat = -2,
    Flat = -1,
    Natural = 0,
    Sharp = 1,
    DoubleSharp = 2
}

public static class AlterationEnum
{
    public static bool IsNatural(this Alteration value) => value is Alteration.Natural;
    public static bool IsAltered(this Alteration value) => value is not Alteration.Natural;
    public static bool IsFlattened(this Alteration value) => value < Alteration.Natural;
    public static bool IsSharpened(this Alteration value) => value > Alteration.Natural;
    public static int ToChromaticDelta(this Alteration value) => (int)value;
    public static Alteration FromChromaticDelta(int value) => checked((Alteration)value);

    public static string GetAccidentalString(this Alteration alteration, AccidentalStringFlags flags)
    {
        if (!flags.IsValid()) throw new ArgumentOutOfRangeException(nameof(flags));

        if (alteration.IsNatural()) return (flags & AccidentalStringFlags.ExplicitNatural) == 0 ? string.Empty : MusicChars.Natural.ToString();

        if ((flags & AccidentalStringFlags.Unicode) == 0)
            return new(alteration.IsSharpened() ? MusicChars.AsciiSharp : MusicChars.AsciiFlat, Math.Abs(alteration.ToChromaticDelta()));

        else if ((flags & AccidentalStringFlags.UseDoubleSymbols) == 0)
            return new(alteration.IsSharpened() ? MusicChars.Sharp : MusicChars.Flat, Math.Abs(alteration.ToChromaticDelta()));
        else
            throw new NotImplementedException();
    }
}

[Flags]
public enum AccidentalStringFlags : byte
{
    Ascii = 0,
    Unicode = 1 << 0,
    ImplicitNatural = 0,
    ExplicitNatural = 1 << 1,
    UseDoubleSymbols = 1 << 2,

    Ascii_ImplicitNatural = Ascii | ImplicitNatural,
    Unicode_ImplicitNatural = Unicode | ImplicitNatural,
    Unicode_ExplicitNatural = Unicode | ExplicitNatural,
}

public static class AccidentalStringFlagsEnum
{
    public static AccidentalStringFlags GetImplicit(bool unicode)
        => unicode ? AccidentalStringFlags.Unicode_ImplicitNatural : AccidentalStringFlags.Ascii_ImplicitNatural;

    public static bool IsValid(this AccidentalStringFlags value)
        => HasAll(value, AccidentalStringFlags.Unicode) || (!HasAll(value, AccidentalStringFlags.ExplicitNatural) && !HasAll(value, AccidentalStringFlags.UseDoubleSymbols));

    public static bool HasAll(this AccidentalStringFlags value, AccidentalStringFlags flags) => (value & flags) == flags;
    public static bool HasAny(this AccidentalStringFlags value, AccidentalStringFlags flags) => (value & flags) != 0;
}
