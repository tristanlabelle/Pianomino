using System;

namespace Pianomino.Theory;

/// <summary>
/// A major or minor key, or tonality.
/// </summary>
public readonly struct DiatonicKey : IEquatable<DiatonicKey>
{
    public static readonly DiatonicKey AMinor = Minor(NoteLetter.A);
    public static readonly DiatonicKey CMajor = Major(NoteLetter.C);

    public NoteClass Tonic { get; }
    public MajorOrMinor MajorOrMinor { get; }

    public DiatonicKey(NoteClass tonic, MajorOrMinor majorOrMinor)
    {
        this.Tonic = tonic;
        this.MajorOrMinor = majorOrMinor;
    }

    public static DiatonicKey Major(NoteClass tonic) => new(tonic, MajorOrMinor.Major);
    public static DiatonicKey Minor(NoteClass tonic) => new(tonic, MajorOrMinor.Minor);

    public static DiatonicKey FromMode(NoteClass tonic, DiatonicMode mode)
    {
        tonic -= Interval.FromDiatonicDelta((int)mode);
        return mode.HasMajorThird() ? Major(tonic) : Minor(tonic - Interval.MinorThird);
    }

    public static DiatonicKey FromSharpsInSignature(int sharps, MajorOrMinor majorOrMinor)
        => new(NoteClass.FromFifths(majorOrMinor == MajorOrMinor.Major ? sharps : sharps + 3), majorOrMinor);

    public int SharpsInSignature => IsMajor ? Tonic.ToFifths() : Tonic.ToFifths() - 3;
    public bool IsMajor => MajorOrMinor == MajorOrMinor.Major;
    public bool IsMinor => MajorOrMinor == MajorOrMinor.Minor;
    public DiatonicMode Mode => DiatonicModeEnum.Get(MajorOrMinor);

    public DiatonicKey GetParallel(MajorOrMinor majorOrMinor) => new(Tonic, majorOrMinor);
    public DiatonicKey GetRelative(MajorOrMinor majorOrMinor) => FromSharpsInSignature(SharpsInSignature, majorOrMinor);

    public Alteration GetNoteAlteration(NoteLetter letter)
        => (Alteration)IntMath.EuclidianDiv(SharpsInSignature + (letter switch
        {
            NoteLetter.C => 5,
            NoteLetter.D => 3,
            NoteLetter.E => 1,
            NoteLetter.F => 6,
            NoteLetter.G => 4,
            NoteLetter.A => 2,
            NoteLetter.B => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(letter))
        }), NoteLetterEnum.Count);

    public override string ToString() => Tonic.ToString() + (IsMajor ? " major" : " minor");

    public bool Equals(DiatonicKey other) => Tonic == other.Tonic && MajorOrMinor == other.MajorOrMinor;
    public override bool Equals(object? obj) => obj is DiatonicKey other && Equals(other);
    public override int GetHashCode() => (int)Tonic.GetHashCode() | ((int)MajorOrMinor * 0x10000);
    public static bool Equals(DiatonicKey lhs, DiatonicKey rhs) => lhs.Equals(rhs);
    public static bool operator ==(DiatonicKey lhs, DiatonicKey rhs) => Equals(lhs, rhs);
    public static bool operator !=(DiatonicKey lhs, DiatonicKey rhs) => !Equals(lhs, rhs);
}
