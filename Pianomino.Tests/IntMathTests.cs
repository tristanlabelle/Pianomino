using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino;

public static class IntMathTests
{
    [Fact]
    public static void TestEuclidianDivMod_PositiveDividendPositiveDivisor()
    {
        Assert.Equal((0, 0), IntMath.EuclidianDivMod(0, 3));
        Assert.Equal((0, 1), IntMath.EuclidianDivMod(1, 3));
        Assert.Equal((0, 2), IntMath.EuclidianDivMod(2, 3));
        Assert.Equal((1, 0), IntMath.EuclidianDivMod(3, 3));
        Assert.Equal((1, 1), IntMath.EuclidianDivMod(4, 3));
    }

    [Fact]
    public static void TestEuclidianDivMod_NegativeDividendPositiveDivisor()
    {
        Assert.Equal((-2, 2), IntMath.EuclidianDivMod(-4, 3));
        Assert.Equal((-1, 0), IntMath.EuclidianDivMod(-3, 3));
        Assert.Equal((-1, 1), IntMath.EuclidianDivMod(-2, 3));
        Assert.Equal((-1, 2), IntMath.EuclidianDivMod(-1, 3));
    }

    [Fact]
    public static void TestEuclidianDivMod_BivisionByOne()
    {
        Assert.Equal((-42, 0), IntMath.EuclidianDivMod(-42, 1));
        Assert.Equal((0, 0), IntMath.EuclidianDivMod(0, 1));
        Assert.Equal((42, 0), IntMath.EuclidianDivMod(42, 1));
    }

    [Fact]
    public static void TestEuclidianDivMod_BivisionByZero()
    {
        Assert.Throws<DivideByZeroException>(() => IntMath.EuclidianDivMod(-42, 0));
        Assert.Throws<DivideByZeroException>(() => IntMath.EuclidianDivMod(0, 0));
        Assert.Throws<DivideByZeroException>(() => IntMath.EuclidianDivMod(42, 0));
    }
}
