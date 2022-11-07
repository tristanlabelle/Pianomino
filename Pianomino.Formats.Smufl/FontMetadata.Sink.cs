using System;
using System.Text;

namespace Pianomino.Formats.Smufl;

partial class FontMetadata
{
    private sealed class Sink : IFontMetadataSink
    {
        private readonly FontMetadata target;
        private readonly GlyphNameResolver glyphNameResolver;

        public Sink(FontMetadata target, GlyphNameResolver glyphNameResolver)
        {
            this.target = target;
            this.glyphNameResolver = glyphNameResolver;
        }

        private char? ResolveGlyphKey(GlyphKey key)
        {
            if (key.IsNull) return null;
            if (key.IsCodePoint) return key.GetCodePoint();
            return glyphNameResolver(key.GetName());
        }

        void IFontMetadataSink.SetName(string name) => target.Name = name;
        void IFontMetadataSink.SetVersion(Version version) => target.Version = version;
        void IFontMetadataSink.SetDesignSize(int size) => target.DesignSize = size;
        void IFontMetadataSink.SetSizeRange(Range range) => target.SizeRange = range;

        void IFontMetadataSink.AddEngravingDefault(string key, DesignDecimal value)
            => target.EngravingDefaults[key] = value;

        void IFontMetadataSink.AddGlyphAnchor(GlyphKey key, string anchorName, in DesignPoint point)
        {
            if (ResolveGlyphKey(key) is char codePoint)
                target.Glyphs.GetOrCreate(codePoint).Anchors[anchorName] = point;
        }

        void IFontMetadataSink.AddAlternateGlyph(GlyphKey key, int alternateCodePoint, string alternateName)
            => throw new NotImplementedException();

        void IFontMetadataSink.AddGlyphBoundingBox(GlyphKey key, in DesignBox box)
        {
            if (ResolveGlyphKey(key) is char codePoint)
                target.Glyphs.GetOrCreate(codePoint).BoundingBox = box;
        }
    }
}
