using System;
using System.Collections;
using System.Collections.Generic;

namespace Pianomino.Formats.iReal;

public sealed class Chart
{
    public SongMetadata SongMetadata { get; set; } = new();
    public IList<ChartRow> Rows { get; } = new List<ChartRow>();
}

public sealed class ChartRow
{
    public const int CellCount = CellArray.FixedLength;

    public CellArray Cells { get; } = new();

    public IEnumerable<CellRange> EnumerateMeasureCellRanges()
    {
        int? startIndex = null;
        bool emptyMeasure = true;
        for (int cellIndex = 0; cellIndex < CellCount; ++cellIndex)
        {
            var cell = Cells[cellIndex];
            if (cell.StartBarline is not null || (cellIndex == 0 && cell.Symbol is not null))
            {
                if (startIndex < cellIndex) yield return CellRange.FromStartEnd(startIndex.Value, cellIndex);
                startIndex = cellIndex;
                emptyMeasure = true;
            }

            if (cell.EndBarline is not null)
            {
                if (startIndex.HasValue) yield return CellRange.FromStartEnd(startIndex.Value, cellIndex + 1);
                startIndex = cellIndex + 1; // Assume a subsequent measure
                emptyMeasure = true;
            }

            if (cell.Symbol is not null) emptyMeasure = false;
        }

        if (startIndex < CellCount && !emptyMeasure)
            yield return CellRange.FromStartEnd(startIndex.Value, CellCount);
    }

    public sealed class CellArray : IReadOnlyList<Cell>
    {
        public const int FixedLength = 16;

        private readonly Cell[] cells;

        internal CellArray()
        {
            cells = new Cell[FixedLength];
            for (int i = 0; i < cells.Length; ++i)
                cells[i] = new Cell();
        }

        public int Length => FixedLength;

        public Cell this[int index]
        {
            get
            {
                if ((uint)index >= (uint)FixedLength) throw new ArgumentOutOfRangeException(nameof(index));
                return cells[index];
            }
        }

        private IEnumerator<Cell> GetIEnumerator() => ((IEnumerable<Cell>)cells).GetEnumerator();

        int IReadOnlyCollection<Cell>.Count => FixedLength;
        IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator() => GetIEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetIEnumerator();
    }
}
