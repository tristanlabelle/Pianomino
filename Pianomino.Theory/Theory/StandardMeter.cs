using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pianomino.Theory;

/// <summary>
/// A meter defined by a note unit and count pair, often expressed as a vulgar fraction.
/// </summary>
public readonly struct StandardMeter : IEquatable<StandardMeter>
{
    public static StandardMeter Common => new(4, NoteUnit.Quarter);
    public static StandardMeter Cut => new(2, NoteUnit.Half);
    public static StandardMeter ThreeFour => new(3, NoteUnit.Quarter);
    public static StandardMeter FourFour => new(4, NoteUnit.Quarter);
    public static StandardMeter FiveFour => new(5, NoteUnit.Quarter);
    public static StandardMeter SixEight => new(6, NoteUnit.Eighth);
    public static StandardMeter NineEight => new(9, NoteUnit.Eighth);
    public static StandardMeter TwelveEight => new(12, NoteUnit.Eighth);

    private readonly byte noteCountMinusOne;
    public NoteUnit NoteUnit { get; }

    public StandardMeter(int noteCount, NoteUnit noteUnit)
    {
        if (noteUnit > NoteUnit.Whole) throw new ArgumentOutOfRangeException(nameof(noteUnit));
        this.noteCountMinusOne = checked((byte)(noteCount - 1));
        this.NoteUnit = noteUnit;
    }

    public int NoteCount => noteCountMinusOne + 1;
    public int TimeSignatureNumerator => NoteCount;
    public int TimeSignatureDenominator => 1 << -NoteUnit.Log2;
    public int TimeSignatureDenominatorLog2 => -NoteUnit.Log2;

    public ShortFixedPoint MeasureDuration => NoteUnit.Duration * (short)NoteCount;

    public BeatBreakdown GetBeats() => new(noteCountMinusOne, NoteUnit);

    public bool Equals(StandardMeter other) => noteCountMinusOne == other.noteCountMinusOne
        && NoteUnit == other.NoteUnit;
    public override bool Equals(object? obj) => obj is StandardMeter other && Equals(other);
    public override int GetHashCode() => (noteCountMinusOne << 13) ^ NoteUnit.GetHashCode();
    public static bool Equals(StandardMeter lhs, StandardMeter rhs) => lhs.Equals(rhs);
    public static bool operator ==(StandardMeter lhs, StandardMeter rhs) => Equals(lhs, rhs);
    public static bool operator !=(StandardMeter lhs, StandardMeter rhs) => !Equals(lhs, rhs);

    public override string ToString() => $"{NoteCount}/{TimeSignatureDenominator}";

    public static StandardMeter FromTimeSignature(int numerator, int denominator)
        => new(numerator, NoteUnit.FromTimeSignatureDenominator(denominator));

    public static StandardMeter FromBeats(NoteValue beatUnit, int beatCount)
    {
        if (beatUnit.DotCount > 1) throw new ArgumentOutOfRangeException(nameof(beatUnit));
        return beatUnit.DotCount == 0 ? new(beatCount, beatUnit.Unit) : new(beatCount * 3, beatUnit.Unit - 1);
    }

    private static readonly Regex parseRegex = new(@"\A(\d+)/(\d+)\Z", RegexOptions.CultureInvariant);

    public static StandardMeter ParseTimeSignature(string str)
    {
        var match = parseRegex.Match(str);
        if (!match.Success) throw new FormatException();
        return FromTimeSignature(
            int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    public readonly struct BeatBreakdown
    {
        private readonly byte noteCountMinusOne;
        private readonly NoteUnit noteUnit;

        internal BeatBreakdown(byte noteCountMinusOne, NoteUnit noteUnit)
        {
            this.noteCountMinusOne = noteCountMinusOne;
            this.noteUnit = noteUnit;
        }

        public bool IsCompound => noteCountMinusOne % 3 == 2 && noteCountMinusOne >= 5;
        public bool IsSimple => !IsCompound;

        public int NoteUnitsPerBeat => IsCompound ? 3 : 1;
        public NoteValue BeatValue => IsCompound ? new(noteUnit + 1, dotCount: 1) : new(noteUnit);
        public int BeatCount => IsCompound ? NoteCount / 3 : NoteCount;

        public bool IsDuple => BeatCount == 2;
        public bool IsTriple => BeatCount == 3;
        public bool IsQuadruple => BeatCount == 4;

        private int NoteCount => noteCountMinusOne + 1;
    }
}
