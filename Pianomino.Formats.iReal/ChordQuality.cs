using Pianomino.Theory;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.iReal;

public readonly partial struct ChordQuality
{
    private const byte ExtendedBit = 0x80;
    private const byte BaseQualityMask = 0x7F;

    private readonly byte baseQualityAndExtendedBit;
    private readonly JazzChordExtension extensionOrDefault;
    private readonly ImmutableArray<ChordTweak> tweaks;

    public ChordQuality(ChordBaseQuality baseQuality, JazzChordExtension? extension, ImmutableArray<ChordTweak> tweaks)
    {
        if (extension is null)
        {
            if (baseQuality.ImpliesSeventh()) extension = JazzChordExtension.Seventh;
        }
        else
        {
            if (extension.Value.IsMajor && !baseQuality.AllowsMajorSeventh())
                throw new ArgumentException();
        }

        this.baseQualityAndExtendedBit = (byte)baseQuality;
        if (extension.HasValue) this.baseQualityAndExtendedBit |= ExtendedBit;
        this.extensionOrDefault = extension.GetValueOrDefault();
        this.tweaks = tweaks;
    }

    public ChordQuality(ChordBaseQuality baseQuality, JazzChordExtension? extension)
        : this(baseQuality, extension, ImmutableArray<ChordTweak>.Empty) { }

    public ChordQuality(ChordBaseQuality baseQuality)
        : this(baseQuality, null) { }

    public ChordBaseQuality BaseQuality => (ChordBaseQuality)(baseQualityAndExtendedBit & BaseQualityMask);
    public JazzChordExtension? Extension => (baseQualityAndExtendedBit & ExtendedBit) == ExtendedBit ? extensionOrDefault : null;
    public ImmutableArray<ChordTweak> Tweaks => tweaks.IsDefault ? ImmutableArray<ChordTweak>.Empty : tweaks;

    public static ChordQuality Parse(ReadOnlySpan<char> str) => PseudoUriParser.ParseChordQuality(str);

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        AppendString(stringBuilder);
        return stringBuilder.ToString();
    }

    public void AppendString(StringBuilder stringBuilder)
    {
        if (BaseQuality is not ChordBaseQuality.ImplicitMajor and not ChordBaseQuality.Suspended4 and not ChordBaseQuality.Altered)
        {
            stringBuilder.Append(BaseQuality switch
            {
                ChordBaseQuality.ExplicitMajor => "^",
                ChordBaseQuality.Minor => "-",
                ChordBaseQuality.Diminished => "o",
                ChordBaseQuality.HalfDiminished => "h",
                ChordBaseQuality.Augmented => "+",
                ChordBaseQuality.Power => "5",
                ChordBaseQuality.Suspended2 => "2",
                _ => throw new UnreachableException()
            });
        }

        if (Extension is JazzChordExtension extension)
        {
            if (extension.IsMajor) stringBuilder.Append('^');
            if (BaseQuality != ChordBaseQuality.HalfDiminished || extension.Degree > TertianChordDegree.Seventh)
                stringBuilder.Append(extension.Degree.ToNumber());
        }

        foreach (var tweak in Tweaks)
        {
            if (tweak.Type == ChordTweakType.Flat) stringBuilder.Append('b');
            else if (tweak.Type == ChordTweakType.Sharp) stringBuilder.Append('#');
            stringBuilder.Append(tweak.DegreeNumber);
        }
    }
}

public enum ChordBaseQuality : byte
{
    ImplicitMajor,
    ExplicitMajor,
    Minor,
    Augmented,
    Diminished,
    HalfDiminished,
    Suspended4, // Denoted 'sus'
    Suspended2, // Denoted '2'
    Power,
    Altered
}

public static class ChordBaseQualityEnum
{
    public static bool ImpliesMinorThird(this ChordBaseQuality value)
        => value is ChordBaseQuality.Minor or ChordBaseQuality.Diminished or ChordBaseQuality.HalfDiminished;

    public static bool ImpliesSeventh(this ChordBaseQuality value)
        => value is ChordBaseQuality.HalfDiminished or ChordBaseQuality.Altered;

    public static bool AllowsMajorSeventh(this ChordBaseQuality value)
        => value is not ChordBaseQuality.HalfDiminished;
}
