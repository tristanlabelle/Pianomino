using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Theory;

public static class DiatonicKeyTests
{
    [Fact]
    public static void TestFromMode()
    {
        Assert.Equal(DiatonicKey.CMajor, DiatonicKey.FromMode(NoteLetter.C, DiatonicMode.Ionian));
        Assert.Equal(DiatonicKey.AMinor, DiatonicKey.FromMode(NoteLetter.A, DiatonicMode.Aeolian));
        Assert.Equal(DiatonicKey.Major(NoteLetter.B.Flat()), DiatonicKey.FromMode(NoteLetter.F, DiatonicMode.Mixolydian));
        Assert.Equal(DiatonicKey.Minor(NoteLetter.F.Sharp()), DiatonicKey.FromMode(NoteLetter.B, DiatonicMode.Dorian));
    }

    [Fact]
    public static void TestSharpsInSignature_Major()
    {
        Assert.Equal(0, DiatonicKey.Major(NoteLetter.C).SharpsInSignature);
        Assert.Equal(2, DiatonicKey.Major(NoteLetter.D).SharpsInSignature);
        Assert.Equal(-1, DiatonicKey.Major(NoteLetter.F).SharpsInSignature);

        Assert.Equal(-2, DiatonicKey.Major(NoteLetter.B.Flat()).SharpsInSignature);
        Assert.Equal(6, DiatonicKey.Major(NoteLetter.F.Sharp()).SharpsInSignature);
    }

    [Fact]
    public static void TestSharpsInSignature_Minor()
    {
        Assert.Equal(0, DiatonicKey.Minor(NoteLetter.A).SharpsInSignature);
        Assert.Equal(-3, DiatonicKey.Minor(NoteLetter.C).SharpsInSignature);
        Assert.Equal(3, DiatonicKey.Minor(NoteLetter.F.Sharp()).SharpsInSignature);
        Assert.Equal(-5, DiatonicKey.Minor(NoteLetter.B.Flat()).SharpsInSignature);
    }
    [Fact]
    public static void TestFromSharpsInSignature()
    {
        Assert.Equal(NoteLetter.C, DiatonicKey.FromSharpsInSignature(0, MajorOrMinor.Major).Tonic);
        Assert.Equal(NoteLetter.E, DiatonicKey.FromSharpsInSignature(4, MajorOrMinor.Major).Tonic);
        Assert.Equal(NoteLetter.E.Flat(), DiatonicKey.FromSharpsInSignature(-3, MajorOrMinor.Major).Tonic);

        Assert.Equal(NoteLetter.A, DiatonicKey.FromSharpsInSignature(0, MajorOrMinor.Minor).Tonic);
        Assert.Equal(NoteLetter.C.Sharp(), DiatonicKey.FromSharpsInSignature(4, MajorOrMinor.Minor).Tonic);
        Assert.Equal(NoteLetter.C, DiatonicKey.FromSharpsInSignature(-3, MajorOrMinor.Minor).Tonic);
    }

    [Fact]
    public static void TestGetNoteAlteration_CMajor()
    {
        var key = DiatonicKey.CMajor;
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.C));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.D));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.E));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.F));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.G));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.A));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.B));
    }

    [Fact]
    public static void TestGetNoteAlteration()
    {
        var key = DiatonicKey.Major(NoteLetter.B.Flat());
        Assert.Equal(Alteration.Flat, key.GetNoteAlteration(NoteLetter.B));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.C));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.D));
        Assert.Equal(Alteration.Flat, key.GetNoteAlteration(NoteLetter.E));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.F));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.G));
        Assert.Equal(Alteration.Natural, key.GetNoteAlteration(NoteLetter.A));
    }
}
