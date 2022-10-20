using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Pianomino.Theory;

/// <summary>
/// Identifies the pitch of a note in scientific pitch notation,
/// from a letter, an octave, and an alteration.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly struct NotePitch : IEquatable<NotePitch>, IComparable<NotePitch>
{
    private static readonly Regex regex = new(
        @"\A
				(?<letter>[A-G])
				( (?<sharps>\#+|♯+) | (?<flats>b+|♭+) )?
				(?<octave>-?[0-9]+)
			\Z",
        RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

    public static readonly NotePitch C0 = NaturalPitch.C0;
    public static readonly NotePitch D0 = NaturalPitch.D0;
    public static readonly NotePitch E0 = NaturalPitch.E0;
    public static readonly NotePitch F0 = NaturalPitch.F0;
    public static readonly NotePitch G0 = NaturalPitch.G0;
    public static readonly NotePitch A0 = NaturalPitch.A0;
    public static readonly NotePitch B0 = NaturalPitch.B0;
    public static readonly NotePitch C1 = NaturalPitch.C1;
    public static readonly NotePitch C2 = NaturalPitch.C2;
    public static readonly NotePitch C3 = NaturalPitch.C3;
    public static readonly NotePitch C4 = NaturalPitch.C4;
    public static readonly NotePitch D4 = NaturalPitch.D4;
    public static readonly NotePitch E4 = NaturalPitch.E4;
    public static readonly NotePitch F4 = NaturalPitch.F4;
    public static readonly NotePitch G4 = NaturalPitch.G4;
    public static readonly NotePitch A4 = NaturalPitch.A4;
    public static readonly NotePitch B4 = NaturalPitch.B4;
    public static readonly NotePitch C5 = NaturalPitch.C5;

    // This used to be represented as an Interval from C0,
    // but negative intervals and below-C0 have a different interpretation:
    // A (-1, -2) interval is a descending major second (natural alteration)
    // A (-1, -2) note is a Bb (flat alteration)
    private readonly ChromaticPitch chromatic;
    private readonly NaturalPitch natural;

    public NotePitch(NaturalPitch natural, ChromaticPitch chromatic)
    {
        this.chromatic = chromatic;
        this.natural = natural;
    }

    public NotePitch(NaturalPitch natural)
        : this(natural, natural.ToChromatic()) { }

    public NotePitch(NaturalPitch natural, Alteration alteration)
        : this(natural, natural.ToChromatic() + alteration.ToChromaticDelta()) { }

    public NotePitch(NoteLetter letter, Alteration alteration, int octave)
        : this(new NaturalPitch(letter, octave), alteration) { }

    public NotePitch(NoteLetter letter, int octave)
        : this(new NaturalPitch(letter, octave)) { }

    public NotePitch(NoteClass @class, int octave)
        : this(new NaturalPitch(@class.Letter, octave), @class.Alteration) { }

    public NaturalPitch NaturalNote => natural;
    public int Octave => natural.Octave;
    public NoteLetter Letter => natural.Letter;
    public NoteClass Class => new(Letter, Alteration);
    public ChromaticClass ChromaticClass => chromatic.Class;
    public Alteration Alteration => AlterationEnum.FromChromaticDelta(chromatic.Value - natural.ToChromatic().Value);

    public ChromaticPitch ToChromatic() => chromatic;

    public NotePitch Flat() => new(natural, chromatic - 1);
    public NotePitch Sharp() => new(natural, chromatic + 1);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder(capacity: 3);
        stringBuilder.Append(Letter.ToChar());
        var alteration = Alteration;
        if (alteration != Alteration.Natural)
            stringBuilder.Append(alteration.IsSharpened() ? '#' : 'b', repeatCount: Math.Abs(alteration.ToChromaticDelta()));
        stringBuilder.Append(Octave);
        return stringBuilder.ToString();
    }

    public bool Equals(NotePitch other) => natural == other.natural && chromatic == other.chromatic;
    public override bool Equals(object? obj) => obj is NotePitch other && Equals(other);
    public override int GetHashCode() => (natural.GetHashCode() << 16) ^ chromatic.GetHashCode();
    public static bool Equals(NotePitch lhs, NotePitch rhs) => lhs.Equals(rhs);
    public static bool operator ==(NotePitch lhs, NotePitch rhs) => Equals(lhs, rhs);
    public static bool operator !=(NotePitch lhs, NotePitch rhs) => !Equals(lhs, rhs);

    public int CompareTo(NotePitch other)
    {
        int comparison = natural.CompareTo(other.natural);
        if (comparison is 0) comparison = chromatic.CompareTo(other.chromatic);
        return comparison;
    }

    public static int Compare(NotePitch lhs, NotePitch rhs) => lhs.CompareTo(rhs);
    public static bool operator >(NotePitch lhs, NotePitch rhs) => Compare(lhs, rhs) > 0;
    public static bool operator <(NotePitch lhs, NotePitch rhs) => Compare(lhs, rhs) < 0;
    public static bool operator >=(NotePitch lhs, NotePitch rhs) => Compare(lhs, rhs) >= 0;
    public static bool operator <=(NotePitch lhs, NotePitch rhs) => Compare(lhs, rhs) <= 0;

    public static implicit operator NotePitch(NaturalPitch naturalNote) => new(naturalNote);

    public static NotePitch Add(NotePitch note, Interval interval) => new(note.natural + interval.DiatonicDelta, note.chromatic + interval.ChromaticDelta);
    public static NotePitch Subtract(NotePitch note, Interval interval) => Add(note, -interval);
    public static NotePitch operator +(NotePitch lhs, Interval rhs) => Add(lhs, rhs);
    public static NotePitch operator -(NotePitch lhs, Interval rhs) => Subtract(lhs, rhs);

    public static Interval Subtract(NotePitch lhs, NotePitch rhs)
        => Interval.FromDiatonicChromaticDeltas(lhs.natural - rhs.natural, lhs.chromatic - rhs.chromatic);
    public static Interval operator -(NotePitch lhs, NotePitch rhs) => Subtract(lhs, rhs);

    public static NotePitch Parse(string str)
    {
        var match = regex.Match(str);
        if (!match.Success) throw new FormatException();
        var letter = NoteLetterEnum.TryFromChar(match.Groups["letter"].Value[0])!.Value;
        int alteration = (match.Groups["sharps"].Value?.Length ?? 0) - (match.Groups["flats"].Value?.Length ?? 0);
        int octave = int.Parse(match.Groups["octave"].Value, CultureInfo.InvariantCulture);
        return new(letter, (Alteration)alteration, octave);
    }

    public static NotePitch C(Alteration alteration, int octave) => new(NaturalPitch.C(octave), alteration);
    public static NotePitch C(int octave) => NaturalPitch.C(octave);
    public static NotePitch D(Alteration alteration, int octave) => new(NaturalPitch.D(octave), alteration);
    public static NotePitch D(int octave) => NaturalPitch.D(octave);
    public static NotePitch E(Alteration alteration, int octave) => new(NaturalPitch.E(octave), alteration);
    public static NotePitch E(int octave) => NaturalPitch.E(octave);
    public static NotePitch F(Alteration alteration, int octave) => new(NaturalPitch.F(octave), alteration);
    public static NotePitch F(int octave) => NaturalPitch.F(octave);
    public static NotePitch G(Alteration alteration, int octave) => new(NaturalPitch.G(octave), alteration);
    public static NotePitch G(int octave) => NaturalPitch.G(octave);
    public static NotePitch A(Alteration alteration, int octave) => new(NaturalPitch.A(octave), alteration);
    public static NotePitch A(int octave) => NaturalPitch.A(octave);
    public static NotePitch B(Alteration alteration, int octave) => new(NaturalPitch.B(octave), alteration);
    public static NotePitch B(int octave) => NaturalPitch.B(octave);

    public static NotePitch FromChromatic(ChromaticPitch chromatic, bool preferSharp)
        => new(NaturalPitch.FromChromatic(chromatic, roundUp: !preferSharp), chromatic);

    public static NotePitch SimplifyEnharmonically(NotePitch value)
        => FromChromatic(value.chromatic, preferSharp: value.chromatic > value.natural.ToChromatic());

    public static NotePitch GetDefaultSpellingRelativeToTonic(ChromaticPitch pitch, NoteClass tonic,
        bool augmentedFourthOverDiminishedFifth = true, bool simplifyEnharmonically = true)
    {
        int chromaticInterval = pitch.Value - (int)tonic.ToChromatic();
        var interval = Interval.FromChromaticDeltaWithDefaultSpelling(chromaticInterval, augmentedFourthOverDiminishedFifth);
        var note = (tonic + interval).WithOctave(pitch.Octave);
        if (simplifyEnharmonically) note = SimplifyEnharmonically(note);
        return note;
    }
}
