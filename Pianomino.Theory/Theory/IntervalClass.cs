using System;
using System.Diagnostics.CodeAnalysis;

namespace Pianomino.Theory;

/// <summary>
/// Represents an octave-agnostic class of intervals (degree + alteration).
/// For example, I, bIII, #V.
/// </summary>
public readonly struct IntervalClass : IEquatable<IntervalClass>
{
    public static IntervalClass Unison => DiatonicDegree.First;
    public static IntervalClass AugmentedUnison => DiatonicDegree.First.Sharp();
    public static IntervalClass MinorSecond => DiatonicDegree.Second.Flat();
    public static IntervalClass MajorSecond => DiatonicDegree.Second;
    public static IntervalClass AugmentedSecond => DiatonicDegree.Second.Sharp();
    public static IntervalClass MinorThird => DiatonicDegree.Third.Flat();
    public static IntervalClass MajorThird => DiatonicDegree.Third;
    public static IntervalClass PerfectFourth => DiatonicDegree.Fourth;
    public static IntervalClass AugmentedFourth => DiatonicDegree.Fourth.Sharp();
    public static IntervalClass DiminishedFifth => DiatonicDegree.Fifth.Flat();
    public static IntervalClass PerfectFifth => DiatonicDegree.Fifth;
    public static IntervalClass AugmentedFifth => DiatonicDegree.Fifth.Sharp();
    public static IntervalClass MinorSixth => DiatonicDegree.Sixth.Flat();
    public static IntervalClass MajorSixth => DiatonicDegree.Sixth;
    public static IntervalClass AugmentedSixth => DiatonicDegree.Sixth.Sharp();
    public static IntervalClass DiminishedSeventh => DiatonicDegree.Seventh.WithAlteration(Alteration.DoubleFlat);
    public static IntervalClass MinorSeventh => DiatonicDegree.Seventh.Flat();
    public static IntervalClass MajorSeventh => DiatonicDegree.Seventh;

    private readonly sbyte diatonicDegreeAndAlteration; // degree % 7 + alteration * 7

    public IntervalClass(DiatonicDegree diatonicDegree, Alteration alteration)
    {
        if (!diatonicDegree.IsValid()) throw new ArgumentOutOfRangeException(nameof(diatonicDegree));
        diatonicDegreeAndAlteration = checked((sbyte)(diatonicDegree.ToDelta() + alteration.ToChromaticDelta() * 7));
    }

    public IntervalClass(DiatonicDegree diatonicDegree)
    {
        if (!diatonicDegree.IsValid()) throw new ArgumentOutOfRangeException(nameof(diatonicDegree));
        diatonicDegreeAndAlteration = (sbyte)diatonicDegree.ToDelta();
    }

    public static implicit operator IntervalClass(DiatonicDegree diatonicDegree) => new(diatonicDegree);

    public DiatonicDegree DiatonicDegree => (DiatonicDegree)IntMath.EuclidianMod(diatonicDegreeAndAlteration, DiatonicDegreeEnum.Count);
    public Alteration Alteration => (Alteration)IntMath.EuclidianDiv(diatonicDegreeAndAlteration, DiatonicDegreeEnum.Count);
    public ChromaticDegree ChromaticDegree => ChromaticDegreeEnum.FromDelta(ChromaticDelta);
    public int ChromaticDelta => (int)DiatonicDegree.ToChromatic() + Alteration.ToChromaticDelta();

    public IntervalClass WithAlteration(Alteration alteration) => new(DiatonicDegree, alteration);
    public Interval WithOctave(int octave) => Interval.FromDiatonicDelta(DiatonicDegree.ToDelta() + octave * DiatonicDegreeEnum.Count, Alteration);
    public Interval WithOctaveZero() => Interval.FromDiatonicDelta(DiatonicDegree.ToDelta(), Alteration);

    public bool Equals(IntervalClass other) => diatonicDegreeAndAlteration == other.diatonicDegreeAndAlteration;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IntervalClass other && Equals(other);
    public static bool Equals(IntervalClass a, IntervalClass b) => a.Equals(b);
    public static bool operator ==(IntervalClass lhs, IntervalClass rhs) => Equals(lhs, rhs);
    public static bool operator !=(IntervalClass lhs, IntervalClass rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => diatonicDegreeAndAlteration;

    public string ToString(AccidentalStringFlags accidentalFlags) => Alteration.GetAccidentalString(accidentalFlags) + DiatonicDegree.ToNumber();
    public override string ToString() => ToString(AccidentalStringFlags.Ascii_ImplicitNatural);

    public static IntervalClass GetRelativeTo(NoteClass note, NoteClass root)
        => (note.WithOctaveZero() - root.WithOctaveZero()).Class;

    public static IntervalClass Add(IntervalClass first, IntervalClass second) => (first.WithOctaveZero() + second.WithOctaveZero()).Class;
    public static IntervalClass Subtract(IntervalClass first, IntervalClass second) => (first.WithOctaveZero() - second.WithOctaveZero()).Class;
    public static IntervalClass Negate(IntervalClass value) => (-value.WithOctaveZero()).Class;
    public static IntervalClass Multiply(IntervalClass value, int multiplier) => (value.WithOctaveZero() * multiplier).Class;
    public static IntervalClass operator +(IntervalClass lhs, IntervalClass rhs) => Add(lhs, rhs);
    public static IntervalClass operator -(IntervalClass lhs, IntervalClass rhs) => Subtract(lhs, rhs);
    public static IntervalClass operator -(IntervalClass value) => Negate(value);
    public static IntervalClass operator *(IntervalClass lhs, int rhs) => Multiply(lhs, rhs);
    public static IntervalClass operator *(int lhs, IntervalClass rhs) => Multiply(rhs, lhs);
}
