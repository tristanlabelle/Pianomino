using System;

namespace Pianomino.Theory;

/// <summary>
/// A chord degree (or extension) of a chord build from a stack of thirds.
/// </summary>
public enum TertianChordDegree : byte
{
    First,
    Third,
    Fifth,
    Seventh,
    Ninth,
    Eleventh,
    Thirteenth
}

public static class TertianChordDegreeEnum
{
    public static TertianChordDegree FromIndexValue(int value) => IntMath.EuclidianMod(value, 7) switch
    {
        0 => TertianChordDegree.First,
        1 => TertianChordDegree.Ninth,
        2 => TertianChordDegree.Third,
        3 => TertianChordDegree.Eleventh,
        4 => TertianChordDegree.Fifth,
        5 => TertianChordDegree.Thirteenth,
        6 => TertianChordDegree.Seventh,
        _ => throw new UnreachableException()
    };

    public static TertianChordDegree FromNumber(int value)
        => value > 0 ? FromIndexValue(value - 1) : throw new ArgumentOutOfRangeException(nameof(value));

    public static bool IsTriadic(this TertianChordDegree degree) => degree <= TertianChordDegree.Fifth;
    public static bool IsExtension(this TertianChordDegree degree) => degree >= TertianChordDegree.Seventh;
    public static int ToIndexValue(this TertianChordDegree degree) => (int)degree * 2;
    public static int ToNumber(this TertianChordDegree degree) => ToIndexValue(degree) + 1;
}
