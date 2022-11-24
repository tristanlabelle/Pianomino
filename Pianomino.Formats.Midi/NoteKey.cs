using Pianomino.Theory;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Pianomino.Formats.Midi;

/// <summary>
/// Represents what is interchangeably called as a note number or key number by the midi standard.
/// </summary>
/// <remarks>
/// Named so because "Note" is ambiguous with a timed event, "key" is ambigious with a tonality,
/// and "NoteNumber" or "KeyNumber" sound like primitives.
/// </remarks>
public readonly struct NoteKey : IEquatable<NoteKey>, IComparable<NoteKey>
{
    public const byte MaxNumber = 127;
    public static NoteKey MinValue => new((byte)0);
    public static NoteKey MaxValue => new(MaxNumber);
    public static NoteKey C0 => new(12);
    public static NoteKey C4 => new(72);
    public static ChromaticPitch MinPitch => ChromaticPitch.C(-1);
    public static ChromaticPitch MaxPitch => ChromaticPitch.C(-1) + 127;

    public byte Number { get; }

    public NoteKey(byte number)
    {
        if (number >= 0x80) throw new ArgumentOutOfRangeException(nameof(number));
        this.Number = number;
    }

    public NoteKey(ChromaticPitch pitch)
    {
        var value = (uint)(pitch.Value + 12);
        if (value >= 0x80) throw new ArgumentOutOfRangeException(nameof(pitch));
        this.Number = (byte)value;
    }

    public NoteKey(GeneralMidiPercussion percussion) : this((byte)percussion) { }

    public ChromaticPitch Pitch => new(Number - 12);
    public ChromaticClass PitchClass => Pitch.Class;
    public int Octave => Pitch.Octave;

    public GeneralMidiPercussion AsPercussion() => (GeneralMidiPercussion)Number;

    public int CompareTo(NoteKey other) => Number.CompareTo(other.Number);

    public bool Equals(NoteKey other) => other.Number == Number;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NoteKey other && Equals(other);
    public override int GetHashCode() => Number;

    public string ToString(bool unicodeAccidentals) => Pitch.ToString(unicodeAccidentals);
    public override string ToString() => ToString(unicodeAccidentals: false);

    public static byte GetNumber(ChromaticPitch pitch)
    {
        var value = (uint)((int)pitch.Value + 12);
        if (value >= 0x80) throw new ArgumentOutOfRangeException(nameof(pitch));
        return (byte)value;
    }

    public static ChromaticPitch ToPitch(byte number)
    {
        if (number >= 0x80) throw new ArgumentOutOfRangeException(nameof(number));
        return new(number - 12);
    }

    public static int Compare(NoteKey lhs, NoteKey rhs) => lhs.CompareTo(rhs);
    public static bool Equals(NoteKey lhs, NoteKey rhs) => lhs.Equals(rhs);
    public static bool operator <(NoteKey lhs, NoteKey rhs) => lhs.Number < rhs.Number;
    public static bool operator <=(NoteKey lhs, NoteKey rhs) => lhs.Number <= rhs.Number;
    public static bool operator >(NoteKey lhs, NoteKey rhs) => lhs.Number > rhs.Number;
    public static bool operator >=(NoteKey lhs, NoteKey rhs) => lhs.Number >= rhs.Number;
    public static bool operator ==(NoteKey lhs, NoteKey rhs) => lhs.Number == rhs.Number;
    public static bool operator !=(NoteKey lhs, NoteKey rhs) => lhs.Number != rhs.Number;

    public static NoteKey Add(NoteKey value, int delta) => new(checked((byte)(value.Number + delta)));
    public static NoteKey Subtract(NoteKey value, int delta) => new(checked((byte)(value.Number + delta)));
    public static NoteKey operator +(NoteKey lhs, int rhs) => Add(lhs, rhs);
    public static NoteKey operator -(NoteKey lhs, int rhs) => Subtract(lhs, rhs);

    public static explicit operator NoteKey(byte number) => new(number);
    public static explicit operator NoteKey(ChromaticPitch pitch) => new(pitch);
    public static explicit operator NoteKey(GeneralMidiPercussion percussion) => new(percussion);
    public static implicit operator byte(NoteKey note) => note.Number;
}
