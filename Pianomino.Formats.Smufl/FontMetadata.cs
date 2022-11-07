using System;
using System.Collections.Generic;
using System.Text;

namespace Pianomino.Formats.Smufl;

public sealed partial class FontMetadata
{
    // From SMuFL spec: Dividing the em in four provides an analogue for a five-line staff.
    // All glyphs should be drawn at a scale consistent with the key measurement that one staff space = 0.25 em.
    public const float StaffSpacesPerEm = 4;

    public string? Name { get; set; }
    public Version? Version { get; set; }
    public int? DesignSize { get; set; }
    public Range? SizeRange { get; set; }
    public EngravingDefaultCollection EngravingDefaults { get; } = new();
    public GlyphCollection Glyphs { get; } = new();

    public IFontMetadataSink CreateSink(GlyphNameResolver glyphNameResolver) => new Sink(this, glyphNameResolver);

    public sealed class EngravingDefaultCollection : Dictionary<string, DesignDecimal>
    {
        public DesignDecimal this[KnownEngravingDefault key]
        {
            get => this[key.GetKeyString()];
            set => this[key.GetKeyString()] = value;
        }

        public DesignDecimal? GetValueOrNull(string key) => TryGetValue(key, out var value) ? value : null;
        public DesignDecimal? GetValueOrNull(KnownEngravingDefault key) => GetValueOrNull(key.GetKeyString());

        public bool TryGetValue(KnownEngravingDefault key, out DesignDecimal value)
            => TryGetValue(key.GetKeyString(), out value);

        public void Add(KnownEngravingDefault key, DesignDecimal value)
            => Add(key.GetKeyString(), value);

        public void Remove(KnownEngravingDefault key) => Remove(key.GetKeyString());
    }

    public sealed class GlyphCollection : Dictionary<char, GlyphMetadata>
    {
        public GlyphMetadata GetOrCreate(char codePoint)
        {
            if (!TryGetValue(codePoint, out var glyph))
            {
                glyph = new GlyphMetadata(codePoint);
                Add(codePoint, glyph);
            }

            return glyph;
        }
    }
}
