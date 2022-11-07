using System;
using System.Collections.Concurrent;

namespace Pianomino.Formats.Smufl;

public enum KnownEngravingDefault : byte
{
    StaffLineThickness,
    StemThickness,
    BeamThickness,
    BeamSpacing,
    LegerLineThickness,
    LegerLineExtension,
    SlurEndpointThickness,
    SlurMidpointThickness,
    TieEndpointThickness,
    TieMidpointThickness,
    ThinBarlineThickness,
    ThickBarlineThickness,
    DashedBarlineThickness,
    DashedBarlineDashLength,
    DashedBarlineGapLength,
    BarlineSeparation,
    RepeatBarlineDotSeparation,
    BracketThickness,
    SubBracketThickness,
    HairpinThickness,
    OctaveLineThickness,
    PedalLineThickness,
    RepeatEndingLineThickness,
    ArrowShaftThickness,
    LyricLineThickness,
    TextEnclosureThickness,
    TupletBracketThickness
}

public static class KnownEngravingDefaultEnum
{
    private static readonly ConcurrentDictionary<KnownEngravingDefault, string> enumerantsToKeys = new();

    public static string GetKeyString(this KnownEngravingDefault value)
    {
        if (enumerantsToKeys.TryGetValue(value, out var key)) return key;

        key = Enum.GetName(value) ?? throw new ArgumentException(nameof(value));
        key = char.ToLowerInvariant(key[0]) + key[1..];
        enumerantsToKeys.TryAdd(value, key);
        return key;
    }
}
