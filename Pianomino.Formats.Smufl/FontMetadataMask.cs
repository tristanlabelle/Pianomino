using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Smufl;

[Flags]
public enum FontMetadataMask
{
    None = 0,

    Name = 1 << 0,
    Version = 1 << 1,
    EngravingDefaults = 1 << 2,
    GlyphAnchors = 1 << 3,
    AlternateGlyphs = 1 << 4,
    GlyphBoundingBoxes = 1 << 5,
    Ligatures = 1 << 6,
    Sets = 1 << 7,

    Header = Name | Version,

    All = -1
}
