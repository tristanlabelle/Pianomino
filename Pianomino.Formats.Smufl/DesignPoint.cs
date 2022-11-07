using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Smufl;

/// <summary>
/// Represents a point in font design space.
/// </summary>
public readonly struct DesignPoint : IEquatable<DesignPoint>
{
    public DesignDecimal X { get; }
    public DesignDecimal Y { get; }

    public DesignPoint(DesignDecimal x, DesignDecimal y) => (this.X, this.Y) = (x, y);

    public bool Equals(DesignPoint other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is DesignPoint other && Equals(other);
    public override int GetHashCode() => unchecked(X.GetHashCode() ^ (Y.GetHashCode() * 13));
    public static bool Equals(DesignPoint lhs, DesignPoint rhs) => lhs.Equals(rhs);
    public static bool operator ==(DesignPoint lhs, DesignPoint rhs) => Equals(lhs, rhs);
    public static bool operator !=(DesignPoint lhs, DesignPoint rhs) => !Equals(lhs, rhs);

    public Vector2 ToVector2() => new Vector2(X.ToSingle(), Y.ToSingle());

    public override string ToString() => $"({X}, {Y})";
}
