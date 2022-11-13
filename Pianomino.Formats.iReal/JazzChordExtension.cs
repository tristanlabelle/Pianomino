using Pianomino.Theory;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Pianomino.Formats.iReal;

public readonly struct JazzChordExtension : IEquatable<JazzChordExtension>
{
    public static readonly JazzChordExtension Seventh = new(TertianChordDegree.Seventh);
    public static readonly JazzChordExtension Ninth = new(TertianChordDegree.Ninth);
    public static readonly JazzChordExtension Eleventh = new(TertianChordDegree.Eleventh);
    public static readonly JazzChordExtension Thirteenth = new(TertianChordDegree.Thirteenth);
    public static readonly JazzChordExtension MajorSeventh = Major(TertianChordDegree.Seventh);
    public static readonly JazzChordExtension MajorNinth = Major(TertianChordDegree.Ninth);
    public static readonly JazzChordExtension MajorEleventh = Major(TertianChordDegree.Eleventh);
    public static readonly JazzChordExtension MajorThirteenth = Major(TertianChordDegree.Thirteenth);

    private const byte MajorBit = 0x80;
    private const byte IndexMask = 0x7F;

    private readonly byte indexAndMajorBit; // Zero = dom7

    public JazzChordExtension(TertianChordDegree degree, bool major = false)
    {
        int index = degree - TertianChordDegree.Seventh;
        if ((uint)index > 3) throw new ArgumentOutOfRangeException(nameof(degree));
        indexAndMajorBit = (byte)index;
        if (major) indexAndMajorBit |= MajorBit;
    }

    public static JazzChordExtension Major(TertianChordDegree degree)
        => new(degree, major: true);

    public TertianChordDegree Degree => TertianChordDegree.Seventh + (byte)(indexAndMajorBit & IndexMask);
    public bool IsMajor => (indexAndMajorBit & MajorBit) == MajorBit;

    public ChordDegreeAlterationMask NinthAlterationMask
        => Degree >= TertianChordDegree.Ninth ? ChordDegreeAlterationMask.Natural : ChordDegreeAlterationMask.None;
    public ChordDegreeAlterationMask EleventhAlterationMask
        => Degree >= TertianChordDegree.Eleventh ? ChordDegreeAlterationMask.Natural : ChordDegreeAlterationMask.None;
    public ChordDegreeAlterationMask ThirteenthAlterationMask
        => Degree >= TertianChordDegree.Thirteenth ? ChordDegreeAlterationMask.Natural : ChordDegreeAlterationMask.None;

    public Alteration GetSeventhAlteration(bool diminished)
    {
        if (diminished) return Alteration.DoubleFlat;
        return IsMajor ? Alteration.Natural : Alteration.Flat;
    }

    public ChordDegreeAlterationMask GetSeventhAlterationMask(bool diminished)
    {
        if (diminished) return ChordDegreeAlterationMask.DoubleFlat;
        return IsMajor ? ChordDegreeAlterationMask.Natural : ChordDegreeAlterationMask.Flat;
    }

    public bool Equals(JazzChordExtension other) => indexAndMajorBit == other.indexAndMajorBit;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is JazzChordExtension other && Equals(other);
    public static bool Equals(JazzChordExtension first, JazzChordExtension second) => first.Equals(second);
    public static bool operator ==(JazzChordExtension lhs, JazzChordExtension rhs) => Equals(lhs, rhs);
    public static bool operator !=(JazzChordExtension lhs, JazzChordExtension rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => indexAndMajorBit;

    public override string ToString() => IsMajor ? ("maj" + Degree.ToNumber()) : Degree.ToNumber().ToString();
}
