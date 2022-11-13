using System;
using System.Collections.Generic;
using System.Linq;

namespace Pianomino.Formats.iReal;

public sealed class ChartBuilder : IChartSink
{
    private enum State : byte { Initial, Chart, Ended }
    private enum MeasureStartBarlineState : byte { Pending, Optional, Specified }

    public Chart Chart { get; } = new();
    private State state;
    private byte cellIndex;

    public ChartSinkFactory GetSinkFactory(bool throwIfMultiple)
    {
        IChartSink? CreateSink(SongMetadata songMetadata, out bool final)
        {
            final = !throwIfMultiple;
            if (state != State.Initial)
            {
                if (throwIfMultiple) throw new InvalidOperationException();
                return null;
            }

            Chart.SongMetadata = songMetadata;
            state = State.Chart;
            return this;
        }

        return CreateSink;
    }

    public void AddBarline(Barline barline)
    {
        if (barline is Barline.Single)
        {
            var cell = GetCurrentCell();
            if (cell.StartBarline is not null) throw new InvalidOperationException();

            if (TryGetPreviousCell() is Cell previousCell && previousCell.EndBarline is null)
                previousCell.EndBarline = Barline.Single;
            else
                cell.StartBarline = Barline.Single;
        }
        else if (barline is Barline.ClosingDouble or Barline.ClosingRepeat or Barline.Final)
        {
            var currentCell = GetCurrentCell();
            if (currentCell.StartBarline is not null) throw new InvalidOperationException();

            var cell = TryGetPreviousCell() ?? throw new InvalidOperationException();
            if (cell.EndBarline is not null) throw new InvalidOperationException();
            cell.EndBarline = barline;
        }
        else
        {
            var cell = GetCurrentCell();
            if (cell.StartBarline is Barline)
            {
                if (barline != Barline.Single) throw new InvalidOperationException();
                cell.EndBarline = Barline.Single;
            }
            else
            {
                cell.StartBarline = barline;
            }
        }
    }

    public void AddTimeSignature(TimeSignature value)
    {
        var cell = GetCurrentCell();
        if (cell.TimeSignature is not null) throw new InvalidOperationException();
        cell.TimeSignature = value;
    }

    public void AddRehearsalMark(RehearsalMark mark)
    {
        var cell = GetCurrentCell();
        if (cell.RehearsalMark is not null) throw new InvalidOperationException();
        cell.RehearsalMark = mark;
    }

    public void AddSegnoCodaOrFermata(SegnoCodaOrFermata value)
    {
        var cell = GetCurrentCell();
        if (cell.SegnoCodaOrFermata is not null) throw new InvalidOperationException();
        cell.SegnoCodaOrFermata = value;
    }

    public void AddEnding(Ending value)
    {
        var cell = GetCurrentCell();
        if (cell.Ending is not null) throw new InvalidOperationException();
        cell.Ending = value;
    }

    public void AddStaffText(StaffText staffText)
    {
        var cell = GetCurrentCell();
        if (cell.StaffText is not null) throw new InvalidOperationException();
        cell.StaffText = staffText;
    }

    public void AddShiftDown(VerticalSpace amount)
    {
        var cell = GetCurrentCell();
        if (cell.ShiftDown is not null) throw new InvalidOperationException();
        cell.ShiftDown = amount;
    }

    public void AddChordSizeChange(ChordSize size)
    {
        var cell = GetCurrentCell();
        if (cell.ChordSizeChange is not null) throw new InvalidOperationException();
        cell.ChordSizeChange = size;
    }

    public void AddCellSymbol(CellSymbol? symbol)
    {
        var cell = GetCurrentCell();
        if (cell.Symbol is not null) throw new InvalidOperationException();
        cell.Symbol = symbol;
        cellIndex++;
    }

    public void AddAlternateChordSymbol(ChordSymbol symbol)
    {
        var cell = TryGetPreviousCell() ?? throw new InvalidOperationException();
        if (cell.AlternateChordSymbol is not null) throw new InvalidOperationException();
        cell.AlternateChordSymbol = symbol;
    }

    public void AddEnd()
    {
        var cell = GetCurrentCell();
        if (cell.IsEnd) throw new InvalidOperationException();
        cell.IsEnd = true;
    }

    public void End()
    {
        if (state == State.Ended) throw new InvalidOperationException();
        state = State.Ended;
    }

    private void EnsureInChart()
    {
        if (state == State.Initial) state = State.Chart;
        if (state != State.Chart) throw new InvalidOperationException();
    }

    private Cell? TryGetPreviousCell()
    {
        EnsureInChart();

        if (Chart.Rows.Count == 0) return null;
        if (cellIndex > 0) return Chart.Rows.Last().Cells[cellIndex - 1];
        if (Chart.Rows.Count == 1) return null;
        return Chart.Rows[^2].Cells[^1];
    }

    private Cell GetCurrentCell()
    {
        EnsureInChart();

        var row = Chart.Rows.LastOrDefault();
        if (row is null || cellIndex == ChartRow.CellCount)
        {
            row = new ChartRow();
            Chart.Rows.Add(row);
            cellIndex = 0;
        }

        return row.Cells[cellIndex];
    }

    public static IEnumerable<Chart> ParseAll(string uri, out string? playlistName)
    {
        PseudoUriParser parser = new(uri);
        playlistName = parser.PlaylistName;
        return ParseToEnd(parser);
    }

    private static IEnumerable<Chart> ParseToEnd(PseudoUriParser parser)
    {
        while (parser.State != UriParserState.End)
        {
            ChartBuilder builder = new();
            builder.Chart.SongMetadata = parser.ParseSongMetadata();
            parser.ParseChartBody(builder);
            yield return builder.Chart;
        }
    }

    public static Chart ParseFirst(string uri)
    {
        ChartBuilder builder = new();
        PseudoUriParser.ParseToSink(uri, builder.GetSinkFactory(throwIfMultiple: false));
        if (builder.state != State.Ended) throw new InvalidOperationException();
        return builder.Chart;
    }

    public static Chart ParseBody(string body)
    {
        ChartBuilder builder = new();
        PseudoUriParser.ParseBody(body, builder);
        if (builder.state != State.Ended) throw new InvalidOperationException();
        return builder.Chart;
    }
}
