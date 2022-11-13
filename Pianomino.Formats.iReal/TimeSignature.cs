using Pianomino.Theory;
using System;

namespace Pianomino.Formats.iReal;

public enum TimeSignature : byte
{
    TwoTwo,
    ThreeTwo,
    TwoFour,
    ThreeFour,
    FourFour,
    FiveFour,
    SixFour,
    SevenFour,
    ThreeEight,
    FiveEight,
    SixEight,
    SevenEight,
    NineEight,
    TwelveEight
}

public static class TimeSignatureEnum
{
    public static (int Numerator, int Denominator) GetFraction(this TimeSignature value) => value switch
    {
        TimeSignature.TwoTwo => (2, 2),
        TimeSignature.ThreeTwo => (3, 2),
        >= TimeSignature.TwoFour and <= TimeSignature.SevenFour
            => (2 + (value - TimeSignature.TwoFour), 4),
        TimeSignature.ThreeEight => (3, 8),
        TimeSignature.FiveEight => (5, 8),
        TimeSignature.SixEight => (6, 8),
        TimeSignature.SevenEight => (7, 8),
        TimeSignature.NineEight => (9, 8),
        TimeSignature.TwelveEight => (12, 8),
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static StandardMeter ToStandardMeter(this TimeSignature value)
    {
        var fraction = GetFraction(value);
        return StandardMeter.FromTimeSignature(fraction.Numerator, fraction.Denominator);
    }

    public static TimeSignature? TryFromEncodedDigits(char first, char second) => (first, second) switch
    {
        ('2', '2') => TimeSignature.TwoTwo,
        ('3', '2') => TimeSignature.ThreeTwo,
        (_, '4') when first is >= '2' and <= '7' => (TimeSignature)((int)TimeSignature.TwoFour + (first - '2')),
        ('3', '8') => TimeSignature.ThreeEight,
        ('5', '8') => TimeSignature.FiveEight,
        ('6', '8') => TimeSignature.SixEight,
        ('7', '8') => TimeSignature.SevenEight,
        ('9', '8') => TimeSignature.NineEight,
        ('1', '2') => TimeSignature.TwelveEight,
        _ => null
    };

    public static (char First, char Second) ToEncodedDigits(this TimeSignature value)
    {
        var fraction = GetFraction(value);
        return fraction.Numerator == 12
            ? ('1', '2') // 12/8 is represented as T12
            : ((char)('0' + fraction.Numerator), (char)('0' + fraction.Denominator));
    }
}
