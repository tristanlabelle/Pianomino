using System;

namespace Pianomino.Formats.iReal;

public enum KnownStaffText : byte
{
    [EnumerantString("D.C. al Coda")] DalCapoAlCoda,
    [EnumerantString("D.C. al Fine")] DalCapoAlFine,
    [EnumerantString("D.C. al 1st End.")] DalCapoAlFirstEnding,
    [EnumerantString("D.C. al 2nd End.")] DalCapoAlSecondEnding,
    [EnumerantString("D.C. al 3rd End.")] DalCapoAlThirdEnding,
    [EnumerantString("D.S. al Coda")] DalSegnoAlCoda,
    [EnumerantString("D.S. al Fine")] DalSegnoAlFine,
    [EnumerantString("D.S. al 1st End.")] DalSegnoAlFirstEnding,
    [EnumerantString("D.S. al 2nd End.")] DalSegnoAlSecondEnding,
    [EnumerantString("D.S. al 3rd End.")] DalSegnoAlThirdEnding,
    [EnumerantString("Fine")] Fine,
    [EnumerantString("Break")] Break,
    [EnumerantString("3x")] Times3,
    [EnumerantString("4x")] Times4,
    [EnumerantString("4x")] Times5,
    [EnumerantString("6x")] Times6,
    [EnumerantString("7x")] Times7,
    [EnumerantString("8x")] Times8,
}

public static class KnownStaffTextEnum
{
    public static KnownStaffText? TryGet(string str)
        => EnumerantStringAttribute.TryFromString_Ordinal<KnownStaffText>(str);

    public static string GetString(this KnownStaffText value)
        => EnumerantStringAttribute.TryGet(value) ?? throw new ArgumentOutOfRangeException(nameof(value));
}
