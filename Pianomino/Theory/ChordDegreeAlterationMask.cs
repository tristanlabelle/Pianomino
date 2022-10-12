using System;

namespace Pianomino.Theory;

/// <summary>
/// Flags for one or more alteration that a chord degree can have.
/// </summary>
[Flags]
public enum ChordDegreeAlterationMask : byte
{
    /// <summary>
    /// Indicates the absence of the chord degree.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a doubly flattened chord degree (bb7).
    /// </summary>
    DoubleFlat = 1 << 0,

    /// <summary>
    /// Indicates a flattened chord degree (b5, b6, b7, b9, b13).
    /// </summary>
    Flat = 1 << 1,

    /// <summary>
    /// Indicates a natural chord degree.
    /// </summary>
    Natural = 1 << 2,

    /// <summary>
    /// Indicates a raised chord degree (#5, #6, #9, #11, #13).
    /// </summary>
    Sharp = 1 << 3,
}

public static class ChordDegreeAlterationMaskEnum
{
    public static ChordDegreeAlterationMask? TryGet(Alteration value) => value switch
    {
        Alteration.DoubleFlat => ChordDegreeAlterationMask.DoubleFlat,
        Alteration.Flat => ChordDegreeAlterationMask.Flat,
        Alteration.Natural => ChordDegreeAlterationMask.Natural,
        Alteration.Sharp => ChordDegreeAlterationMask.Sharp,
        _ => null
    };

    public static ChordDegreeAlterationMask Get(Alteration value)
        => TryGet(value) ?? throw new ArgumentOutOfRangeException(nameof(value));

    public static ChordDegreeAlterationMask Get(Alteration? value)
        => value is null ? ChordDegreeAlterationMask.None : Get(value.Value);

    public static bool HasAll(this ChordDegreeAlterationMask value, ChordDegreeAlterationMask mask)
        => (value & mask) == mask;
    public static bool HasAny(this ChordDegreeAlterationMask value, ChordDegreeAlterationMask mask)
        => (value & mask) != 0;

    public static bool IsFlat(this ChordDegreeAlterationMask value) => value == ChordDegreeAlterationMask.Flat;
    public static bool IsNatural(this ChordDegreeAlterationMask value) => value == ChordDegreeAlterationMask.Natural;
    public static bool IsSharp(this ChordDegreeAlterationMask value) => value == ChordDegreeAlterationMask.Sharp;

    public static bool HasFlat(this ChordDegreeAlterationMask value) => (value & ChordDegreeAlterationMask.Flat) != 0;
    public static bool HasNatural(this ChordDegreeAlterationMask value) => (value & ChordDegreeAlterationMask.Natural) != 0;
    public static bool HasSharp(this ChordDegreeAlterationMask value) => (value & ChordDegreeAlterationMask.Sharp) != 0;

    public static bool HasNone(this ChordDegreeAlterationMask value) => value == ChordDegreeAlterationMask.None;
    public static bool HasAny(this ChordDegreeAlterationMask value) => value != ChordDegreeAlterationMask.None;

    public static int GetCount(this ChordDegreeAlterationMask value)
        => Bits.CountOnes((byte)value);

    public static bool HasSingle(this ChordDegreeAlterationMask value)
        => GetCount(value) == 1;
}
