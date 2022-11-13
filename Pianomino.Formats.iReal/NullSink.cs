using Pianomino.Theory;
using System;

namespace Pianomino.Formats.iReal;

public sealed class NullSink : IChartSink
{
    public static NullSink Instance { get; } = new();
    public static ChartSinkFactory Factory { get; } = delegate (SongMetadata header, out bool final)
    {
        final = false;
        return Instance;
    };

    void IChartSink.AddBarline(Barline barline) { }
    void IChartSink.AddTimeSignature(TimeSignature value) { }
    void IChartSink.AddRehearsalMark(RehearsalMark mark) { }
    void IChartSink.AddSegnoCodaOrFermata(SegnoCodaOrFermata mark) { }
    void IChartSink.AddEnding(Ending ending) { }
    void IChartSink.AddStaffText(StaffText staffText) { }
    void IChartSink.AddShiftDown(VerticalSpace amount) { }
    void IChartSink.AddChordSizeChange(ChordSize size) { }
    void IChartSink.AddCellSymbol(CellSymbol? symbol) { }
    void IChartSink.AddAlternateChordSymbol(ChordSymbol symbol) { }
    void IChartSink.AddEnd() { }
    void IChartSink.End() { }
}
