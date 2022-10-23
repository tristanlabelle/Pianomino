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

    public static string? TryGetSyllable(IntervalClass value, bool twoLetterSol = false) => (value.DiatonicDegree, value.ChromaticDelta) switch
    {
        (DiatonicDegree.First, 0) => "do",
        (DiatonicDegree.Second, 1) => "ra",
        (DiatonicDegree.Second, 2) => "re",
        (DiatonicDegree.Second, 3) => "ri",
        (DiatonicDegree.Third, 3) => "me",
        (DiatonicDegree.Third, 4) => "mi",
        (DiatonicDegree.Fourth, 5) => "fa",
        (DiatonicDegree.Fourth, 6) => "fi",
        (DiatonicDegree.Fifth, 6) => "se",
        (DiatonicDegree.Fifth, 7) => twoLetterSol ? "so" : "sol",
        (DiatonicDegree.Fifth, 8) => "si",
        (DiatonicDegree.Sixth, 8) => "le",
        (DiatonicDegree.Sixth, 9) => "la",
        (DiatonicDegree.Seventh, 9) => "ta",
        (DiatonicDegree.Seventh, 10) => "te",
        (DiatonicDegree.Seventh, 11) => "ti",
        _ => null
    };

    public static IntervalClass? TryParse(string str) => str.Trim().ToLowerInvariant() switch
    {
        "do" => IntervalClass.Unison,
        "ra" => IntervalClass.MinorSecond,
        "re" => IntervalClass.MajorSecond,
        "ri" => IntervalClass.AugmentedSecond,
        "me" => IntervalClass.MinorThird,
        "mi" => IntervalClass.MajorThird,
        "fa" => IntervalClass.PerfectFourth,
        "fi" => IntervalClass.AugmentedFourth,
        "se" => IntervalClass.DiminishedFifth,
        "so" or "sol" => IntervalClass.PerfectFifth,
        "si" => IntervalClass.AugmentedFifth,
        "le" => IntervalClass.MinorSixth,
        "la" => IntervalClass.MajorSixth,
        "li" => IntervalClass.AugmentedSixth,
        "ta" => IntervalClass.DiminishedSeventh,
        "te" => IntervalClass.MinorSeventh,
        "ti" => IntervalClass.MajorSeventh,
        _ => null
    };
}
