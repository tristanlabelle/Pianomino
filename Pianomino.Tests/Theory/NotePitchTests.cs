using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Theory;

public static class NotePitchTests
{
    [Fact]
    public static void TestParse()
    {
        Assert.Equal(NotePitch.C(Alteration.Sharp, octave: 8), NotePitch.Parse("C#8"));
    }

    [Fact]
    public static void TestParseToStringRoundTrip()
    {
        Assert.Equal("C#8", NotePitch.Parse("C#8").ToString());
    }

    [Fact]
    public static void TestAlteration()
    {
        Assert.Equal(Alteration.Flat, NotePitch.Parse("Bb1").Alteration);
        Assert.Equal(Alteration.Flat, NotePitch.Parse("Bb-1").Alteration);
    }

    [Fact]
    public static void TestAddIntervals()
    {
        Assert.Equal(NotePitch.C0, NotePitch.C0 + Interval.Unison);
        Assert.Equal(NotePitch.E0.Flat(), NotePitch.C0 + Interval.MinorThird);
        Assert.Equal(NotePitch.A(octave: -1), NotePitch.C0 - Interval.MinorThird);
        Assert.Equal(NotePitch.B(Alteration.DoubleFlat, octave: 0), NotePitch.C0 + Interval.DiminishedSeventh);
    }

    [Fact]
    public static void TestSimplifyEnharmonically()
    {
        Assert.Equal(NotePitch.F0, NotePitch.SimplifyEnharmonically(NotePitch.E0.Sharp()));
        Assert.Equal(NotePitch.E0, NotePitch.SimplifyEnharmonically(NotePitch.F0.Flat()));

        Assert.Equal(NotePitch.A0, NotePitch.SimplifyEnharmonically(NotePitch.B0.Flat().Flat()));

        Assert.Equal(NotePitch.F0.Sharp(), NotePitch.SimplifyEnharmonically(NotePitch.F0.Sharp()));
        Assert.Equal(NotePitch.F0.Sharp(), NotePitch.SimplifyEnharmonically(NotePitch.E0.Sharp().Sharp()));
        Assert.Equal(NotePitch.B0.Flat(), NotePitch.SimplifyEnharmonically(NotePitch.B0.Flat()));
        Assert.Equal(NotePitch.B0.Flat(), NotePitch.SimplifyEnharmonically(NotePitch.C(1).Flat().Flat()));
    }

    [Fact]
    public static void TestDefaultSpellingRelativeToTonic()
    {
        Assert.Equal(NotePitch.F0.Sharp(),
            NotePitch.GetDefaultSpellingRelativeToTonic(
                new ChromaticPitch(ChromaticClass.FsGb, octave: 0),
                tonic: NoteLetter.G));
        Assert.Equal(NotePitch.G0.Flat(),
            NotePitch.GetDefaultSpellingRelativeToTonic(
                new ChromaticPitch(ChromaticClass.FsGb, octave: 0),
                tonic: NoteLetter.E.Flat()));
    }
}
