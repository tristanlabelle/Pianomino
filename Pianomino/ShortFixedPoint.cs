using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Pianomino;

/// <summary>
/// A fixed point numeric type with a 16-bit whole part and a 16-bit binary decimal part.
/// </summary>
public readonly struct ShortFixedPoint : IEquatable<ShortFixedPoint>, IComparable<ShortFixedPoint>
{
    public const int RawUnit = 0x10000;
    public const int RawUnitShift = 16;

    public int RawValue { get; }

    private readonly struct RawValueTag { }
    private ShortFixedPoint(int rawValue, RawValueTag _) => RawValue = rawValue;
    public ShortFixedPoint(short value) => RawValue = value << RawUnitShift;
    public static implicit operator ShortFixedPoint(short value) => new(value);

    public bool IsZero => RawValue == 0;
    public bool IsPositive => RawValue > 0;
    public bool IsNegative => RawValue < 0;

    public (int Mantissa, sbyte Exponent) ToExponentialForm()
    {
        if (RawValue == 0) return (0, 0);
        if (RawValue == int.MinValue) return (int.MinValue, -RawUnitShift);

        int trailingZeroCount = Bits.CountTrailingZeroes(RawValue);
        return (RawValue >> trailingZeroCount, (sbyte)(trailingZeroCount - RawUnitShift));
    }

    public bool Equals(ShortFixedPoint other) => RawValue == other.RawValue;
    public override bool Equals(object? obj) => obj is ShortFixedPoint other && Equals(other);
    public override int GetHashCode() => RawValue;
    public static bool Equals(ShortFixedPoint lhs, ShortFixedPoint rhs) => lhs.Equals(rhs);
    public static bool operator ==(ShortFixedPoint lhs, ShortFixedPoint rhs) => Equals(lhs, rhs);
    public static bool operator !=(ShortFixedPoint lhs, ShortFixedPoint rhs) => !Equals(lhs, rhs);

    public bool Equals(int value) => (short)value == value && value << RawUnitShift == RawValue;
    public static bool Equals(ShortFixedPoint lhs, int rhs) => lhs.Equals(rhs);
    public static bool Equals(int lhs, ShortFixedPoint rhs) => rhs.Equals(lhs);
    public static bool operator ==(ShortFixedPoint lhs, int rhs) => Equals(lhs, rhs);
    public static bool operator ==(int lhs, ShortFixedPoint rhs) => Equals(lhs, rhs);
    public static bool operator !=(ShortFixedPoint lhs, int rhs) => !Equals(lhs, rhs);
    public static bool operator !=(int lhs, ShortFixedPoint rhs) => !Equals(lhs, rhs);

    public int CompareTo(ShortFixedPoint other) => RawValue.CompareTo(other.RawValue);
    public static int Compare(ShortFixedPoint lhs, ShortFixedPoint rhs) => lhs.CompareTo(rhs);
    public static bool operator <(ShortFixedPoint lhs, ShortFixedPoint rhs) => Compare(lhs, rhs) < 0;
    public static bool operator >(ShortFixedPoint lhs, ShortFixedPoint rhs) => Compare(lhs, rhs) > 0;
    public static bool operator <=(ShortFixedPoint lhs, ShortFixedPoint rhs) => Compare(lhs, rhs) <= 0;
    public static bool operator >=(ShortFixedPoint lhs, ShortFixedPoint rhs) => Compare(lhs, rhs) >= 0;

    public double ToDouble() => (double)RawValue / RawUnit;
    public static explicit operator double(ShortFixedPoint value) => value.ToDouble();

    public float ToSingle() => (float)RawValue / RawUnit;
    public static explicit operator float(ShortFixedPoint value) => value.ToSingle();

    public override string ToString() => ToDouble().ToString();

    public static ShortFixedPoint FromRawValue(int rawValue)
        => new(rawValue, new RawValueTag());

    public static ShortFixedPoint FromInt(int value)
    {
        if ((short)value != value) throw new ArgumentOutOfRangeException(nameof(value));
        return new((short)value);
    }

    public static explicit operator ShortFixedPoint(int value) => FromInt(value);

    public static ShortFixedPoint TwoExponent(int exponent)
        => FromRawValue(exponent >= 0 ? RawUnit << exponent : RawUnit >> -exponent);

    public static ShortFixedPoint Min(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => lhs <= rhs ? lhs : rhs;
    public static ShortFixedPoint Max(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => lhs <= rhs ? rhs : lhs;

    public static ShortFixedPoint Negate(ShortFixedPoint value)
        => FromRawValue(-value.RawValue);
    public static ShortFixedPoint Add(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => FromRawValue(lhs.RawValue + rhs.RawValue);
    public static ShortFixedPoint Subtract(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => FromRawValue(lhs.RawValue - rhs.RawValue);
    public static ShortFixedPoint Multiply(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => FromRawValue((int)(lhs.RawValue * (long)rhs.RawValue >> RawUnitShift));
    public static ShortFixedPoint Divide(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => FromRawValue((int)(((long)lhs.RawValue << RawUnitShift) / rhs.RawValue));
    public static ShortFixedPoint Mod(ShortFixedPoint lhs, ShortFixedPoint rhs)
        => FromRawValue(lhs.RawValue % rhs.RawValue);
    public static ShortFixedPoint ShiftLeft(ShortFixedPoint value, int amount)
        => FromRawValue(value.RawValue & unchecked((int)0x80_00_00_00) | value.RawValue << amount & 0x7F_FF_FF_FF);
    public static ShortFixedPoint ShiftRight(ShortFixedPoint value, int amount)
        => FromRawValue(value.RawValue & unchecked((int)0x80_00_00_00) | (value.RawValue & 0x7F_FF_FF_FF) >> amount);

    public static ShortFixedPoint operator +(ShortFixedPoint value) => value;
    public static ShortFixedPoint operator -(ShortFixedPoint value) => Negate(value);
    public static ShortFixedPoint operator +(ShortFixedPoint lhs, ShortFixedPoint rhs) => Add(lhs, rhs);
    public static ShortFixedPoint operator -(ShortFixedPoint lhs, ShortFixedPoint rhs) => Subtract(lhs, rhs);
    public static ShortFixedPoint operator *(ShortFixedPoint lhs, ShortFixedPoint rhs) => Multiply(lhs, rhs);
    public static ShortFixedPoint operator /(ShortFixedPoint lhs, ShortFixedPoint rhs) => Divide(lhs, rhs);
    public static ShortFixedPoint operator %(ShortFixedPoint lhs, ShortFixedPoint rhs) => Mod(lhs, rhs);
    public static ShortFixedPoint operator <<(ShortFixedPoint lhs, int rhs) => ShiftLeft(lhs, rhs);
    public static ShortFixedPoint operator >>(ShortFixedPoint lhs, int rhs) => ShiftRight(lhs, rhs);

    public static readonly ShortFixedPoint Zero = FromRawValue(0);
    public static readonly ShortFixedPoint One = new(1);
    public static readonly ShortFixedPoint Half = FromRawValue(RawUnit / 2);
    public static readonly ShortFixedPoint Quarter = FromRawValue(RawUnit / 4);
    public static readonly ShortFixedPoint Eighth = FromRawValue(RawUnit / 8);
    public static readonly ShortFixedPoint MinValue = FromRawValue(int.MinValue);
    public static readonly ShortFixedPoint MaxValue = FromRawValue(int.MaxValue);
    public static readonly ShortFixedPoint Epsilon = FromRawValue(1);

    private static readonly Regex parseRegex = new(@"\A[+-]?[0-9]*(\.([0-9]*5)?0*)?\Z", RegexOptions.CultureInvariant);

    public static ShortFixedPoint ParseDecimal(string str)
    {
        if (!parseRegex.IsMatch(str)) throw new FormatException();
        var value = float.Parse(str, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture);
        float rawValueF = value * RawUnit;
        if (rawValueF > int.MaxValue || rawValueF < int.MinValue || rawValueF % 1.0f != 0.0f)
            throw new ArgumentOutOfRangeException();
        return FromRawValue((int)rawValueF);
    }
}
