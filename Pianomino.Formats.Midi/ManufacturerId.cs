using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public readonly struct ManufacturerId : IEquatable<ManufacturerId>
{
    public static ManufacturerId NonCommercial { get; } = new(0x7D);
    public static ManufacturerId UniversalSysEx_NonRealTime { get; } = new(0x7E);
    public static ManufacturerId UniversalSysEx_RealTime { get; } = new(0x7F);

    private const byte SecondByte_None = 0xFF;

    private readonly byte firstByte;
    private readonly byte secondByte; // May be SecondByte_None

    public ManufacturerId(byte singleByte)
    {
        if (singleByte == 0 || singleByte >= 0x80) throw new ArgumentOutOfRangeException(nameof(singleByte));
        this.firstByte = singleByte;
        this.secondByte = SecondByte_None;
    }

    public ManufacturerId(byte firstByte, byte secondByte)
    {
        if (firstByte > 0x80) throw new ArgumentOutOfRangeException(nameof(firstByte));
        if (firstByte > 0x80) throw new ArgumentOutOfRangeException(nameof(firstByte));
        this.firstByte = firstByte;
        this.secondByte = secondByte;
    }

    public bool IsSingleByte => secondByte == SecondByte_None;
    public byte SingleByte => IsSingleByte ? firstByte : throw new InvalidOperationException();
    public bool IsBytePair => secondByte != SecondByte_None;
    public (byte First, byte Second) BytePair => IsBytePair ? (firstByte, secondByte) : throw new InvalidOperationException();

    public bool Equals(ManufacturerId other) => firstByte == other.firstByte && secondByte == other.secondByte;
    public override bool Equals(object? obj) => obj is ManufacturerId other && Equals(other);
    public override int GetHashCode() => ((int)firstByte << 8) | secondByte;

    public override string ToString() => IsSingleByte ? $"{SingleByte:X2}" : $"{BytePair.First:X2} {BytePair.Second:X2}";

    public static ManufacturerId? TryFromData(ReadOnlySpan<byte> data)
    {
        if (data.Length < 1) return null;
        if (data[0] != 0) return new(singleByte: data[0]);
        if (data.Length < 3) return null;
        return new(firstByte: data[1], secondByte: data[2]);
    }

    public static bool Equals(ManufacturerId lhs, ManufacturerId rhs) => lhs.Equals(rhs);
    public static bool operator ==(ManufacturerId lhs, ManufacturerId rhs) => Equals(lhs, rhs);
    public static bool operator !=(ManufacturerId lhs, ManufacturerId rhs) => !Equals(lhs, rhs);
}
