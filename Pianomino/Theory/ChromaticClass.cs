using System;

namespace Pianomino.Theory;

public enum ChromaticClass : byte
{
    C, CsDb, D, DsEb, E, F, FsGb, G, GsAb, A, AsBb, B
}

public static class ChromaticClassEnum
{
    private static readonly string[] asciiNames = { "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B" };
    private static readonly string[] unicodeNames = { "C", "C♯/D♭", "D", "D♯/E♭", "E", "F", "F♯/G♭", "G", "G♯/A♭", "A", "A♯/B♭", "B" };

    public const int Count = 12;

    public static string GetName(this ChromaticClass value, bool unicodeAccidentals = false)
        => (unicodeAccidentals ? unicodeNames : asciiNames)[(int)value];

    public static bool IsValid(this ChromaticClass value) => (uint)value < Count;
    public static bool IsNatural(this ChromaticClass value) => ((ChromaticDegree)value).IsDiatonic();

    public static int GetValue(this ChromaticClass value) => (int)value;

    public static ChromaticClass Add(this ChromaticClass value, int delta)
        => (ChromaticClass)IntMath.EuclidianMod((int)value + delta, Count);

    public static ChromaticClass FromValue(int value)
        => (ChromaticClass)IntMath.EuclidianMod(value, Count);

    public static ChromaticClass FromValue(int value, out int octave)
    {
        var result = IntMath.EuclidianDivMod(value, Count);
        octave = result.Quotient;
        return (ChromaticClass)result.Remainder;
    }

    public static ChromaticClass NaturalFloor(this ChromaticClass value)
        => IsNatural(value) ? value : (ChromaticClass)((int)value - 1);

    public static ChromaticClass NaturalCeil(this ChromaticClass value)
        => IsNatural(value) ? value : (ChromaticClass)((int)value + 1);

    public static ChromaticClass Add(this ChromaticClass value, ChromaticDegree delta)
        => (ChromaticClass)IntMath.EuclidianMod((int)value + (int)delta, Count);

    public static ChromaticDegree ToDegreeFrom(this ChromaticClass value, ChromaticClass root)
        => ChromaticDegreeEnum.FromDelta(value - root);

    public static ChromaticDegree ToDegreeFromC(this ChromaticClass value) => (ChromaticDegree)value;

    public static ChromaticPitch WithOctave(this ChromaticClass value, int octave)
        => new(value, octave);
}
