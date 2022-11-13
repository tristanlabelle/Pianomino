using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.iReal;

public readonly struct CellRange : IEquatable<CellRange>
{
    public int Start { get; }
    public int Length { get; }

    private CellRange(int start, int length)
    {
        if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        this.Start = start;
        this.Length = length;
    }

    public int End => Start + Length;

    public static CellRange FromStartLength(int start, int length) => new(start, length);
    public static CellRange FromStartEnd(int start, int end) => new(start, end - start);

    public static CellRange Shift(CellRange range, int offset)
    {
        if (range.Start + offset < 0 || range.End + offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));
        return new CellRange(range.Start + offset, range.Length);
    }

    public bool Equals(CellRange other) => Start == other.Start && Length == other.Length;
    public override bool Equals(object? obj) => obj is CellRange other && Equals(other);
    public static bool Equals(CellRange lhs, CellRange rhs) => lhs.Equals(rhs);
    public static bool operator ==(CellRange lhs, CellRange rhs) => Equals(lhs, rhs);
    public static bool operator !=(CellRange lhs, CellRange rhs) => !Equals(lhs, rhs);
    public override int GetHashCode() => Start ^ (Length * 13);
    public override string ToString() => $"Start={Start}, Length={Length}";

    public static implicit operator System.Range(CellRange range) => new(range.Start, range.End);
}
