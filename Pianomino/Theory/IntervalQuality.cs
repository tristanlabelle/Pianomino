using System;

namespace Pianomino.Theory;

public enum IntervalQuality : sbyte
{
    DoublyDiminished = -3,
    Diminished = -2,
    Minor = -1,
    Perfect = 0,
    Major = 1,
    Augmented = 2,
    DoublyAugmented = 3,
}

public static class IntervalQualityEnum
{
    public static char? GetChar(this IntervalQuality quality) => quality switch
    {
        IntervalQuality.Diminished => 'd',
        IntervalQuality.Minor => 'm',
        IntervalQuality.Perfect => 'P',
        IntervalQuality.Major => 'M',
        IntervalQuality.Augmented => 'A',
        _ => null
    };

    public static string GetPrefix(this IntervalQuality quality) => quality switch
    {
        IntervalQuality.Diminished => "d",
        IntervalQuality.Minor => "m",
        IntervalQuality.Perfect => "P",
        IntervalQuality.Major => "M",
        IntervalQuality.Augmented => "A",
        < 0 => new string('d', IntervalQuality.Diminished - quality + 1),
        _ => new string('A', quality - IntervalQuality.Augmented + 1)
    };

    public static bool IsValidFromPerfect(this IntervalQuality quality)
        => quality is not IntervalQuality.Major and not IntervalQuality.Minor;

    public static bool IsValidFromMajor(this IntervalQuality quality)
        => quality is not IntervalQuality.Perfect;

    public static bool IsValid(this IntervalQuality quality, bool fromPerfect)
        => fromPerfect ? IsValidFromPerfect(quality) : IsValidFromMajor(quality);

    public static Alteration GetPerfectAlteration(this IntervalQuality quality)
        => AlterationEnum.FromChromaticDelta(quality switch
        {
            IntervalQuality.Perfect => 0,
            IntervalQuality.Major or IntervalQuality.Minor => throw new ArgumentOutOfRangeException(nameof(quality)),
            <= IntervalQuality.Diminished => (quality - IntervalQuality.Diminished) - 1,
            _ => (quality - IntervalQuality.Augmented) + 1
        });

    public static Alteration GetMajorAlteration(this IntervalQuality quality)
        => AlterationEnum.FromChromaticDelta(quality switch
        {
            IntervalQuality.Major => 0,
            IntervalQuality.Minor => -1,
            IntervalQuality.Perfect => throw new ArgumentOutOfRangeException(nameof(quality)),
            <= IntervalQuality.Diminished => (quality - IntervalQuality.Diminished) - 2,
            _ => (quality - IntervalQuality.Augmented) + 1
        });

    public static Alteration GetAlteration(this IntervalQuality quality, bool fromPerfect)
        => fromPerfect ? GetPerfectAlteration(quality) : GetMajorAlteration(quality);

    public static IntervalQuality FromPerfectAlteration(Alteration alteration) => alteration switch
    {
        < Alteration.Natural => IntervalQuality.Diminished - (sbyte)(alteration.ToChromaticDelta() + 1),
        Alteration.Natural => IntervalQuality.Perfect,
        > Alteration.Natural => IntervalQuality.Augmented + (sbyte)(alteration.ToChromaticDelta() - 1)
    };

    public static IntervalQuality FromMajorAlteration(Alteration alteration) => alteration switch
    {
        <= Alteration.DoubleFlat => IntervalQuality.Diminished - (sbyte)(alteration.ToChromaticDelta() + 2),
        Alteration.Flat => IntervalQuality.Minor,
        Alteration.Natural => IntervalQuality.Major,
        >= Alteration.Sharp => IntervalQuality.Augmented + (sbyte)(alteration.ToChromaticDelta() - 1)
    };

    public static IntervalQuality FromAlteration(Alteration alteration, bool fromPerfect)
        => fromPerfect ? FromPerfectAlteration(alteration) : FromMajorAlteration(alteration);
}
