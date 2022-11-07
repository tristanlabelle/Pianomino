using System;

namespace Pianomino.Formats.Smufl;

public interface IFontMetadataSink
{
    void SetName(string name);
    void SetVersion(Version version);
    void SetDesignSize(int size);
    void SetSizeRange(Range range);

    void AddEngravingDefault(string name, DesignDecimal value);
    void AddGlyphAnchor(GlyphKey key, string anchorName, in DesignPoint point);
    void AddAlternateGlyph(GlyphKey key, int alternateCodePoint, string alternateName);
    void AddGlyphBoundingBox(GlyphKey key, in DesignBox box);
}
