using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Theory;

public static class IntervalTests
{
    [Fact]
    public static void TestDiatonicToChromatic()
    {
        Assert.Equal(4, Interval.DiatonicToChromatic(2));
        Assert.Equal(-3, Interval.DiatonicToChromatic(-2));
    }

    [Fact]
    public static void TestShortNames_Ascending()
    {
        Assert.Equal("P1", Interval.Unison.ShortName);

        Assert.Equal("d3", Interval.FromDiatonicDelta(2, Alteration.DoubleFlat).ShortName);
        Assert.Equal("m3", Interval.MinorThird.ShortName);
        Assert.Equal("M3", Interval.MajorThird.ShortName);
        Assert.Equal("A3", Interval.FromDiatonicDelta(2, Alteration.Sharp).ShortName);

        Assert.Equal("d5", Interval.DiminishedFifth.ShortName);
        Assert.Equal("P5", Interval.PerfectFifth.ShortName);
        Assert.Equal("A5", Interval.AugmentedFifth.ShortName);
    }

    [Fact]
    public static void TestShortNames_Descending()
    {
        Assert.Equal("-P5", (-Interval.PerfectFifth).ShortName);
        Assert.Equal("-M2", (-Interval.MajorSecond).ShortName);
        Assert.Equal("-A4", (-Interval.AugmentedFourth).ShortName);
    }

    [Fact]
    public static void TestModOctave()
    {
        Assert.Equal(Interval.MajorSecond, Interval.ModOctave(Interval.MajorSecond + Interval.Octave));
        Assert.Equal(Interval.MajorSecond, Interval.ModOctave(Interval.MajorSecond - Interval.Octave));

        Assert.Equal(Interval.MajorSixth, Interval.ModOctave(-Interval.MinorThird));
    }
}
