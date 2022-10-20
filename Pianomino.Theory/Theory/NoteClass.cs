using System;
using System.Runtime.InteropServices;

namespace Pianomino.Theory;

/// <summary>
/// An altered note without octave distinction. e.g.: C, F, C#, Bb
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct NoteClass : IEquatable<NoteClass>
{
    private static sbyte[] lettersToFifths = { 0, 2, 4, -1, 1, 3, 5 };
    private static NoteLetter[] fifthsToLetters = { NoteLetter.C, NoteLetter.G, NoteLetter.D, NoteLetter.A, NoteLetter.E, NoteLetter.B, NoteLetter.F };

    private readonly sbyte letterAndAlteration; // Letter mod 7 + alteration * 7.

    private NoteClass(sbyte letterAndAlteration) => this.letterAndAlteration = letterAndAlteration;

    public NoteClass(NoteLetter letter, Alteration alteration)
    {
        if (!letter.IsValid()) throw new ArgumentOutOfRangeException(nameof(letter));
        this.letterAndAlteration = (sbyte)((int)letter + (int)alteration * NoteLetterEnum.Count);
    }

    public NoteClass(NoteLetter letter)
    {
        if (!letter.IsValid()) throw new ArgumentOutOfRangeException(nameof(letter));
        this.letterAndAlteration = (sbyte)letter;
    }

    public static implicit operator NoteClass(NoteLetter letter) => new(letter);

    public NoteLetter Letter => (NoteLetter)IntMath.EuclidianMod(letterAndAlteration, NoteLetterEnum.Count);
    public Alteration Alteration => (Alteration)IntMath.EuclidianDiv(letterAndAlteration, NoteLetterEnum.Count);

    public NoteClass Flat() => new(Letter, Alteration - 1);
    public NoteClass Sharp() => new(Letter, Alteration + 1);
    public NoteClass Alter(Alteration alteration) => new(Letter, (Alteration)((int)Alteration + (int)alteration));
    public NoteClass WithAlteration(Alteration alteration) => new(Letter, alteration);
    public NotePitch WithOctave(int octave) => new(this, octave);
    public NotePitch WithOctaveZero() => WithOctave(0);

    public ChromaticClass ToChromatic() => WithOctave(0).ChromaticClass;
    public int ToFifths() => lettersToFifths[(int)Letter] + (int)Alteration * NoteLetterEnum.Count;

    public string ToString(AccidentalStringFlags accidentalFlags) => Letter.ToString() + Alteration.GetAccidentalString(accidentalFlags);
    public override string ToString() => ToString(AccidentalStringFlags.Ascii_ImplicitNatural);

    public bool Equals(NoteClass other) => other.letterAndAlteration == letterAndAlteration;
    public override bool Equals(object? obj) => obj is NoteClass other && Equals(other);
    public override int GetHashCode() => letterAndAlteration;
    public static bool Equals(NoteClass lhs, NoteClass rhs) => lhs.Equals(rhs);
    public static bool operator ==(NoteClass lhs, NoteClass rhs) => Equals(lhs, rhs);
    public static bool operator !=(NoteClass lhs, NoteClass rhs) => !Equals(lhs, rhs);

    public static NoteClass FromFifths(int fifths)
        => new((sbyte)((int)fifthsToLetters[IntMath.EuclidianMod(fifths, NoteLetterEnum.Count)]
            + IntMath.EuclidianDiv(fifths + 1, NoteLetterEnum.Count) * NoteLetterEnum.Count));

    public static NoteClass C(Alteration alteration = Alteration.Natural) => new(NoteLetter.C, alteration);
    public static NoteClass D(Alteration alteration = Alteration.Natural) => new(NoteLetter.D, alteration);
    public static NoteClass E(Alteration alteration = Alteration.Natural) => new(NoteLetter.E, alteration);
    public static NoteClass F(Alteration alteration = Alteration.Natural) => new(NoteLetter.F, alteration);
    public static NoteClass G(Alteration alteration = Alteration.Natural) => new(NoteLetter.G, alteration);
    public static NoteClass A(Alteration alteration = Alteration.Natural) => new(NoteLetter.A, alteration);
    public static NoteClass B(Alteration alteration = Alteration.Natural) => new(NoteLetter.B, alteration);

    public static NoteClass Add(NoteClass value, Interval interval) => (value.WithOctave(0) + interval).Class;
    public static NoteClass Subtract(NoteClass value, Interval interval) => (value.WithOctave(0) - interval).Class;
    public static NoteClass operator +(NoteClass value, Interval interval) => Add(value, interval);
    public static NoteClass operator -(NoteClass value, Interval interval) => Subtract(value, interval);
    
    public static NoteClass Add(NoteClass value, IntervalClass intervalClass) => Add(value, intervalClass.WithOctaveZero());
    public static NoteClass Subtract(NoteClass value, IntervalClass intervalClass) => Subtract(value, intervalClass.WithOctaveZero());
    public static NoteClass operator +(NoteClass value, IntervalClass intervalClass) => Add(value, intervalClass);
    public static NoteClass operator -(NoteClass value, IntervalClass intervalClass) => Subtract(value, intervalClass);

    public static IntervalClass Subtract(NoteClass first, NoteClass second) => (first.WithOctaveZero() - second.WithOctaveZero()).Class;
    public static IntervalClass operator -(NoteClass lhs, NoteClass rhs) => Subtract(lhs, rhs);

    public static NoteClass GetSimplestMajorTonic(ChromaticClass tonic, bool fSharpOverGFlat = true) => tonic switch
    {
        ChromaticClass.C => NoteLetter.C,
        ChromaticClass.CsDb => NoteLetter.D.Flat(), // Db = 5 flats
        ChromaticClass.D => NoteLetter.D,
        ChromaticClass.DsEb => NoteLetter.E.Flat(), // Eb = 3 flats
        ChromaticClass.E => NoteLetter.E,
        ChromaticClass.F => NoteLetter.F,
        ChromaticClass.FsGb => fSharpOverGFlat ? NoteLetter.F.Sharp() : NoteLetter.G.Flat(), // F# = 6 sharps, Gb = 6 flats
        ChromaticClass.G => NoteLetter.G,
        ChromaticClass.GsAb => NoteLetter.A.Flat(), // Ab = 4 flats
        ChromaticClass.A => NoteLetter.A,
        ChromaticClass.AsBb => NoteLetter.B.Flat(), // Bb = 2 flats
        ChromaticClass.B => NoteLetter.B,
        _ => throw new ArgumentOutOfRangeException(nameof(tonic))
    };

    public static NoteClass GetSimplestMinorTonic(ChromaticClass tonic, bool fSharpOverGFlat = true)
        => GetSimplestMajorTonic(tonic.Add(3), fSharpOverGFlat);

    public static NoteClass FromSimplestMajorKey(ChromaticClass chromaticClass, bool gSharpOverAFlat = true) => chromaticClass switch
    {
        ChromaticClass.C => NoteLetter.C,
        ChromaticClass.CsDb => NoteLetter.C.Sharp(), // C# happens in D (2 sharps), Db happens in Ab (4 flats)
        ChromaticClass.D => NoteLetter.D,
        ChromaticClass.DsEb => NoteLetter.E.Flat(), // D# happens in E (4 sharps), Eb happens in Bb (2 flats)
        ChromaticClass.E => NoteLetter.E,
        ChromaticClass.F => NoteLetter.F,
        ChromaticClass.FsGb => NoteLetter.F.Sharp(), // F# happens in G (1 sharp), Gb happens in Db (5 flats)
        ChromaticClass.G => NoteLetter.G,
        ChromaticClass.GsAb => gSharpOverAFlat ? NoteLetter.G.Sharp() : NoteLetter.A.Flat(), // G# happens in A (3 sharps), Ab happens in Eb (3 flats)
        ChromaticClass.A => NoteLetter.A,
        ChromaticClass.AsBb => NoteLetter.B.Flat(), // A# happens in B (5 sharps), Bb happens in F (1 flats)
        ChromaticClass.B => NoteLetter.B,
        _ => throw new UnreachableException()
    };
}
