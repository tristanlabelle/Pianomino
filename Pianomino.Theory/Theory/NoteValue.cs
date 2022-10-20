using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Pianomino.Theory;

/// <summary>
/// The value (duration) of a note, with or without augmentation dots.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 2)]
public readonly struct NoteValue : IEquatable<NoteValue>, IComparable<NoteValue>
{
    public NoteUnit Unit { get; }
    private readonly byte dotCount;

    public NoteValue(NoteUnit unit)
    {
        this.Unit = unit;
        this.dotCount = 0;
    }

    public NoteValue(NoteUnit unit, int dotCount)
    {
        this.Unit = unit;
        this.dotCount = checked((byte)dotCount);
    }

    public int DotCount => dotCount;
    public bool ImpliesStem => Unit.ImpliesNoteStem;
    public int ImpliedFlagCount => Unit.ImpliedNoteFlagCount;

    public ShortFixedPoint Duration
    {
        get
        {
            // It's interesting to note that the binary representation of note value durations
            // will always have a single contiguous sequence of ones, so we can be clever here.
            var rawValue = ((2 << dotCount) - 1) << (ShortFixedPoint.RawUnitShift + Unit.Log2 - dotCount);
            return ShortFixedPoint.FromRawValue(rawValue);
        }
    }

    public NoteValue Dot() => new(Unit, dotCount + 1);

    public string GetUSEnglishString()
    {
        var unitName = (NoteUnit.TryGetUSEnglishName(Unit) ?? Unit.ToString());
        if (DotCount == 0) return unitName;

        return DotCount switch
        {
            1 => "dotted ",
            2 => "double dotted ",
            3 => "triple dotted ",
            int n => $"{n}-dotted "
        } + unitName;
    }

    public override string ToString()
        => Unit.ToString() + (dotCount == 0 ? string.Empty : new string('.', DotCount));

    public bool Equals(NoteValue other) => Unit == other.Unit && dotCount == other.dotCount;
    public override bool Equals(object? obj) => obj is NoteValue other && Equals(other);
    public override int GetHashCode() => (dotCount << 13) ^ Unit.GetHashCode();
    public static bool Equals(NoteValue lhs, NoteValue rhs) => lhs.Equals(rhs);
    public static bool operator ==(NoteValue lhs, NoteValue rhs) => Equals(lhs, rhs);
    public static bool operator !=(NoteValue lhs, NoteValue rhs) => !Equals(lhs, rhs);

    public int CompareTo(NoteValue other) => Unit == other.Unit
        ? dotCount.CompareTo(other.dotCount) : Unit.CompareTo(other.Unit);
    public static int Compare(NoteValue lhs, NoteValue rhs) => lhs.CompareTo(rhs);
    public static bool operator >(NoteValue lhs, NoteValue rhs) => Compare(lhs, rhs) > 0;
    public static bool operator <(NoteValue lhs, NoteValue rhs) => Compare(lhs, rhs) < 0;
    public static bool operator >=(NoteValue lhs, NoteValue rhs) => Compare(lhs, rhs) >= 0;
    public static bool operator <=(NoteValue lhs, NoteValue rhs) => Compare(lhs, rhs) <= 0;

    public static ShortFixedPoint GetDotsDurationMultiplier(int dotCount)
    {
        if ((uint)dotCount > 8) throw new ArgumentOutOfRangeException(nameof(dotCount));
        return new ShortFixedPoint((short)((1 << (dotCount + 1)) - 1)) * ShortFixedPoint.TwoExponent((sbyte)(-dotCount));
    }

    public static ImmutableArray<NoteValue> SplitDuration(ShortFixedPoint duration)
    {
        if (duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
        if (duration == 0) return ImmutableArray<NoteValue>.Empty;

        // Minimize allocations for typical 1 and 2 cases.
        var first = GetLargestInDuration_Unchecked(duration);
        duration -= first.Duration;
        if (duration == 0) return ImmutableArray.Create(first);

        var second = GetLargestInDuration_Unchecked(duration);
        duration -= second.Duration;
        if (duration == 0) return ImmutableArray.Create(first, second);

        var arrayBuilder = ImmutableArray.CreateBuilder<NoteValue>(3);
        arrayBuilder.Add(first);
        arrayBuilder.Add(second);
        do
        {
            var noteValue = GetLargestInDuration_Unchecked(duration);
            arrayBuilder.Add(noteValue);
            duration -= noteValue.Duration;
        } while (duration > 0);

        arrayBuilder.Capacity = arrayBuilder.Count;
        return arrayBuilder.MoveToImmutable();
    }

    public static NoteValue GetLargestInDuration(ShortFixedPoint duration)
    {
        if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
        return GetLargestInDuration_Unchecked(duration);
    }

    private static NoteValue GetLargestInDuration_Unchecked(ShortFixedPoint duration)
    {
        var rawValue = duration.RawValue;
        int msbIndex = Bits.IndexOfMostSignificant(rawValue);
        int contiguousBitCount = 1;

        while (((rawValue >> (msbIndex - contiguousBitCount)) & 1) == 1)
            contiguousBitCount++;

        var noteUnit = NoteUnit.FromLog2(msbIndex - ShortFixedPoint.RawUnitShift);
        return new(noteUnit, dotCount: contiguousBitCount - 1);
    }

    public static readonly NoteValue Whole = new(NoteUnit.Whole);
    public static readonly NoteValue Half = new(NoteUnit.Half);
    public static readonly NoteValue Quarter = new(NoteUnit.Quarter);
    public static readonly NoteValue Eighth = new(NoteUnit.Eighth);
    public static readonly NoteValue Sixteenth = new(NoteUnit.Sixteenth);

    public static readonly NoteValue DottedWhole = new(NoteUnit.Whole, dotCount: 1);
    public static readonly NoteValue DottedHalf = new(NoteUnit.Half, dotCount: 1);
    public static readonly NoteValue DottedQuarter = new(NoteUnit.Quarter, dotCount: 1);
    public static readonly NoteValue DottedEighth = new(NoteUnit.Eighth, dotCount: 1);
    public static readonly NoteValue DottedSixteenth = new(NoteUnit.Sixteenth, dotCount: 1);
}
