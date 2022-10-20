using System;

namespace Pianomino.Theory;

/// <summary>
/// One of the seven note letters: C, D, E, F, G, A, B.
/// </summary>
public enum NoteLetter : byte
{
    C, D, E, F, G, A, B
}

public static class NoteLetterEnum
{
    private static readonly NoteLetter[] fromChromaticClass_floor = {
            NoteLetter.C, NoteLetter.C, NoteLetter.D, NoteLetter.D, NoteLetter.E, NoteLetter.F,
            NoteLetter.F, NoteLetter.G, NoteLetter.G, NoteLetter.A, NoteLetter.G, NoteLetter.B };
    private static readonly NoteLetter[] fromChromaticClass_ceiling = {
            NoteLetter.C, NoteLetter.D, NoteLetter.D, NoteLetter.E, NoteLetter.E, NoteLetter.F,
            NoteLetter.G, NoteLetter.G, NoteLetter.A, NoteLetter.A, NoteLetter.B, NoteLetter.B };

    public const int Count = 7;

    public static bool IsValid(this NoteLetter letter) => (uint)letter < (uint)Count;

    public static NoteLetter? TryFromChar(char c) => c switch
    {
        >= 'C' and <= 'G' => (NoteLetter)((int)NoteLetter.C + (c - 'C')),
        >= 'c' and <= 'g' => (NoteLetter)((int)NoteLetter.C + (c - 'c')),
        'A' or 'a' => NoteLetter.A,
        'B' or 'b' => NoteLetter.B,
        _ => null
    };

    public static char ToChar(this NoteLetter letter) => "CDEFGAB"[(int)letter];

    public static NoteLetter FromDiatonicValue(int value) => (NoteLetter)IntMath.EuclidianMod(value, Count);

    public static int ToDiatonicValue(this NoteLetter letter) => (int)letter;

    public static NoteLetter FromChromatic(ChromaticClass value, bool roundUp)
        => (roundUp ? fromChromaticClass_ceiling : fromChromaticClass_floor)[(int)value];

    public static NaturalPitch WithOctave(this NoteLetter letter, int octave) => new(letter, octave);
    public static NoteClass WithAlteration(this NoteLetter letter, Alteration alteration) => new(letter, alteration);

    public static NoteClass Flat(this NoteLetter letter) => new(letter, Alteration.Flat);
    public static NoteClass Natural(this NoteLetter letter) => new(letter, Alteration.Natural);
    public static NoteClass Sharp(this NoteLetter letter) => new(letter, Alteration.Sharp);
}
