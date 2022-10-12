using Xunit;

namespace Pianomino.Theory;

public static class ToneSetTests
{
    [Fact]
    public static void TestDefaultValue()
    {
        Assert.Empty(default(ToneSet));
        Assert.Equal(0, default(ToneSet).Mask);
        Assert.True(!default(ToneSet).Contains(ChromaticDegree.P1));
    }

    [Fact]
    public static void TestRootOnly()
    {
        Assert.Single(ToneSet.RootOnly);
        Assert.Equal(1, ToneSet.RootOnly.Mask);
        Assert.True(ToneSet.RootOnly.Contains(ChromaticDegree.P1));
    }

    [Fact]
    public static void TestTones()
    {
        Assert.Equal(new[] { ChromaticDegree.P1, ChromaticDegree.M2, ChromaticDegree.M3,
                ChromaticDegree.P4, ChromaticDegree.P5, ChromaticDegree.M6, ChromaticDegree.M7 },
            ToneSet.Scales.Ionian);
    }

    [Fact]
    public static void TestRotateBy()
    {
        Assert.Equal(ToneSet.Scales.Ionian, ToneSet.Scales.Ionian.RotateBy(0));
        Assert.Equal(ToneSet.Scales.Locrian, ToneSet.Scales.Ionian.RotateBy(1));
        Assert.Equal(ToneSet.Scales.Dorian, ToneSet.Scales.Ionian.RotateBy(-2));
        Assert.Equal(ToneSet.Scales.Aeolian, ToneSet.Scales.Ionian.RotateBy(15));
    }

    [Fact]
    public static void TestGetInversion()
    {
        Assert.Equal(ToneSet.Scales.Ionian, ToneSet.Scales.Ionian.GetInversion(0));
        Assert.Equal(ToneSet.Scales.Dorian, ToneSet.Scales.Ionian.GetInversion(1));
        Assert.Equal(ToneSet.Scales.Phrygian, ToneSet.Scales.Ionian.GetInversion(2));
        Assert.Equal(ToneSet.Scales.Locrian, ToneSet.Scales.Ionian.GetInversion(-1));
        Assert.Equal(ToneSet.Scales.Ionian, ToneSet.Scales.Ionian.GetInversion(7));
    }
}
