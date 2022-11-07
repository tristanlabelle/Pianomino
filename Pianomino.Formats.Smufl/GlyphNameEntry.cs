using System;
using System.Text;

namespace Pianomino.Formats.Smufl;

/// <summary>
/// Represents a named entry in the glyphnames.json metadata file.
/// </summary>
public readonly struct GlyphNameEntry
{
    public char CodePoint { get; }
    public string? Description { get; }

    public GlyphNameEntry(char codePoint, string? description)
    {
        if (!CodePoints.IsValid(codePoint)) throw new ArgumentOutOfRangeException(nameof(codePoint));

        this.CodePoint = codePoint;
        this.Description = description;
    }

    public override string ToString() => CodePoint == default ? "<none>" : CodePoint.ToString();
}
