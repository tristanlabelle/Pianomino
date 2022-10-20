using System;

namespace Pianomino.Theory;

/// <summary>
/// Identifies one of the four basic triads formed by stacking thirds:
/// major, minor, diminished or augmented
/// </summary>
public enum TertianTriad : sbyte
{
    Diminished = -2,
    Minor = -1,
    Major = 0,
    Augmented = 1
}

public static class TertianTriadEnum
{
    public static bool IsValid(this TertianTriad value)
        => value >= TertianTriad.Diminished && value <= TertianTriad.Augmented;

    public static MajorOrMinor GetThirdQuality(this TertianTriad value)
        => value < TertianTriad.Major ? MajorOrMinor.Minor : MajorOrMinor.Major;

    public static IntervalClass GetThirdInterval(this TertianTriad value)
        => new(DiatonicDegree.Third, value < TertianTriad.Major ? Alteration.Flat : Alteration.Natural);

    public static IntervalQuality GetFifthQuality(this TertianTriad value) => value switch
    {
        TertianTriad.Diminished => IntervalQuality.Diminished,
        TertianTriad.Minor => IntervalQuality.Perfect,
        TertianTriad.Major => IntervalQuality.Perfect,
        TertianTriad.Augmented => IntervalQuality.Augmented,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static IntervalClass GetFifthInterval(this TertianTriad value)
        => new(DiatonicDegree.Fifth, value switch
        {
            TertianTriad.Diminished => Alteration.Flat,
            TertianTriad.Minor => Alteration.Natural,
            TertianTriad.Major => Alteration.Natural,
            TertianTriad.Augmented => Alteration.Sharp,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        });
}
