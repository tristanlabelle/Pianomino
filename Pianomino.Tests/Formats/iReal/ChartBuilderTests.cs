using Pianomino.Theory;
using System;
using Xunit;

namespace Pianomino.Formats.iReal;

public static class ChartBuilderTests
{
    [Fact]
    public static void TestSingleMeasure()
    {
        var cell = Assert.Single(ChartBuilder.ParseBody("|A|").Rows).Cells[0];
        Assert.Equal(Barline.Single, cell.StartBarline);
        Assert.IsType<ChordSymbol>(cell.Symbol);
        Assert.Equal(Barline.Single, cell.EndBarline);
    }

    [Fact]
    public static void TestTwoMeasures()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|A|B|").Rows);
        Assert.Equal(Barline.Single, row.Cells[0].StartBarline);
        Assert.IsType<ChordSymbol>(row.Cells[0].Symbol);
        Assert.Equal(Barline.Single, row.Cells[0].EndBarline);
        Assert.Null(row.Cells[1].StartBarline);
        Assert.IsType<ChordSymbol>(row.Cells[1].Symbol);
        Assert.Equal(Barline.Single, row.Cells[1].EndBarline);
    }

    [Fact]
    public static void TestMultipleRows()
    {
        var chart = ChartBuilder.ParseBody(new string(' ', ChartRow.CellCount) + "A");
        Assert.Equal(2, chart.Rows.Count);
        Assert.IsType<ChordSymbol>(chart.Rows[1].Cells[0].Symbol);
    }

    [Fact]
    public static void TestBarlines()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("{A|B}[C]D||E[FZ").Rows);
        Assert.Equal(Barline.OpeningRepeat, row.Cells[0].StartBarline);
        Assert.Equal(Barline.Single, row.Cells[0].EndBarline);
        Assert.Null(row.Cells[1].StartBarline);
        Assert.Equal(Barline.ClosingRepeat, row.Cells[1].EndBarline);
        Assert.Equal(Barline.OpeningDouble, row.Cells[2].StartBarline);
        Assert.Equal(Barline.ClosingDouble, row.Cells[2].EndBarline);
        Assert.Null(row.Cells[3].StartBarline);
        Assert.Equal(Barline.Single, row.Cells[3].EndBarline);
        Assert.Equal(Barline.Single, row.Cells[4].StartBarline);
        Assert.Null(row.Cells[4].EndBarline);
        Assert.Equal(Barline.OpeningDouble, row.Cells[5].StartBarline);
        Assert.Equal(Barline.Final, row.Cells[5].EndBarline);
    }

    [Fact]
    public static void TestRehearsalMarks()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|*A |*v |").Rows);
        Assert.Equal(RehearsalMark.SectionA, row.Cells[0].RehearsalMark);
        Assert.Equal(RehearsalMark.Verse, row.Cells[1].RehearsalMark);
    }

    [Fact]
    public static void TestSegnoCodaFermata()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|S |Q |f |").Rows);
        Assert.Equal(SegnoCodaOrFermata.Segno, row.Cells[0].SegnoCodaOrFermata);
        Assert.Equal(SegnoCodaOrFermata.Coda, row.Cells[1].SegnoCodaOrFermata);
        Assert.Equal(SegnoCodaOrFermata.Fermata, row.Cells[2].SegnoCodaOrFermata);
    }

    [Fact]
    public static void TestTimeSignatures()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|T22 |T34 |T78 |T12 Z").Rows);
        Assert.Equal(TimeSignature.TwoTwo, row.Cells[0].TimeSignature);
        Assert.Equal(TimeSignature.ThreeFour, row.Cells[1].TimeSignature);
        Assert.Equal(TimeSignature.SevenEight, row.Cells[2].TimeSignature);
        Assert.Equal(TimeSignature.TwelveEight, row.Cells[3].TimeSignature);
    }

    [Fact]
    public static void TestEndings()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|N1 |N2 |N0 Z").Rows);
        Assert.Equal(Ending.One, row.Cells[0].Ending);
        Assert.Equal(Ending.Two, row.Cells[1].Ending);
        Assert.Equal(Ending.Final, row.Cells[2].Ending);
    }

    [Fact]
    public static void TestMeasureRepeats()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|x|rZ").Rows);
        Assert.Equal(CellSymbol.SingleMeasureRepeat, row.Cells[0].Symbol);
        Assert.Equal(CellSymbol.DoubleMeasureRepeat, row.Cells[1].Symbol);
    }

    [Fact]
    public static void TestNoBlankCells()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|Cp,n,Z").Rows);
        Assert.IsType<ChordSymbol>(row.Cells[0].Symbol);
        Assert.Equal(CellSymbol.ChordRepeatSlash, row.Cells[1].Symbol);
        Assert.Equal(CellSymbol.NoChord, row.Cells[2].Symbol);
        Assert.Equal(Barline.Final, row.Cells[2].EndBarline);
    }

    [Fact]
    public static void TestStaffText()
    {
        var row = Assert.Single(ChartBuilder.ParseBody("|<foo> <*42bar> |").Rows);
        Assert.Equal("foo", row.Cells[0].StaffText?.Text);
        Assert.Null(row.Cells[0].StaffText?.Height);
        Assert.Equal("bar", row.Cells[1].StaffText?.Text);
        Assert.Equal((byte)42, row.Cells[1].StaffText?.Height);
    }
}
