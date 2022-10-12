using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Pianomino;

public static class BitsTests
{
    [Fact]
    public static void TestIsSingle()
    {
        Assert.True(Bits.IsSingle(0b1000));
        Assert.True(Bits.IsSingle(0x80000000));
        Assert.True(Bits.IsSingle(1));
        Assert.False(Bits.IsSingle(0));
        Assert.False(Bits.IsSingle(0b1100100));
    }

    [Fact]
    public static void TestCountTrailingZeroes()
    {
        Assert.Equal(32, Bits.CountTrailingZeroes(0));
        Assert.Equal(0, Bits.CountTrailingZeroes(1));
        Assert.Equal(2, Bits.CountTrailingZeroes(0b100));
        Assert.Equal(31, Bits.CountTrailingZeroes(0x80000000));
        Assert.Equal(0, Bits.CountTrailingZeroes(0xFFFFFFFF));
        Assert.Equal(2, Bits.CountTrailingZeroes(0b100100));
    }

    [Fact]
    public static void TestIndexOfMostSignificant()
    {
        Assert.Equal(0, Bits.IndexOfMostSignificant(1));
        Assert.Equal(2, Bits.IndexOfMostSignificant(0b100));
        Assert.Equal(31, Bits.IndexOfMostSignificant(0x80000000));
        Assert.Equal(3, Bits.IndexOfMostSignificant(0b1001));
    }
}
