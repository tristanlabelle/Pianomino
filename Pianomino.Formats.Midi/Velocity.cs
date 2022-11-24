using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pianomino.Formats.Midi;

/// <summary>
/// Represents a note on/off velocity, or aftertouch pressure value.
/// </summary>
public readonly struct Velocity : IEquatable<Velocity>, IComparable<Velocity>
{
    public const float ByteToFloatFactor = 1f / 127f;
    public const float FloatToByteMultiplier = 127.999992371f; // 128 minus epsilon
    public const byte MinByte = 0;
    public const byte MaxByte = 127;
    public static Velocity Zero => new(0);
    public static Velocity Off => Zero;
    public static Velocity MezzoForte => new(64);
    public static Velocity MaxValue => new(MaxByte);

    public byte Value { get; }

    public Velocity(byte value)
    {
        if (value >= 0x80) throw new ArgumentOutOfRangeException(nameof(value));
        this.Value = value;
    }

    public bool IsZero => Value == 0;
    public float FloatValue => Value * ByteToFloatFactor;

    public int CompareTo(Velocity other) => Value.CompareTo(other.Value);

    public bool Equals(Velocity other) => other.Value == Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Velocity other && Equals(other);
    public override int GetHashCode() => Value;

    public override string ToString() => Value switch
    {
        0 => "0",
        _ => FloatValue.ToString("F2", CultureInfo.InvariantCulture)
    };

    public static float ByteToFloat(byte value) => value * ByteToFloatFactor;
    public static byte FloatToByte(float value) => (byte)Math.Clamp(value * FloatToByteMultiplier, 0f, FloatToByteMultiplier);
    public static Velocity FromFloat(float value) => new(FloatToByte(value));

    public static int Compare(Velocity lhs, Velocity rhs) => lhs.CompareTo(rhs);
    public static bool Equals(Velocity lhs, Velocity rhs) => lhs.Equals(rhs);
    public static bool operator <(Velocity lhs, Velocity rhs) => lhs.Value < rhs.Value;
    public static bool operator <=(Velocity lhs, Velocity rhs) => lhs.Value <= rhs.Value;
    public static bool operator >(Velocity lhs, Velocity rhs) => lhs.Value > rhs.Value;
    public static bool operator >=(Velocity lhs, Velocity rhs) => lhs.Value >= rhs.Value;
    public static bool operator ==(Velocity lhs, Velocity rhs) => lhs.Value == rhs.Value;
    public static bool operator !=(Velocity lhs, Velocity rhs) => lhs.Value != rhs.Value;

    public static explicit operator Velocity(byte Value) => new(Value);
    public static implicit operator byte(Velocity Velocity) => Velocity.Value;
}
