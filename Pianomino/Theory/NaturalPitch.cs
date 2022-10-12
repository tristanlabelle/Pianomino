using System;
using System.Runtime.InteropServices;

namespace Pianomino.Theory;

/// <summary>
/// Identifies a natural note pitch, i.e. one of the white keys on a keyboard.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct NaturalPitch : IEquatable<NaturalPitch>, IComparable<NaturalPitch>
{
    public const int PerOctave = NoteLetterEnum.Count;

    public static readonly NaturalPitch GeneralMidiZero = new(-1 * PerOctave);
    public static readonly NaturalPitch C0 = new(0);
    public static readonly NaturalPitch D0 = new(1);
    public static readonly NaturalPitch E0 = new(2);
    public static readonly NaturalPitch F0 = new(3);
    public static readonly NaturalPitch G0 = new(4);
    public static readonly NaturalPitch A0 = new(5);
    public static readonly NaturalPitch B0 = new(6);
    public static readonly NaturalPitch C1 = new(1 * PerOctave + 0);
    public static readonly NaturalPitch C2 = new(2 * PerOctave + 0);
    public static readonly NaturalPitch C3 = new(3 * PerOctave + 0);
    public static readonly NaturalPitch F3 = new(3 * PerOctave + 3); // F Clef
    public static readonly NaturalPitch C4 = new(4 * PerOctave + 0);
    public static readonly NaturalPitch D4 = new(4 * PerOctave + 1);
    public static readonly NaturalPitch E4 = new(4 * PerOctave + 2); // Bottom staff line on G clef
    public static readonly NaturalPitch F4 = new(4 * PerOctave + 3);
    public static readonly NaturalPitch G4 = new(4 * PerOctave + 4); // G Clef
    public static readonly NaturalPitch A4 = new(4 * PerOctave + 5);
    public static readonly NaturalPitch B4 = new(4 * PerOctave + 6);
    public static readonly NaturalPitch C5 = new(5 * PerOctave + 0);
    public static readonly NaturalPitch MiddleC = C4; // C Clef

    private readonly sbyte diatonicValue;

    private NaturalPitch(int diatonicValue) => this.diatonicValue = checked((sbyte)diatonicValue);

    public NaturalPitch(NoteLetter letter, int octave)
    {
        if (!letter.IsValid()) throw new ArgumentOutOfRangeException(nameof(letter));
        this.diatonicValue = checked((sbyte)(octave * PerOctave + (int)letter));
    }

    public int DiatonicValue => diatonicValue;
    public int Octave => IntMath.EuclidianDiv(diatonicValue, PerOctave);
    public NoteLetter Letter => NoteLetterEnum.FromDiatonicValue(diatonicValue);

    public NotePitch WithAlteration(Alteration alteration) => new(this, alteration);

    public ChromaticPitch ToChromatic() => new(Interval.DiatonicToChromatic(DiatonicValue));
    public override string ToString() => $"{Letter}{Octave}";

    public bool Equals(NaturalPitch other) => diatonicValue == other.diatonicValue;
    public override bool Equals(object? obj) => obj is NaturalPitch other && Equals(other);
    public override int GetHashCode() => diatonicValue;
    public static bool Equals(NaturalPitch lhs, NaturalPitch rhs) => lhs.Equals(rhs);
    public static bool operator ==(NaturalPitch lhs, NaturalPitch rhs) => Equals(lhs, rhs);
    public static bool operator !=(NaturalPitch lhs, NaturalPitch rhs) => !Equals(lhs, rhs);

    public int CompareTo(NaturalPitch other) => diatonicValue.CompareTo(other.diatonicValue);
    public static int Compare(NaturalPitch lhs, NaturalPitch rhs) => lhs.CompareTo(rhs);
    public static bool operator >(NaturalPitch lhs, NaturalPitch rhs) => Compare(lhs, rhs) > 0;
    public static bool operator <(NaturalPitch lhs, NaturalPitch rhs) => Compare(lhs, rhs) < 0;
    public static bool operator >=(NaturalPitch lhs, NaturalPitch rhs) => Compare(lhs, rhs) >= 0;
    public static bool operator <=(NaturalPitch lhs, NaturalPitch rhs) => Compare(lhs, rhs) <= 0;

    public static NaturalPitch FromDiatonicValue(int value) => new(value);
    public static NaturalPitch OctaveZero(NoteLetter letter) => new(letter, octave: 0);

    public static NaturalPitch FromChromatic(ChromaticPitch pitch, bool roundUp)
        => new(NoteLetterEnum.FromChromatic(pitch.Class, roundUp), pitch.Octave);

    public static NaturalPitch C(int octave) => new(octave * PerOctave + (int)NoteLetter.C);
    public static NaturalPitch D(int octave) => new(octave * PerOctave + (int)NoteLetter.D);
    public static NaturalPitch E(int octave) => new(octave * PerOctave + (int)NoteLetter.E);
    public static NaturalPitch F(int octave) => new(octave * PerOctave + (int)NoteLetter.F);
    public static NaturalPitch G(int octave) => new(octave * PerOctave + (int)NoteLetter.G);
    public static NaturalPitch A(int octave) => new(octave * PerOctave + (int)NoteLetter.A);
    public static NaturalPitch B(int octave) => new(octave * PerOctave + (int)NoteLetter.B);

    public static NaturalPitch Add(NaturalPitch pitch, int steps) => new(pitch.DiatonicValue + steps);
    public static NaturalPitch Subtract(NaturalPitch pitch, int steps) => Add(pitch, -steps);
    public static int Subtract(NaturalPitch lhs, NaturalPitch rhs) => lhs.DiatonicValue - rhs.DiatonicValue;
    public static NaturalPitch operator +(NaturalPitch lhs, int rhs) => Add(lhs, rhs);
    public static NaturalPitch operator -(NaturalPitch lhs, int rhs) => Subtract(lhs, rhs);
    public static int operator -(NaturalPitch lhs, NaturalPitch rhs) => Subtract(lhs, rhs);
}
