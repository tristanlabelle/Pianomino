using System;
using System.Diagnostics.CodeAnalysis;

namespace Pianomino.Theory;

/// <summary>
/// A musical key based on church modes rather than only major/minor tonality.
/// </summary>
public readonly struct ModalKey : IEquatable<ModalKey>
{
    public NoteClass Tonic { get; }
    public DiatonicMode Mode { get; }

    public ModalKey(NoteClass tonic, DiatonicMode mode)
    {
        this.Tonic = tonic;
        this.Mode = mode;
    }

    public int SharpsInSignature => ToMajorOrMinor().SharpsInSignature;

    public DiatonicKey ToMajorOrMinor() => DiatonicKey.FromMode(Tonic, Mode);

    public bool Equals(ModalKey other) => Tonic == other.Tonic && Mode == other.Mode;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ModalKey other && Equals(other);
    public static bool Equals(ModalKey first, ModalKey second) => first.Equals(second);
    public static bool operator ==(ModalKey lhs, ModalKey rhs) => Equals(lhs, rhs);
    public static bool operator !=(ModalKey lhs, ModalKey rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => (Tonic.GetHashCode() << 16) ^ Mode.GetHashCode();

    public override string ToString() => $"{Tonic} {Mode.ToString().ToLowerInvariant()}";

    public static ModalKey FromDiatonicKey(DiatonicKey key) => new(key.Tonic, key.Mode);
    public static implicit operator ModalKey(DiatonicKey key) => FromDiatonicKey(key);
}
