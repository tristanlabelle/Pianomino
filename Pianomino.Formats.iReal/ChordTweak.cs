using System;
using System.Diagnostics.CodeAnalysis;

namespace Pianomino.Formats.iReal;

public readonly struct ChordTweak : IEquatable<ChordTweak>
{
    internal readonly byte degreeNumberAndType;

    private ChordTweak(int degreeNumber, ChordTweakType type)
    {
        degreeNumberAndType = (byte)(((int)degreeNumber << 4) | (int)type);
    }

    public static ChordTweak Add(int degreeNumber)
        => degreeNumber is 3 or 6 or 9 or 11 or 13
            ? new(degreeNumber, ChordTweakType.Add)
            : throw new ArgumentException();

    public static ChordTweak Flat(int degreeNumber)
        => degreeNumber is 5 or 6 or 9 or 13
            ? new(degreeNumber, ChordTweakType.Flat)
            : throw new ArgumentException();

    public static ChordTweak Sharp(int degreeNumber)
        => degreeNumber is 5 or 6 or 9 or 11
            ? new(degreeNumber, ChordTweakType.Sharp)
            : throw new ArgumentException();

    public int DegreeNumber => degreeNumberAndType >> 4;
    public ChordTweakType Type => (ChordTweakType)(degreeNumberAndType & 0xF);

    public bool Equals(ChordTweak other) => degreeNumberAndType == other.degreeNumberAndType;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ChordTweak other && Equals(other);
    public static bool Equals(ChordTweak first, ChordTweak second) => first.Equals(second);
    public static bool operator ==(ChordTweak lhs, ChordTweak rhs) => Equals(lhs, rhs);
    public static bool operator !=(ChordTweak lhs, ChordTweak rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => degreeNumberAndType;
}

public enum ChordTweakType : byte
{
    Add,
    Flat,
    Sharp
}
