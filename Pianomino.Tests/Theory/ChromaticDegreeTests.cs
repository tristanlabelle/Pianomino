using System;
using Xunit;

namespace Pianomino.Theory;

public static class ChromaticDegreeTests
{
    [Fact]
    public static void TestToNegativeHarmony()
    {
        Assert.Equal(ChromaticDegree.P5, ChromaticDegree.P1.ToNegativeHarmony());
        Assert.Equal(ChromaticDegree.P1, ChromaticDegree.P5.ToNegativeHarmony());

        Assert.Equal(ChromaticDegree.m3, ChromaticDegree.M3.ToNegativeHarmony());
        Assert.Equal(ChromaticDegree.M3, ChromaticDegree.m3.ToNegativeHarmony());
    }

    [Fact]
    public static void TestDiatonicFloorCeiling()
    {
        Assert.Equal(DiatonicDegree.Second, ChromaticDegree.M2.DiatonicStepFloor());
        Assert.Equal(DiatonicDegree.Second, ChromaticDegree.M2.DiatonicStepCeiling());

        Assert.Equal(DiatonicDegree.Fourth, ChromaticDegree.TT.DiatonicStepFloor());
        Assert.Equal(DiatonicDegree.Fifth, ChromaticDegree.TT.DiatonicStepCeiling());
    }
}
