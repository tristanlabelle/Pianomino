using System;

namespace Pianomino.Formats.iReal;

public enum Barline : byte
{
    Single,
    OpeningDouble,
    ClosingDouble,
    OpeningRepeat,
    ClosingRepeat,
    Final
}

public static class BarlineEnum
{
    public static Barline? TryFromChar(char c) => c switch
    {
        '|' => Barline.Single,
        '[' => Barline.OpeningDouble,
        ']' => Barline.ClosingDouble,
        '{' => Barline.OpeningRepeat,
        '}' => Barline.ClosingRepeat,
        'Z' => Barline.Final,
        _ => null
    };

    public static char ToChar(this Barline barline) => barline switch
    {
        Barline.Single => '|',
        Barline.OpeningDouble => '[',
        Barline.ClosingDouble => ']',
        Barline.OpeningRepeat => '{',
        Barline.ClosingRepeat => '}',
        Barline.Final => 'Z',
        _ => throw new ArgumentOutOfRangeException(nameof(barline))
    };
}
