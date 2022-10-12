using System;
using System.Collections.Immutable;

namespace Pianomino.Theory;

/// <summary>
/// A chord described by stacking thirds, each degree having zero or more alterations.
/// </summary>
public readonly partial struct TertianChordSpelling : IEquatable<TertianChordSpelling>
{
    private static ChordDegreeAlterationMask ToMask(Alteration? alteration)
        => ChordDegreeAlterationMaskEnum.Get(alteration);

    public const ChordDegreeAlterationMask ValidFifthsMask = ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Natural | ChordDegreeAlterationMask.Sharp;
    public const ChordDegreeAlterationMask ValidSeventhsMask = ChordDegreeAlterationMask.DoubleFlat | ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Natural;
    public const ChordDegreeAlterationMask ValidNinthsMask = ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Natural | ChordDegreeAlterationMask.Sharp;
    public const ChordDegreeAlterationMask ValidEleventhsMask = ChordDegreeAlterationMask.Natural | ChordDegreeAlterationMask.Sharp;
    public const ChordDegreeAlterationMask ValidThirteenthsMask = ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Natural | ChordDegreeAlterationMask.Sharp;

    public MajorOrMinor? Third { get; }
    public ChordDegreeAlterationMask Fifths { get; }
    public ChordDegreeAlterationMask Sevenths { get; }
    public ChordDegreeAlterationMask Ninths { get; }
    public ChordDegreeAlterationMask Elevenths { get; }
    public ChordDegreeAlterationMask Thirteenths { get; }

    public TertianChordSpelling(MajorOrMinor? third, ChordDegreeAlterationMask fifths,
        ChordDegreeAlterationMask sevenths = default, ChordDegreeAlterationMask ninths = default,
        ChordDegreeAlterationMask elevenths = default, ChordDegreeAlterationMask thirteenths = default)
    {
        if ((fifths & ValidFifthsMask) != fifths) throw new ArgumentOutOfRangeException(nameof(fifths));
        if ((sevenths & ValidSeventhsMask) != sevenths) throw new ArgumentOutOfRangeException(nameof(sevenths));
        if ((ninths & ValidNinthsMask) != ninths) throw new ArgumentOutOfRangeException(nameof(ninths));
        if ((elevenths & ValidEleventhsMask) != elevenths) throw new ArgumentOutOfRangeException(nameof(elevenths));
        if ((thirteenths & ValidThirteenthsMask) != thirteenths) throw new ArgumentOutOfRangeException(nameof(thirteenths));

        if ((ninths.HasAll(ChordDegreeAlterationMask.Sharp) && third == MajorOrMinor.Minor)
            || (elevenths.HasAll(ChordDegreeAlterationMask.Sharp) && fifths.HasAll(ChordDegreeAlterationMask.Flat))
            || (fifths.HasAll(ChordDegreeAlterationMask.Sharp) && thirteenths.HasAll(ChordDegreeAlterationMask.Flat))
            || (thirteenths.HasAll(ChordDegreeAlterationMask.Natural) && sevenths.HasAll(ChordDegreeAlterationMask.DoubleFlat))
            || (thirteenths.HasAll(ChordDegreeAlterationMask.Sharp) && sevenths.HasAny(ChordDegreeAlterationMask.DoubleFlat | ChordDegreeAlterationMask.Flat)))
        {
            throw new ArgumentException();
        }

        this.Third = third;
        this.Fifths = fifths;
        this.Sevenths = sevenths;
        this.Ninths = ninths;
        this.Elevenths = elevenths;
        this.Thirteenths = thirteenths;
    }

    public TertianChordSpelling(MajorOrMinor? third, IntervalQuality? fifth,
        IntervalQuality? seventh = default, IntervalQuality? ninth = default,
        IntervalQuality? eleventh = default, IntervalQuality? thirteenth = default)
        : this(third, ToAlterations(fifth, perfect: true), ToAlterations(seventh, perfect: false),
              ToAlterations(ninth, perfect: false), ToAlterations(eleventh, perfect: true), ToAlterations(thirteenth, perfect: false))
    { }

    public TertianChordSpelling(MajorOrMinor? third, Alteration? fifth, Alteration? seventh = null,
        Alteration? ninth = null, Alteration? eleventh = null, Alteration? thirteenth = null)
        : this(third, ToMask(fifth), ToMask(seventh), ToMask(ninth), ToMask(eleventh), ToMask(thirteenth)) { }

    private static ChordDegreeAlterationMask ToAlterations(IntervalQuality? quality, bool perfect)
    {
        if (quality is null) return ChordDegreeAlterationMask.None;
        return ToMask(quality.Value.GetAlteration(perfect));
    }

    public bool IsTriad => Third.HasValue && Fifths.HasSingle() && Sevenths.HasNone()
        && Ninths.HasNone() && Elevenths.HasNone() && Thirteenths.HasNone();

    public int ToneCount => 1 + (Third.HasValue ? 1 : 0) + Fifths.GetCount() + Sevenths.GetCount()
        + Ninths.GetCount() + Elevenths.GetCount() + Thirteenths.GetCount();

    public bool Equals(TertianChordSpelling other) => Third == other.Third && Fifths == other.Fifths
        && Sevenths == other.Sevenths && Ninths == other.Ninths
        && Elevenths == other.Elevenths && Thirteenths == other.Thirteenths;
    public override bool Equals(object? obj) => obj is TertianChordSpelling other && Equals(other);
    public static bool Equals(TertianChordSpelling lhs, TertianChordSpelling rhs) => lhs.Equals(rhs);
    public static bool operator ==(TertianChordSpelling lhs, TertianChordSpelling rhs) => Equals(lhs, rhs);
    public static bool operator !=(TertianChordSpelling lhs, TertianChordSpelling rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => throw new NotImplementedException();

    public DiatonicDegree? TryGetStep(ChromaticDegree tone) => tone switch
    {
        ChromaticDegree.P1 => DiatonicDegree.First,
        ChromaticDegree.m2 => Ninths.HasAll(ChordDegreeAlterationMask.Flat) ? DiatonicDegree.Second : null,
        ChromaticDegree.M2 => Ninths.HasAll(ChordDegreeAlterationMask.Natural) ? DiatonicDegree.Second : null,
        ChromaticDegree.m3 => Ninths.HasAll(ChordDegreeAlterationMask.Sharp) ? DiatonicDegree.Second
            : Third == MajorOrMinor.Minor ? DiatonicDegree.Third : null,
        ChromaticDegree.M3 => Third == MajorOrMinor.Major ? DiatonicDegree.Third : null,
        ChromaticDegree.P4 => Elevenths.HasAll(ChordDegreeAlterationMask.Natural) ? DiatonicDegree.Fourth : null,
        ChromaticDegree.TT => Elevenths.HasAll(ChordDegreeAlterationMask.Sharp) ? DiatonicDegree.Fourth
            : Fifths.HasAll(ChordDegreeAlterationMask.Flat) ? DiatonicDegree.Fifth : null,
        ChromaticDegree.P5 => Fifths.HasAll(ChordDegreeAlterationMask.Natural) ? DiatonicDegree.Fifth : null,
        ChromaticDegree.m6 => Fifths.HasAll(ChordDegreeAlterationMask.Sharp) ? DiatonicDegree.Fifth
            : Thirteenths.HasAll(ChordDegreeAlterationMask.Flat) ? DiatonicDegree.Sixth : null,
        ChromaticDegree.M6 => Thirteenths.HasAll(ChordDegreeAlterationMask.Natural) ? DiatonicDegree.Sixth
            : Sevenths.HasAll(ChordDegreeAlterationMask.DoubleFlat) ? DiatonicDegree.Seventh : null,
        ChromaticDegree.m7 => Thirteenths.HasAll(ChordDegreeAlterationMask.Sharp) ? DiatonicDegree.Sixth
            : Sevenths.HasAll(ChordDegreeAlterationMask.Flat) ? DiatonicDegree.Seventh : null,
        ChromaticDegree.M7 => Sevenths.HasAll(ChordDegreeAlterationMask.Natural) ? DiatonicDegree.Seventh : null,
        _ => throw new ArgumentOutOfRangeException(nameof(tone))
    };

    public sbyte? TryGetAlteration(ChromaticDegree tone)
        => TryGetStep(tone) is DiatonicDegree step ? (sbyte)((int)tone - (int)step.ToChromatic()) : null;

    public Interval? TryGetInterval(ChromaticDegree tone)
        => TryGetStep(tone) is DiatonicDegree step ? Interval.FromDiatonicChromaticDeltas((int)step, (int)tone) : null;

    public ToneSet ToToneSet(bool rootless = false)
    {
        ToneSet toneSet = default;
        if (!rootless) toneSet = toneSet.With(ChromaticDegree.P1);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Flat)) toneSet = toneSet.With(ChromaticDegree.m2);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Natural)) toneSet = toneSet.With(ChromaticDegree.M2);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Sharp)) toneSet = toneSet.With(ChromaticDegree.m3);
        if (Third == MajorOrMinor.Minor) toneSet = toneSet.With(ChromaticDegree.m3);
        if (Third == MajorOrMinor.Major) toneSet = toneSet.With(ChromaticDegree.M3);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Natural)) toneSet = toneSet.With(ChromaticDegree.P4);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Sharp)) toneSet = toneSet.With(ChromaticDegree.TT);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Flat)) toneSet = toneSet.With(ChromaticDegree.TT);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Natural)) toneSet = toneSet.With(ChromaticDegree.P5);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Sharp)) toneSet = toneSet.With(ChromaticDegree.m6);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Flat)) toneSet = toneSet.With(ChromaticDegree.m6);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Natural)) toneSet = toneSet.With(ChromaticDegree.M6);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Sharp)) toneSet = toneSet.With(ChromaticDegree.m7);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.DoubleFlat)) toneSet = toneSet.With(ChromaticDegree.M6);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Flat)) toneSet = toneSet.With(ChromaticDegree.m7);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Natural)) toneSet = toneSet.With(ChromaticDegree.M7);

        return toneSet;
    }

    public ImmutableArray<Interval> GetSimpleIntervals(bool includeUnison = true)
    {
        var intervals = ImmutableArray.CreateBuilder<Interval>(initialCapacity: ToneCount);

        if (includeUnison) intervals.Add(Interval.Unison);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.MinorSecond);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.MajorSecond);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.AugmentedSecond);
        if (Third == MajorOrMinor.Minor) intervals.Add(Interval.MinorThird);
        if (Third == MajorOrMinor.Major) intervals.Add(Interval.MajorThird);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.PerfectFourth);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.AugmentedFourth);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.DiminishedFifth);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.PerfectFifth);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.AugmentedFifth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.MinorSixth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.MajorSixth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.AugmentedSixth);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.DoubleFlat)) intervals.Add(Interval.DiminishedSeventh);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.MinorSeventh);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.MajorSeventh);

        intervals.Capacity = intervals.Count;
        return intervals.MoveToImmutable();
    }

    public ImmutableArray<Interval> GetCompoundIntervals(bool includeUnison = true)
    {
        var intervals = ImmutableArray.CreateBuilder<Interval>(initialCapacity: ToneCount);

        if (includeUnison) intervals.Add(Interval.Unison);
        if (Third == MajorOrMinor.Minor) intervals.Add(Interval.MinorThird);
        if (Third == MajorOrMinor.Major) intervals.Add(Interval.MajorThird);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.DiminishedFifth);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.PerfectFifth);
        if (Fifths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.AugmentedFifth);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.DoubleFlat)) intervals.Add(Interval.DiminishedSeventh);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.MinorSeventh);
        if (Sevenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.MajorSeventh);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.Octave + Interval.MinorSecond);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.Octave + Interval.MajorSecond);
        if (Ninths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.Octave + Interval.AugmentedSecond);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.Octave + Interval.PerfectFourth);
        if (Elevenths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.Octave + Interval.AugmentedFourth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Flat)) intervals.Add(Interval.Octave + Interval.MinorSixth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Natural)) intervals.Add(Interval.Octave + Interval.MajorSixth);
        if (Thirteenths.HasAll(ChordDegreeAlterationMask.Sharp)) intervals.Add(Interval.Octave + Interval.AugmentedSixth);

        intervals.Capacity = intervals.Count;
        return intervals.MoveToImmutable();
    }

    public static TertianChordSpelling Recognize(ToneSet chord)
    {
        MajorOrMinor? third = null;
        ChordDegreeAlterationMask fifths = default;
        ChordDegreeAlterationMask sevenths = default;
        ChordDegreeAlterationMask ninths = default;
        ChordDegreeAlterationMask elevenths = default;
        ChordDegreeAlterationMask thirteenths = default;

        // m2 and M2
        if (chord.Contains(ChromaticDegree.m2)) ninths |= ChordDegreeAlterationMask.Flat;
        if (chord.Contains(ChromaticDegree.M2)) ninths |= ChordDegreeAlterationMask.Natural;

        // m3 and M3
        if (chord.Contains(ChromaticDegree.M3))
        {
            third = MajorOrMinor.Major;
            if (chord.Contains(ChromaticDegree.m3)) ninths |= ChordDegreeAlterationMask.Sharp;
        }
        else if (chord.Contains(ChromaticDegree.m3))
        {
            third = MajorOrMinor.Minor;
        }

        if (chord.Contains(ChromaticDegree.P4)) elevenths |= ChordDegreeAlterationMask.Natural;

        // TT, P5, m6
        if (chord.Contains(ChromaticDegree.P5))
        {
            fifths |= ChordDegreeAlterationMask.Natural;
            if (chord.Contains(ChromaticDegree.TT)) elevenths |= ChordDegreeAlterationMask.Sharp;
            if (chord.Contains(ChromaticDegree.m6)) thirteenths |= ChordDegreeAlterationMask.Flat;
        }
        else
        {
            if (third == MajorOrMinor.Minor)
            {
                if (chord.Contains(ChromaticDegree.TT))
                {
                    // dim[b13]
                    fifths |= ChordDegreeAlterationMask.Flat;

                    if (chord.Contains(ChromaticDegree.m6))
                        thirteenths |= ChordDegreeAlterationMask.Flat;
                }
                else
                {
                    // min#5 (almost certainly a first inversion)
                    if (chord.Contains(ChromaticDegree.m6))
                        fifths |= ChordDegreeAlterationMask.Sharp;
                }
            }
            else
            {
                if (chord.Contains(ChromaticDegree.m6))
                {
                    // aug[#11]
                    fifths |= ChordDegreeAlterationMask.Sharp;

                    if (chord.Contains(ChromaticDegree.TT))
                        elevenths |= ChordDegreeAlterationMask.Sharp;
                }
                else
                {
                    // b5
                    if (chord.Contains(ChromaticDegree.TT))
                        fifths |= ChordDegreeAlterationMask.Flat;
                }
            }
        }

        // M6
        if (chord.Contains(ChromaticDegree.M6))
        {
            if (fifths == ChordDegreeAlterationMask.Flat && thirteenths == ChordDegreeAlterationMask.None)
                sevenths |= ChordDegreeAlterationMask.DoubleFlat;
            else
                thirteenths |= ChordDegreeAlterationMask.Natural;
        }

        // m7, M7
        if (chord.Contains(ChromaticDegree.M7))
        {
            sevenths |= ChordDegreeAlterationMask.Natural;
            if (chord.Contains(ChromaticDegree.m7))
            {
                if (thirteenths == ChordDegreeAlterationMask.None)
                    thirteenths |= ChordDegreeAlterationMask.Sharp;
                else
                    sevenths |= ChordDegreeAlterationMask.Flat;
            }
        }
        else if (chord.Contains(ChromaticDegree.m7))
        {
            sevenths |= ChordDegreeAlterationMask.Flat;
        }

        return new TertianChordSpelling(third, fifths, sevenths, ninths, elevenths, thirteenths);
    }

    public static int GetLikeliestRootInversionIndex(ToneSet chord)
    {
        int inversionIndex = 0;
        int bestInversionIndex = 0;
        int bestScore = GetInversionScore(chord);
        while (true)
        {
            inversionIndex++;
            var inversion = chord.GetInversion(inversionIndex);
            if (inversion == chord) break;

            var score = GetInversionScore(inversion);
            if (score > bestScore)
            {
                bestInversionIndex = inversionIndex;
                bestScore = score;
            }
        }

        return bestInversionIndex;
    }

    private static int GetInversionScore(ToneSet chord)
    {
        // Approach: distinguish the three triad inversions
        // third + fifth = looks like a root inversion (except if minor sharp five)
        // third + sixth = looks like a first inversion
        // fourth + sixth = looks like a second inversion
        var spelling = Recognize(chord);

        int score = 0;
        if (spelling.Fifths.HasAny() && (spelling.Third != MajorOrMinor.Minor || !spelling.Fifths.IsSharp())) ++score;
        else if (spelling.Thirteenths.HasAny()) --score;

        if (spelling.Third.HasValue) ++score;
        else if (spelling.Elevenths.HasAny()) --score;

        return score;
    }
}
