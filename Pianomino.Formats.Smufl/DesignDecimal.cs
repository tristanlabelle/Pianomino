using System;
using System.Diagnostics;

namespace Pianomino.Formats.Smufl;

/// <summary>
/// A signed base-10 floating point number supporting up to 8 significant digits and 7 decimal point positions.
/// For example, it can represent -0.0000001, 12345678 and 1.2345678.
/// </summary>
public readonly struct DesignDecimal : IEquatable<DesignDecimal>
{
    public enum Precision : byte
    {
        Exact,
        Truncated,
        Capped
    }

    public static readonly DesignDecimal Zero = default;
    public static readonly DesignDecimal One = new(1);
    public static readonly DesignDecimal Epsilon = new(1, MinExponent);
    public static readonly DesignDecimal MaxValue = new(99_999_999);
    public static readonly DesignDecimal MinValue = new(-99_999_999);

    public const int SignificantDigitCount = 8; // floor(log10(2^28))
    public const sbyte MinExponent = -7;
    public const sbyte MaxExponent = 0;
    private const int MantissaShift = 3;
    private const int MantissaCap = 100_000_000;
    private const int ExponentMask = 0x7;

    // 0xAAAAAAAB: 2's complement mantissa and negated exponent field (0-7 = 0 to -7)
    // The mantissa should never be a multiple of 10 unless the exponent is zero.
    private readonly int mantissaAndNegatedExponent;

    private struct RawValueTag { }
    private DesignDecimal(int rawValue, RawValueTag _)
    {
        this.mantissaAndNegatedExponent = rawValue;
    }

    public DesignDecimal(int mantissa, sbyte exponent, out Precision precision)
    {
        mantissaAndNegatedExponent = ToRawValue(mantissa, exponent, out precision);
    }

    private static int ToRawValue(int mantissa, int exponent, out Precision precision)
    {
        precision = Precision.Exact;

        if (mantissa == 0) return 0;

        while (exponent < 0 && Math.DivRem(mantissa, 10, out int rem) is int div && rem == 0)
        {
            mantissa = div;
            exponent++;
        }

        if ((mantissa % MantissaCap) != mantissa)
        {
            if (exponent == 0)
            {
                precision = Precision.Capped;
                return (mantissa > 0 ? MaxValue : MinValue).mantissaAndNegatedExponent;
            }

            mantissa /= 10;
            exponent++;
            precision = Precision.Truncated;
        }

        while (exponent < MinExponent)
        {
            mantissa /= 10;
            precision = Precision.Truncated;
            exponent++;

            if (mantissa == 0) return 0;
        }

        while (exponent > 0)
        {
            var newMantissa = mantissa * 10;
            if (newMantissa % MantissaCap != mantissa)
            {
                precision = Precision.Capped;
                return (mantissa > 0 ? MaxValue : MinValue).mantissaAndNegatedExponent;
            }

            mantissa = newMantissa;
            exponent--;
        }

        Debug.Assert(exponent is >= MinExponent and <= 0);
        return (mantissa << MantissaShift) | (-exponent);
    }

    public DesignDecimal(int mantissa, sbyte exponent = 0, bool allowTruncate = true, bool allowCap = false)
    {
        mantissaAndNegatedExponent = ToRawValue(mantissa, exponent, out var precision);
        CheckPrecision(precision, allowTruncate, allowCap);
    }

    private static void CheckPrecision(Precision precision, bool allowTruncate, bool allowCap)
    {
        if (precision != Precision.Exact)
        {
            if (!allowTruncate && precision == Precision.Truncated) throw new OverflowException();
            if (!allowCap && precision == Precision.Capped) throw new OverflowException();
        }
    }

    public static explicit operator DesignDecimal(int value) => new(value);
    public static implicit operator DesignDecimal(short value) => new(value); // Implicitness allows comparisons with literals

    public DesignDecimal(decimal value, out Precision precision)
    {
        mantissaAndNegatedExponent = ToRawValue(value, out precision);
    }

    private static int ToRawValue(decimal value, out Precision precision)
    {
        if (value > 99_999_999)
        {
            precision = Precision.Capped;
            return MaxValue.mantissaAndNegatedExponent;
        }

        if (value < -99_999_999)
        {
            precision = Precision.Capped;
            return MinValue.mantissaAndNegatedExponent;
        }

        precision = Precision.Exact;
        if (value == 0) return 0;

        return Parse(value.ToString(), out precision).mantissaAndNegatedExponent;
    }

    public DesignDecimal(decimal value, bool allowTruncate = true, bool allowCap = false)
        : this(value, out var precision)
    {
        CheckPrecision(precision, allowTruncate, allowCap);
    }

    public static explicit operator DesignDecimal(decimal value) => new(value);

    public DesignDecimal(double value, out Precision precision)
    {
        mantissaAndNegatedExponent = ToRawValue(value, out precision);
    }

    private static int ToRawValue(double value, out Precision precision)
    {
        if (value > 99_999_999)
        {
            precision = Precision.Capped;
            return MaxValue.mantissaAndNegatedExponent;
        }

        if (value < -99_999_999)
        {
            precision = Precision.Capped;
            return MinValue.mantissaAndNegatedExponent;
        }

        if (double.IsNaN(value))
        {
            precision = Precision.Capped;
            return 0;
        }

        precision = Precision.Exact;
        if (value == 0) return 0;

        return Parse(value.ToString(), out precision).mantissaAndNegatedExponent;
    }

    public DesignDecimal(double value, bool allowTruncate = true, bool allowCap = false)
        : this(value, out var precision)
    {
        CheckPrecision(precision, allowTruncate, allowCap);
    }

    public static explicit operator DesignDecimal(double value) => new(value);
    public static explicit operator DesignDecimal(float value) => new((double)value);

    public int Mantissa => mantissaAndNegatedExponent >> MantissaShift;
    public int Sign => Math.Sign(Mantissa);
    public bool IsZero => mantissaAndNegatedExponent == 0;
    public int Exponent => -(mantissaAndNegatedExponent & ExponentMask);

    public bool Equals(DesignDecimal other)
    {
        MatchExponent(this, other, out var a, out var b);
        return a == b;
    }

    public override bool Equals(object? obj) => obj is DesignDecimal other && Equals(other);
    public override int GetHashCode() => throw new NotImplementedException();
    public static bool Equals(DesignDecimal lhs, DesignDecimal rhs) => lhs.Equals(rhs);
    public static bool operator ==(DesignDecimal lhs, DesignDecimal rhs) => Equals(lhs, rhs);
    public static bool operator !=(DesignDecimal lhs, DesignDecimal rhs) => !Equals(lhs, rhs);

    public double ToDouble() => Mantissa * Math.Pow(10, Exponent);
    public static explicit operator double(DesignDecimal value) => value.ToDouble();

    public float ToSingle() => (float)ToDouble();
    public static explicit operator float(DesignDecimal value) => value.ToSingle();

    public decimal ToDecimal()
    {
        var result = (decimal)Mantissa;
        var exponent = Exponent;
        while (exponent < 0)
        {
            result /= 10;
            exponent++;
        }

        return result;
    }
    public static implicit operator decimal(DesignDecimal value) => value.ToDecimal();

    public int ToInt()
    {
        int mantissa = Mantissa;
        int exponent = Exponent;
        while (exponent < 0)
        {
            mantissa /= 10;
            exponent++;
        }
        return mantissa;
    }
    public static explicit operator int(DesignDecimal value) => value.ToInt();

    public override string ToString()
    {
        var mantissa = Mantissa;
        if (mantissa == 0) return "0";

        bool negative = mantissa < 0;
        var chars = new char[SignificantDigitCount + 2]; // Max digits plus minus sign and dot
        int charsStartIndex = chars.Length;
        int decimalPlace = Exponent;
        while (mantissa != 0 || decimalPlace < 1)
        {
            int digit = Math.Abs(mantissa % 10);
            chars[--charsStartIndex] = (char)('0' + digit);
            if (decimalPlace == -1) chars[--charsStartIndex] = '.';
            decimalPlace++;
            mantissa /= 10;
        }

        if (negative) chars[--charsStartIndex] = '-';

        return new string(chars, charsStartIndex, chars.Length - charsStartIndex);
    }

    public static DesignDecimal Parse(string str, out Precision precision)
    {
        if (str.Length == 0 || str[^1] == '.')
            throw new FormatException();

        bool isNegative = false;
        bool hasDigits = false;
        bool hasPoint = false;
        byte negativeExponent = 0;
        int mantissa = 0;
        int trailingZeroCount = 0;
        for (int i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (c == '-' && i == 0) isNegative = true;
            else if (c == '.' && !hasPoint && hasDigits) hasPoint = true;
            else if (c >= '0' && c <= '9')
            {
                hasDigits = true;

                int digit = c - '0';
                if (hasPoint)
                {
                    if (c == '0')
                    {
                        trailingZeroCount++;
                        continue;
                    }

                    // "Realize" trailing zeroes
                    negativeExponent += (byte)trailingZeroCount;
                    while (trailingZeroCount > 0)
                    {
                        mantissa *= 10;
                        trailingZeroCount--;
                    }

                    negativeExponent++;
                }

                mantissa *= 10;
                mantissa += digit;
            }
            else throw new FormatException();
        }

        if (isNegative) mantissa *= -1;

        return new(mantissa, (sbyte)(-negativeExponent), out precision);
    }

    public static DesignDecimal Parse(string str, bool allowTruncate = true, bool allowCap = false)
    {
        var result = Parse(str, out var precision);
        CheckPrecision(precision, allowTruncate, allowCap);
        return result;
    }

    public static int MatchExponent(DesignDecimal lhs, DesignDecimal rhs,
        out long newLhsMantissa, out long newRhsMantissa)
    {
        newLhsMantissa = lhs.Mantissa;
        newRhsMantissa = rhs.Mantissa;
        int lhsExponent = lhs.Exponent;
        int rhsExponent = rhs.Exponent;

        while (lhsExponent > rhsExponent)
        {
            newLhsMantissa *= 10;
            lhsExponent--;
        }

        while (rhsExponent > lhsExponent)
        {
            newRhsMantissa *= 10;
            rhsExponent--;
        }

        Debug.Assert(lhsExponent == rhsExponent);
        return lhsExponent;
    }

    public static bool IsMantissaInRange(int mantissa)
    {
        return mantissa != int.MinValue && Math.Abs(mantissa) < MantissaCap;
    }
}
