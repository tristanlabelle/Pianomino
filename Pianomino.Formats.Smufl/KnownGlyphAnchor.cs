using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Smufl;

public enum KnownGlyphAnchor : byte
{
    StemUpSE,
    StemDownNW,
    StemUpNW,
    StemDownSW,
    NominalWidth,
    NumeralTop,
    NumeralBottom,
    CutOutNE,
    CutOutSE,
    CutOutSW,
    CutOutNW,
}

public static class KnownGlyphAnchorEnum
{
    private static readonly ConcurrentDictionary<KnownGlyphAnchor, string> enumerantsToKeys = new();

    public static string GetKeyString(this KnownGlyphAnchor value)
    {
        if (enumerantsToKeys.TryGetValue(value, out var key)) return key;

        key = Enum.GetName(value) ?? throw new ArgumentException(nameof(value));
        key = char.ToLowerInvariant(key[0]) + key[1..];
        enumerantsToKeys.TryAdd(value, key);
        return key;
    }
}
