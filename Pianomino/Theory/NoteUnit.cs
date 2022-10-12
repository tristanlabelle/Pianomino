using System;
using System.Runtime.InteropServices;

namespace Pianomino.Theory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct NoteUnit : IEquatable<NoteUnit>, IComparable<NoteUnit>
{
    private readonly sbyte log2;

    private NoteUnit(sbyte log2) => this.log2 = log2;

    public int Log2 => log2;
    public ShortFixedPoint Duration => ShortFixedPoint.TwoExponent(log2);
    public bool IsValidTimeSignatureUnit => log2 <= 0;
    public bool ImpliesNoteStem => log2 <= -2;
    public int ImpliedNoteFlagCount => Math.Max(0, -2 - log2);

    public int? AsTimeSignatureDenominator() => IsValidTimeSignatureUnit ? (1 << -Log2) : null;
    public int ToTimeSignatureDenominator() => AsTimeSignatureDenominator() ?? throw new InvalidOperationException();

    public NoteValue ToNoteValue() => new(this);
    public NoteValue ToNoteValue(int dotCount) => new(this, dotCount);

    public override string ToString() => log2 < 0 ? "1/" + (1 << -log2) : (1 << log2).ToString();

    public bool Equals(NoteUnit other) => log2 == other.log2;
    public override bool Equals(object? obj) => obj is NoteUnit other && Equals(other);
    public override int GetHashCode() => log2;
    public static bool Equals(NoteUnit lhs, NoteUnit rhs) => lhs.Equals(rhs);
    public static bool operator ==(NoteUnit lhs, NoteUnit rhs) => Equals(lhs, rhs);
    public static bool operator !=(NoteUnit lhs, NoteUnit rhs) => !Equals(lhs, rhs);

    public int CompareTo(NoteUnit other) => log2.CompareTo(other.log2);
    public static int Compare(NoteUnit lhs, NoteUnit rhs) => lhs.CompareTo(rhs);
    public static bool operator >(NoteUnit lhs, NoteUnit rhs) => Compare(lhs, rhs) > 0;
    public static bool operator <(NoteUnit lhs, NoteUnit rhs) => Compare(lhs, rhs) < 0;
    public static bool operator >=(NoteUnit lhs, NoteUnit rhs) => Compare(lhs, rhs) >= 0;
    public static bool operator <=(NoteUnit lhs, NoteUnit rhs) => Compare(lhs, rhs) <= 0;

    public static NoteUnit Add(NoteUnit unit, int delta)
        => new(checked((sbyte)(unit.log2 + delta)));
    public static NoteUnit Subtract(NoteUnit unit, int delta) => Add(unit, -delta);
    public static NoteUnit operator +(NoteUnit unit, int delta) => Add(unit, delta);
    public static NoteUnit operator -(NoteUnit unit, int delta) => Subtract(unit, delta);

    public static NoteUnit FromLog2(int value) => new(checked((sbyte)value));

    public static NoteUnit FromTimeSignatureDenominator(int value)
    {
        if (value <= 0 || !IntMath.IsPowerOfTwo((uint)value))
            throw new ArgumentOutOfRangeException(nameof(value));
        return FromLog2(-IntMath.Log2(value));
    }

    public static string? TryGetUSEnglishName(NoteUnit value) => value.Log2 switch
    {
        0 => "whole",
        -1 => "half",
        -2 => "quarter",
        -3 => "eighth",
        -4 => "sixteenth",
        -5 => "thirty-second",
        -6 => "sixty-fourth",
        _ => null
    };

    public static readonly NoteUnit DoubleWhole = new(1);
    public static readonly NoteUnit Whole = new(0);
    public static readonly NoteUnit Half = new(-1);
    public static readonly NoteUnit Quarter = new(-2); // Crotchet
    public static readonly NoteUnit Eighth = new(-3);
    public static readonly NoteUnit Sixteenth = new(-4);
}
