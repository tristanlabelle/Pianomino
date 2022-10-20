using System;
using System.Globalization;

namespace Pianomino.Theory;

/// <summary>
/// The absolute pitch of a note, without distinguishing sharps from flats.
/// </summary>
public readonly struct ChromaticPitch : IEquatable<ChromaticPitch>, IComparable<ChromaticPitch>
{
    public const int PerOctave = 12;

    public static readonly ChromaticPitch C0 = new(0);
    public static readonly ChromaticPitch C4 = new(PerOctave * 4);
    public static readonly ChromaticPitch A4 = new(PerOctave * 4 + (int)ChromaticClass.A);

    private readonly short value;

    public ChromaticPitch(int value) => this.value = checked((short)value);
    public ChromaticPitch(ChromaticClass @class, int octave)
    {
        if (!@class.IsValid()) throw new ArgumentOutOfRangeException(nameof(@class));
        this.value = checked((short)(octave * PerOctave + (int)@class));
    }

    public int Value => this.value;
    public ChromaticClass Class => ChromaticClassEnum.FromValue(value);
    public bool IsNatural => Class.IsNatural();
    public int Octave => IntMath.EuclidianDiv(value, PerOctave);

    public ChromaticPitch WithOctaveZero() => new((int)Class);
    public ChromaticPitch WithOctave(int octave) => new(checked(octave * PerOctave + (int)Class));

    public string ToString(bool unicodeAccidentals) => Class.GetName(unicodeAccidentals) + Octave.ToString(CultureInfo.InvariantCulture);
    public override string ToString() => ToString(unicodeAccidentals: false);

    public bool Equals(ChromaticPitch other) => value == other.value;
    public override bool Equals(object? obj) => obj is ChromaticPitch other && Equals(other);
    public override int GetHashCode() => value;
    public static bool Equals(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.Equals(rhs);
    public static bool operator ==(ChromaticPitch lhs, ChromaticPitch rhs) => Equals(lhs, rhs);
    public static bool operator !=(ChromaticPitch lhs, ChromaticPitch rhs) => !Equals(lhs, rhs);

    public int CompareTo(ChromaticPitch other) => value.CompareTo(other.value);
    public static int Compare(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.CompareTo(rhs);
    public static bool operator <(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.value < rhs.value;
    public static bool operator <=(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.value <= rhs.value;
    public static bool operator >(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.value > rhs.value;
    public static bool operator >=(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.value >= rhs.value;

    public static ChromaticPitch Add(ChromaticPitch lhs, int rhs) => new(checked(lhs.Value + rhs));
    public static ChromaticPitch Subtract(ChromaticPitch lhs, int rhs) => new(checked(lhs.Value - rhs));
    public static int Subtract(ChromaticPitch lhs, ChromaticPitch rhs) => lhs.value - rhs.value;
    public static ChromaticPitch operator +(ChromaticPitch lhs, int rhs) => Add(lhs, rhs);
    public static ChromaticPitch operator -(ChromaticPitch lhs, int rhs) => Subtract(lhs, rhs);
    public static int operator -(ChromaticPitch lhs, ChromaticPitch rhs) => Subtract(lhs, rhs);

    public static ChromaticPitch Min(ChromaticPitch lhs, ChromaticPitch rhs) => lhs < rhs ? lhs : rhs;
    public static ChromaticPitch Max(ChromaticPitch lhs, ChromaticPitch rhs) => lhs > rhs ? lhs : rhs;

    public static ChromaticPitch C(int octave) => new(checked(octave * PerOctave));

    public static ChromaticPitch OctaveZero(ChromaticClass chromaticClass) => new(chromaticClass, octave: 0);
}
