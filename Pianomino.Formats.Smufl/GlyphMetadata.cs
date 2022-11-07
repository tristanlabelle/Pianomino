using System;
using System.Collections.Generic;
using System.Text;

namespace Pianomino.Formats.Smufl;

public sealed class GlyphMetadata
{
    public char CodePoint { get; }
    public AnchorCollection Anchors { get; } = new();
    public DesignBox? BoundingBox { get; set; }

    public GlyphMetadata(char codePoint)
    {
        if (!CodePoints.IsValid(codePoint)) throw new ArgumentOutOfRangeException(nameof(codePoint));
        this.CodePoint = codePoint;
    }

    public sealed class AnchorCollection : Dictionary<string, DesignPoint>
    {
        public DesignPoint this[KnownGlyphAnchor key]
        {
            get => this[key.GetKeyString()];
            set => this[key.GetKeyString()] = value;
        }

        public DesignPoint? GetValueOrNull(string key) => TryGetValue(key, out var value) ? value : null;
        public DesignPoint? GetValueOrNull(KnownGlyphAnchor key) => GetValueOrNull(key.GetKeyString());

        public bool TryGetValue(KnownGlyphAnchor key, out DesignPoint value)
            => TryGetValue(key.GetKeyString(), out value);

        public void Add(KnownGlyphAnchor key, DesignPoint value)
            => Add(key.GetKeyString(), value);

        public void Remove(KnownGlyphAnchor key) => Remove(key.GetKeyString());
    }
}
