using System;

namespace Pianomino.Theory;

/// <summary>
/// Converts to and from solfege syllables.
/// </summary>
public static class Solfege
{
    public static string GetSyllable(DiatonicDegree step, bool twoLetterSol = false) => step switch
    {
        DiatonicDegree.First => "do",
        DiatonicDegree.Second => "re",
        DiatonicDegree.Third => "mi",
        DiatonicDegree.Fourth => "fa",
        DiatonicDegree.Fifth => twoLetterSol ? "so" : "sol",
        DiatonicDegree.Sixth => "la",
        DiatonicDegree.Seventh => "ti",
        _ => throw new ArgumentOutOfRangeException(nameof(step))
    };

    public static string? TryGetSyllable(Interval value, bool twoLetterSol = false) => (value.DiatonicDelta, value.ChromaticDelta) switch
    {
        (0, 0) => "do",
        (1, 1) => "ra",
        (1, 2) => "re",
        (1, 3) => "ri",
        (2, 3) => "me",
        (2, 4) => "mi",
        (3, 5) => "fa",
        (3, 6) => "fi",
        (4, 6) => "se",
        (4, 7) => twoLetterSol ? "so" : "sol",
        (4, 8) => "si",
        (5, 8) => "le",
        (5, 9) => "la",
        (6, 9) => "ta",
        (6, 10) => "te",
        (6, 11) => "ti",
        _ => null
    };

    public static Interval? TryParse(string str) => str.Trim().ToLowerInvariant() switch
    {
        "do" => Interval.Unison,
        "di" => Interval.FromDiatonicChromaticDeltas(0, 1),
        "ra" => Interval.MinorSecond,
        "re" => Interval.MajorSecond,
        "ri" => Interval.AugmentedSecond,
        "me" => Interval.MinorThird,
        "mi" => Interval.MajorThird,
        "fa" => Interval.PerfectFourth,
        "fi" => Interval.AugmentedFourth,
        "se" => Interval.DiminishedFifth,
        "so" => Interval.PerfectFifth,
        "sol" => Interval.PerfectFifth,
        "si" => Interval.AugmentedFifth,
        "le" => Interval.MinorSixth,
        "la" => Interval.MajorSixth,
        "li" => Interval.AugmentedSixth,
        "ta" => Interval.DiminishedSeventh,
        "te" => Interval.MinorSeventh,
        "ti" => Interval.MajorSeventh,
        _ => null
    };
}
