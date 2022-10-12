using System;
using System.Collections.Immutable;

namespace Pianomino.Theory;

/// <summary>
/// Represents a pattern of increasing intervals starting with unison
/// and constrained to an octave. Can represent scale and chord types.
/// </summary>
public sealed class IntervalPattern
{
    public ImmutableArray<Interval> Intervals { get; }

    public IntervalPattern(ImmutableArray<Interval> intervals)
    {
        Validate(intervals.AsSpan());
        this.Intervals = intervals;
    }

    public IntervalPattern(params Interval[] intervals) : this(ImmutableArray.Create(intervals)) { }

    public int NoteCount => Intervals.Length;
    public bool IsHeptatonic => NoteCount == 7;

    public Interval GetDegree(int degree)
    {
        var (octave, index) = IntMath.EuclidianDivMod(degree, Intervals.Length);
        return Intervals[index] + Interval.Octave * octave;
    }

    public static void Validate(ReadOnlySpan<Interval> intervals)
    {
        if (intervals.IsEmpty || intervals[0] != Interval.Unison)
            throw new ArgumentException(message: null, paramName: nameof(intervals));

        for (int i = 1; i < intervals.Length; ++i)
            if (intervals[i].DiatonicDelta < intervals[i - 1].DiatonicDelta || intervals[i].ChromaticDelta <= intervals[i - 1].ChromaticDelta || intervals[i].DiatonicDelta >= 7 || intervals[i].ChromaticDelta >= 12)
                throw new ArgumentException(message: null, paramName: nameof(intervals));
    }

    public static class DiatonicModes
    {
        public static ImmutableArray<IntervalPattern> All { get; } = ImmutableArray.Create<IntervalPattern>(
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MajorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MinorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MinorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.AugmentedFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MajorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MinorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MinorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.DiminishedFifth, Interval.MinorSixth, Interval.MinorSeventh));

        public static IntervalPattern Ionian { get; } = All[0];
        public static IntervalPattern Dorian { get; } = All[1];
        public static IntervalPattern Phrygian { get; } = All[2];
        public static IntervalPattern Lydian { get; } = All[3];
        public static IntervalPattern Mixolydian { get; } = All[4];
        public static IntervalPattern Aeolian { get; } = All[5];
        public static IntervalPattern Locrian { get; } = All[6];
    }

    public static class MelodicMinorModes
    {
        public static ImmutableArray<IntervalPattern> All { get; } = ImmutableArray.Create<IntervalPattern>(
            new(Interval.Unison, Interval.MajorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MajorSeventh),
            new(Interval.Unison, Interval.MinorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.AugmentedFourth, Interval.AugmentedFifth, Interval.MajorSixth, Interval.MajorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.AugmentedFourth, Interval.PerfectFifth, Interval.MajorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MajorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MinorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MajorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.DiminishedFifth, Interval.MinorSixth, Interval.MinorSeventh),
            new(Interval.Unison, Interval.MinorSecond, Interval.MinorThird, Interval.DiminishedFourth, Interval.DiminishedFifth, Interval.MinorSixth, Interval.MinorSeventh));

        public static IntervalPattern MelodicMinor { get; } = All[0];
        public static IntervalPattern DorianFlat2 { get; } = All[1];
        public static IntervalPattern PhrygianSharp6 => DorianFlat2;
        public static IntervalPattern LydianAugmented { get; } = All[2];
        public static IntervalPattern LydianDominant { get; } = All[3];
        public static IntervalPattern MixolydianFlat6 { get; } = All[4];
        public static IntervalPattern AeolianFlat5 { get; } = All[5];
        public static IntervalPattern LocrianSharp2 => AeolianFlat5;
        public static IntervalPattern LocrianFlat4 { get; } = All[6];
        public static IntervalPattern SuperLocrian => LocrianFlat4;
    }

    public static class Scales
    {
        public static IntervalPattern Major => DiatonicModes.Ionian;
        public static IntervalPattern Minor => DiatonicModes.Aeolian;
        public static IntervalPattern HarmonicMinor { get; } = new(
            Interval.Unison, Interval.MajorSecond, Interval.MinorThird, Interval.PerfectFourth, Interval.PerfectFifth, Interval.MinorSixth, Interval.MajorSeventh);
        public static IntervalPattern MelodicMinor => MelodicMinorModes.MelodicMinor;
    }

    public static class Chords
    {
        public static IntervalPattern Major { get; } = new(
            Interval.Unison, Interval.MajorThird, Interval.PerfectFifth);
        public static IntervalPattern Minor { get; } = new(
            Interval.Unison, Interval.MinorThird, Interval.PerfectFifth);
        public static IntervalPattern Diminished { get; } = new(
            Interval.Unison, Interval.MinorThird, Interval.DiminishedFifth);
        public static IntervalPattern Augmented { get; } = new(
            Interval.Unison, Interval.MajorThird, Interval.AugmentedFifth);

        public static IntervalPattern Suspended2 { get; } = new(
            Interval.Unison, Interval.MajorSecond, Interval.PerfectFifth);
        public static IntervalPattern Suspended4 { get; } = new(
            Interval.Unison, Interval.PerfectFourth, Interval.PerfectFifth);
    }
}
