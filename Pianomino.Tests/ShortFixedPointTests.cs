using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Pianomino;

public static class ShortFixedPointTests
{
    [Fact]
    public static void TestExponentialForm()
    {
        Assert.Equal((1, 0), ShortFixedPoint.One.ToExponentialForm());
        Assert.Equal((1, 1), ((ShortFixedPoint)2).ToExponentialForm());
        Assert.Equal((3, 0), ((ShortFixedPoint)3).ToExponentialForm());
        Assert.Equal((1, -1), (ShortFixedPoint.One / 2).ToExponentialForm());
        Assert.Equal((1, -ShortFixedPoint.RawUnitShift), ShortFixedPoint.Epsilon.ToExponentialForm());

        Assert.Equal((-1, 0), ((ShortFixedPoint)(-1)).ToExponentialForm());
        Assert.Equal((-1, 1), ((ShortFixedPoint)(-2)).ToExponentialForm());
        Assert.Equal((-1, -ShortFixedPoint.RawUnitShift), (-ShortFixedPoint.Epsilon).ToExponentialForm());
    }
}
