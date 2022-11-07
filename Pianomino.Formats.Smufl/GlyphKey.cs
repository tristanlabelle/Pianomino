using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Pianomino.Formats.Smufl;

/// <summary>
/// A lookup key for a smufl glyph, which can be either a code point or a name.
/// </summary>
public readonly struct GlyphKey : IEquatable<GlyphKey>
{
    public static readonly GlyphKey None = new();

    private readonly string? name;
    private readonly char codePoint;

    public GlyphKey(string name)
    {
        this.name = name;
        this.codePoint = default;
    }

    public GlyphKey(char codePoint)
    {
        if (!CodePoints.IsValid(codePoint)) throw new ArgumentOutOfRangeException(nameof(codePoint));
        this.name = null;
        this.codePoint = codePoint;
    }

    public bool IsNull => name == null && codePoint == default;
    public bool IsName => name != null;
    public bool IsCodePoint => codePoint != default;

    public string GetName() => name ?? throw new InvalidOperationException();
    public char GetCodePoint() => IsCodePoint ? codePoint : throw new InvalidOperationException();

    public bool Equals(GlyphKey other) => name == other.name && codePoint == other.codePoint;
    public override bool Equals(object? obj) => obj is GlyphKey other && Equals(other);
    public override int GetHashCode() => name?.GetHashCode() ?? codePoint.GetHashCode();
    public static bool Equals(GlyphKey lhs, GlyphKey rhs) => lhs.Equals(rhs);
    public static bool operator ==(GlyphKey lhs, GlyphKey rhs) => Equals(lhs, rhs);
    public static bool operator !=(GlyphKey lhs, GlyphKey rhs) => !Equals(lhs, rhs);

    public override string ToString()
    {
        if (IsCodePoint) return $"U+{(ushort)codePoint:X4}";
        return name ?? "<none>";
    }

    public static char? TryParseUPlusNotation(string str)
    {
        var match = Regex.Match(str, @"\AU+(?<hex>[0-9A-Fa-f]{4})\Z");
        if (!match.Success) return null;
        return (char)Convert.ToUInt16(match.Groups["hex"].Value, fromBase: 16);
    }

    public static GlyphKey Parse(string str)
    {
        var codePoint = TryParseUPlusNotation(str);
        if (codePoint.HasValue) return new(codePoint.Value);

        // May start with a digit, ex: "4stringTabClef"
        var match = Regex.Match(str, @"\A[a-zA-Z0-9_]*\Z", RegexOptions.CultureInvariant);
        if (match.Success) return new(str);

        throw new FormatException("Invalid glyph key format, should be either a glyph name or a unicode code point.");
    }
}
