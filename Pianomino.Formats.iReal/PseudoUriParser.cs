using Pianomino.Theory;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace Pianomino.Formats.iReal;

public sealed class PseudoUriParser
{
    private ReadOnlyMemory<char> str;
    public bool IsProFormat { get; }
    public string? PlaylistName { get; }
    private bool expectChartBody;

    public PseudoUriParser(ReadOnlyMemory<char> str)
    {
        this.IsProFormat = ConsumeUriPrefix(ref str);
        if (str.Span.Contains('%')) str = Uri.UnescapeDataString(new string(str.Span)).AsMemory();
        if (IsProFormat) PlaylistName = TryConsumePlaylistNameSuffix(ref str);
        this.str = str;
    }

    public PseudoUriParser(string str) : this(str.AsMemory()) { }

    public UriParserState State => expectChartBody ? UriParserState.BeforeChartBody
        : (str.Length == 0 ? UriParserState.End : UriParserState.BeforeSongMetadata);

    public void ParseChart(ChartSinkFactory sinkFactory, out bool final)
    {
        if (State != UriParserState.BeforeSongMetadata) throw new InvalidOperationException();

        var songMetadata = ParseSongMetadata();
        if (sinkFactory(songMetadata, out final) is IChartSink sink)
            ParseChartBody(sink);
        else
            SkipChartBody();
    }

    public SongMetadata ParseSongMetadata()
    {
        if (State != UriParserState.BeforeSongMetadata) throw new InvalidOperationException();

        var span = str.Span;
        var songMetadata = ConsumeMetadataPrefix(ref span, IsProFormat);
        str = str[^span.Length..];
        expectChartBody = true;
        return songMetadata;
    }

    public static SongMetadata ParseFirstSongMetadata(ReadOnlySpan<char> str)
    {
        bool proFormat = ConsumeUriPrefix(ref str);
        if (str.Contains('%')) str = Uri.UnescapeDataString(new string(str));
        return ConsumeMetadataPrefix(ref str, proFormat);
    }

    public void ParseChartBody(IChartSink sink) => ParseChartBodyImpl(sink);
    public void SkipChartBody() => ParseChartBodyImpl(null);
    public string ExtractChartBodyString() => ParseChartBodyImpl(null, returnString: true)!;

    private string? ParseChartBodyImpl(IChartSink? sink, bool returnString = false)
    {
        if (State != UriParserState.BeforeChartBody) throw new InvalidOperationException();

        ReadOnlySpan<char> bodySpan;
        {
            var span = str.Span;
            bodySpan = ConsumeBodyAsSpan(ref span, IsProFormat);
            str = str[^span.Length..];
        }

        // Deobfuscate the body if we need to (we're either returning or parsing it)
        string? bodyString = null;
        if (returnString || (sink is not null && IsProFormat))
        {
            if (IsProFormat)
            {
                bodyString = DeobfuscateBody(bodySpan);
                bodySpan = bodyString;
            }
            else bodyString = new string(bodySpan);
        }

        if (sink is not null) ParseBody(bodySpan, sink);

        if (IsProFormat)
        {
            // Skip suffix fields
            var span = str.Span;
            bool skippedFields = SpanParsing.TrySkipUntil(ref span, '=', skipTerminator: true) // Actual style
                && SpanParsing.TrySkipUntil(ref span, '=', skipTerminator: true) // Tempo
                && SpanParsing.TrySkipUntil(ref span, '=', skipTerminator: true) // Repeats
                && SpanParsing.TryConsume(ref span, "=="); // Song separator
            if (!skippedFields) throw new FormatException();

            str = str[^span.Length..];
        }

        expectChartBody = false;
        return bodyString;
    }

    public static void ParseToSink(string str, ChartSinkFactory sinkFactory, out string? playlistName)
    {
        PseudoUriParser parser = new(str);
        playlistName = parser.PlaylistName;
        while (parser.State != UriParserState.End)
        {
            parser.ParseChart(sinkFactory, out bool final);
            if (final) break;
        }
    }

    public static void ParseToSink(string str, ChartSinkFactory sinkFactory) => ParseToSink(str, sinkFactory, out _);

    public static void ParseBody(ReadOnlySpan<char> str, IChartSink sink)
    {
        while (str.Length > 0)
        {
            if (SpanParsing.TryConsume(ref str, ' '))
                sink.AddCellSymbol(null);
            else if (TryConsumeCellSymbol(ref str) is CellSymbol cellSymbol)
                sink.AddCellSymbol(cellSymbol);
            else if (BarlineEnum.TryFromChar(str[0]) is Barline barline)
            {
                SpanParsing.ConsumeChar(ref str);
                sink.AddBarline(barline);
            }
            else if (SpanParsing.TryConsume(ref str, ',')) { }
            else if (SpanParsing.TryConsume(ref str, '('))
            {
                var alternateChordSymbol = TryConsumeChordSymbol(ref str, allowBassChange: true) ?? throw new FormatException();
                if (!SpanParsing.TryConsume(ref str, ')')) throw new FormatException();

                sink.AddAlternateChordSymbol(alternateChordSymbol);
            }
            else if (SpanParsing.TryConsume(ref str, 'T'))
            {
                var firstDigit = SpanParsing.TryConsumeChar(ref str) ?? throw new FormatException();
                var secondDigit = SpanParsing.TryConsumeChar(ref str) ?? throw new FormatException();
                var timeSignature = TimeSignatureEnum.TryFromEncodedDigits(firstDigit, secondDigit) ?? throw new FormatException();
                sink.AddTimeSignature(timeSignature);
            }
            else if (SpanParsing.TryConsume(ref str, '<'))
            {
                byte? height = null;
                if (SpanParsing.TryConsume(ref str, '*'))
                {
                    var firstDigit = SpanParsing.TryConsumeDigit(ref str) ?? throw new FormatException();
                    var secondDigit = SpanParsing.TryConsumeDigit(ref str) ?? throw new FormatException();
                    height = (byte)(firstDigit * 10 + secondDigit);
                    if (height > StaffText.MaxHeight) throw new FormatException();
                }

                var text = SpanParsing.TryConsumeUntil(ref str, '>', skipTerminator: true)
                    ?? throw new FormatException();
                sink.AddStaffText(new StaffText(text, height));
            }
            else if (SpanParsing.SkipZeroOrMore(ref str, 'Y') is int yCount and > 0)
            {
                if (yCount > 3) throw new FormatException();
                sink.AddShiftDown((VerticalSpace)yCount);
            }
            else if (SpanParsing.TryConsume(ref str, '*'))
            {
                var rehearsalMark = (SpanParsing.TryConsumeChar(ref str) ?? throw new FormatException()) switch
                {
                    'A' => RehearsalMark.SectionA,
                    'B' => RehearsalMark.SectionB,
                    'C' => RehearsalMark.SectionC,
                    'D' => RehearsalMark.SectionD,
                    'v' => RehearsalMark.Verse,
                    'i' => RehearsalMark.Intro,
                    _ => throw new FormatException()
                };
                sink.AddRehearsalMark(rehearsalMark);
            }
            else if (SpanParsing.TryConsume(ref str, 'S'))
                sink.AddSegnoCodaOrFermata(SegnoCodaOrFermata.Segno);
            else if (SpanParsing.TryConsume(ref str, 'Q'))
                sink.AddSegnoCodaOrFermata(SegnoCodaOrFermata.Coda);
            else if (SpanParsing.TryConsume(ref str, 'f'))
                sink.AddSegnoCodaOrFermata(SegnoCodaOrFermata.Fermata);
            else if (SpanParsing.TryConsume(ref str, 'N'))
            {
                sink.AddEnding(SpanParsing.TryConsumeDigit(ref str) switch
                {
                    0 => Ending.Final,
                    1 => Ending.One,
                    2 => Ending.Two,
                    3 => Ending.Three,
                    _ => throw new FormatException()
                });
            }
            else if (SpanParsing.TryConsume(ref str, 's'))
                sink.AddChordSizeChange(ChordSize.Small);
            else if (SpanParsing.TryConsume(ref str, 'l'))
                sink.AddChordSizeChange(ChordSize.Large);
            else if (SpanParsing.TryConsume(ref str, 'n'))
                sink.AddCellSymbol(CellSymbol.NoChord);
            else if (SpanParsing.TryConsume(ref str, 'p'))
                sink.AddCellSymbol(CellSymbol.ChordRepeatSlash);
            else if (SpanParsing.TryConsume(ref str, 'x'))
                sink.AddCellSymbol(CellSymbol.SingleMeasureRepeat);
            else if (SpanParsing.TryConsume(ref str, 'r'))
                sink.AddCellSymbol(CellSymbol.DoubleMeasureRepeat);
            else if (SpanParsing.TryConsume(ref str, 'U'))
                sink.AddEnd();
            else if (SpanParsing.SkipZeroOrMore(ref str, '\n') > 0)
            {
                // Allow at end
                if (str.Length > 0) throw new FormatException();
                break;
            }
            else
            {
                throw new FormatException();
            }
        }

        sink.End();
    }

    public static ChordSymbol ParseChordSymbol(ReadOnlySpan<char> str, bool allowBassChange = false)
    {
        var chord = TryConsumeChordSymbol(ref str, allowBassChange) ?? throw new FormatException();
        if (str.Length > 0) throw new FormatException();
        return chord;
    }

    public static ChordQuality ParseChordQuality(ReadOnlySpan<char> str)
    {
        var chordQuality = ConsumeChordQuality(ref str);
        if (str.Length > 0) throw new FormatException();
        return chordQuality;
    }

    private static bool ConsumeUriPrefix(ref ReadOnlyMemory<char> str)
    {
        var span = str.Span;
        bool proFormat = ConsumeUriPrefix(ref span);
        str = str[^span.Length..];
        return proFormat;
    }

    private static bool ConsumeUriPrefix(ref ReadOnlySpan<char> span)
    {
        if (SpanParsing.TryConsume(ref span, PseudoUriFormat.LegacyPrefix)) return false;
        else if (SpanParsing.TryConsume(ref span, PseudoUriFormat.ProPrefix)) return true;
        else throw new FormatException();
    }

    private static string? TryConsumePlaylistNameSuffix(ref ReadOnlyMemory<char> str)
    {
        var span = str.Span;
        var name = TryConsumePlaylistNameSuffix(ref span);
        str = str[0..span.Length];
        return name;
    }

    private static string? TryConsumePlaylistNameSuffix(ref ReadOnlySpan<char> str)
    {
        // ...===PlaylistName
        int lastEqualsIndex = str.LastIndexOf('=');
        if (lastEqualsIndex < PseudoUriFormat.ProPrefix.Length || lastEqualsIndex == str.Length - 1) return null;
        if (str[lastEqualsIndex - 1] != '=' || str[lastEqualsIndex - 2] != '=') return null;
        var name = new string(str[(lastEqualsIndex + 1)..]);
        str = str[0..(lastEqualsIndex + 1)]; // Remove the name, keep the 3 equals
        return name;
    }

    private static ReadOnlySpan<char> ConsumeBodyAsSpan(scoped ref ReadOnlySpan<char> str, bool proFormat)
    {
        int postBodyEqualsIndex = str.IndexOf('=');
        ReadOnlySpan<char> result;
        if (postBodyEqualsIndex == -1)
        {
            if (proFormat) throw new FormatException();
            result = str;
            str = default;
        }
        else
        {
            result = str[0..postBodyEqualsIndex];
            str = str[(postBodyEqualsIndex + 1)..];
        }

        return result;
    }

    // irealb:// URIs are obfuscated using a simple algorithm
    private static string DeobfuscateBody(ReadOnlySpan<char> str)
    {
        const string magicPrefix = "1r34LbKcu7";

        if (!SpanParsing.TryConsume(ref str, magicPrefix)) throw new FormatException();

        StringBuilder stringBuilder = new(capacity: str.Length);
        WriteShuffled(str, stringBuilder);

        // Remove obfuscating substitutions
        stringBuilder.Replace("XyQ", "   ");
        stringBuilder.Replace("LZ", " |");
        stringBuilder.Replace("Kcl", "| x");

        return stringBuilder.ToString();
    }

    private static void WriteShuffled(ReadOnlySpan<char> source, StringBuilder destination)
    {
        const int blockLength = 50;
        const int minLength = 52;
        while (source.Length >= minLength)
        {
            Write(source.Slice(45, 5), destination, reverse: true);
            Write(source.Slice(5, 5), destination, reverse: false);
            Write(source.Slice(26, 14), destination, reverse: true);
            Write(source.Slice(24, 2), destination, reverse: false);
            Write(source.Slice(10, 14), destination, reverse: true);
            Write(source.Slice(40, 5), destination, reverse: false);
            Write(source.Slice(0, 5), destination, reverse: true);
            SpanParsing.Skip(ref source, blockLength);
        }

        destination.Append(source);
    }

    private static void Write(ReadOnlySpan<char> source, StringBuilder destination, bool reverse)
    {
        if (reverse)
        {
            for (int i = source.Length - 1; i >= 0; i--)
                destination.Append(source[i]);
        }
        else destination.Append(source);
    }

    private static NoteClass? TryConsumeNoteClass(ref ReadOnlySpan<char> str)
    {
        if (SpanParsing.PeekOrNul(str) is < 'A' or > 'G') return null;
        NoteLetter noteLetter = NoteLetterEnum.TryFromChar(SpanParsing.ConsumeChar(ref str))!.Value;

        if (SpanParsing.TryConsume(ref str, 'b')) return noteLetter.Flat();
        if (SpanParsing.TryConsume(ref str, '#')) return noteLetter.Sharp();
        return noteLetter;
    }

    private static CellSymbol? TryConsumeCellSymbol(ref ReadOnlySpan<char> str)
    {
        if (TryConsumeChordSymbol(ref str, allowBassChange: true) is ChordSymbol chordSymbol) return chordSymbol;
        if (SpanParsing.TryConsume(ref str, 'n')) return CellSymbol.NoChord;
        if (SpanParsing.TryConsume(ref str, 'p')) return CellSymbol.ChordRepeatSlash;
        if (SpanParsing.TryConsume(ref str, 'x')) return CellSymbol.SingleMeasureRepeat;
        if (SpanParsing.TryConsume(ref str, 'r')) return CellSymbol.DoubleMeasureRepeat;
        return null;
    }

    private static ChordSymbol? TryConsumeChordSymbol(ref ReadOnlySpan<char> str, bool allowBassChange)
    {
        if (allowBassChange && SpanParsing.TryConsume(ref str, 'W'))
        {
            if (!SpanParsing.TryConsume(ref str, '/')) throw new FormatException();
            return ChordSymbol.BassChange(TryConsumeNoteClass(ref str) ?? throw new FormatException());
        }

        if (TryConsumeNoteClass(ref str) is not NoteClass root) return null;

        ChordQuality quality;
        if (SpanParsing.TryConsume(ref str, '*'))
        {
            var bogusQualityStr = SpanParsing.TryConsumeUntil(ref str, '*', skipTerminator: true) ?? throw new FormatException();
            quality = default;
        }
        else
            quality = ConsumeChordQuality(ref str);

        NoteClass? bass = null;
        if (SpanParsing.TryConsume(ref str, '/'))
            bass = TryConsumeNoteClass(ref str) ?? throw new FormatException();

        return new ChordSymbol(root, quality, bass);
    }

    private static ChordQuality ConsumeChordQuality(ref ReadOnlySpan<char> str)
    {
        var baseQuality = ParseOptChordBaseQualityPrefix(ref str);
        var extension = ParseOptChordExtension(ref str, baseQuality);
        var tweaks = ParseOptChordTweaksAndBaseQualitySuffix(ref str, ref baseQuality);
        return new ChordQuality(baseQuality, extension, tweaks);
    }

    private static ImmutableArray<ChordTweak> ParseOptChordTweaksAndBaseQualitySuffix(
        ref ReadOnlySpan<char> str, ref ChordBaseQuality baseQuality)
    {
        ImmutableArray<ChordTweak>.Builder? tweakArrayBuilder = null;
        while (true)
        {
            if (TryConsumeChordTweak(ref str) is ChordTweak tweak)
            {
                tweakArrayBuilder ??= ImmutableArray.CreateBuilder<ChordTweak>();
                tweakArrayBuilder.Add(tweak);
            }
            else if (SpanParsing.TryConsume(ref str, "sus"))
            {
                if (baseQuality != ChordBaseQuality.ImplicitMajor) throw new FormatException();
                baseQuality = ChordBaseQuality.Suspended4;
            }
            else if (SpanParsing.TryConsume(ref str, "alt"))
            {
                if (baseQuality != ChordBaseQuality.ImplicitMajor) throw new FormatException();
                baseQuality = ChordBaseQuality.Altered;
            }
            else break;
        }

        if (tweakArrayBuilder is null) return ImmutableArray<ChordTweak>.Empty;

        tweakArrayBuilder.Capacity = tweakArrayBuilder.Count;
        return tweakArrayBuilder.MoveToImmutable();
    }

    private static ChordBaseQuality ParseOptChordBaseQualityPrefix(ref ReadOnlySpan<char> str)
    {
        if (SpanParsing.TryConsume(ref str, '-')) return ChordBaseQuality.Minor;
        if (SpanParsing.TryConsume(ref str, 'o')) return ChordBaseQuality.Diminished;
        if (SpanParsing.TryConsume(ref str, 'h')) return ChordBaseQuality.HalfDiminished;
        if (SpanParsing.TryConsume(ref str, '+')) return ChordBaseQuality.Augmented;
        if (SpanParsing.PeekOrNul(str) is '^' && SpanParsing.AtOrNul(str, 1) is not '7' and not '9' and not '1')
        {
            SpanParsing.ConsumeChar(ref str);
            return ChordBaseQuality.ExplicitMajor;
        }

        if (SpanParsing.TryConsume(ref str, '2')) return ChordBaseQuality.Suspended2; // Sus2 or add2?
        if (SpanParsing.TryConsume(ref str, '5')) return ChordBaseQuality.Power;
        return ChordBaseQuality.ImplicitMajor;
    }

    private static JazzChordExtension? ParseOptChordExtension(ref ReadOnlySpan<char> str, ChordBaseQuality baseQuality)
    {
        bool major = SpanParsing.TryConsume(ref str, '^');

        if (SpanParsing.TryConsume(ref str, '7')) return new(TertianChordDegree.Seventh, major);
        if (SpanParsing.TryConsume(ref str, '9')) return new(TertianChordDegree.Ninth, major);
        if (SpanParsing.TryConsume(ref str, "11")) return new(TertianChordDegree.Eleventh, major);
        if (SpanParsing.TryConsume(ref str, "13")) return new(TertianChordDegree.Thirteenth, major);
        if (major) throw new FormatException();
        return baseQuality.ImpliesSeventh() ? JazzChordExtension.Seventh : null;
    }

    private static ChordTweak? TryConsumeChordTweak(ref ReadOnlySpan<char> str)
    {
        ChordTweakType? type = null;
        if (SpanParsing.TryConsume(ref str, "add")) type = ChordTweakType.Add;
        else if (SpanParsing.TryConsume(ref str, 'b')) type = ChordTweakType.Flat;
        else if (SpanParsing.TryConsume(ref str, '#')) type = ChordTweakType.Sharp;

        int degreeNumber;
        if (SpanParsing.PeekOrNul(str) is '3' or '5' or '6' or '7' or '9') // susadd3 is legal
            degreeNumber = SpanParsing.ConsumeChar(ref str) - '0';
        else if (str.StartsWith("11", StringComparison.Ordinal) || str.StartsWith("13", StringComparison.Ordinal))
        {
            degreeNumber = 10 + (str[1] - '0');
            SpanParsing.Skip(ref str, 2);
        }
        else
        {
            if (type != null) throw new FormatException();
            return null;
        }

        return (type ?? ChordTweakType.Add) switch
        {
            ChordTweakType.Add => ChordTweak.Add(degreeNumber),
            ChordTweakType.Flat => ChordTweak.Flat(degreeNumber),
            ChordTweakType.Sharp => ChordTweak.Sharp(degreeNumber),
            _ => throw new UnreachableException()
        };
    }

    private static void ConsumePostCell(ref ReadOnlySpan<char> str)
    {
        if (SpanParsing.TryConsume(ref str, ',')) return;
        if (SpanParsing.PeekOrNul(str) is not ' ' and not '(') throw new FormatException();
    }

    private static SongMetadata ConsumeMetadataPrefix(ref ReadOnlySpan<char> str, bool proFormat)
    {
        var title = ConsumeHeaderField(ref str, "title");
        var composer = ConsumeHeaderField(ref str, "composer");
        if (proFormat) _ = ConsumeHeaderField(ref str, "?");
        var style = ConsumeHeaderField(ref str, "style");

        DiatonicKey? key;
        if (proFormat) key = ParseOptKey(ConsumeHeaderField(ref str, "key"));
        else
        {
            var preKeyN = ConsumeHeaderField(ref str, "n");
            var keyStr = ConsumeHeaderField(ref str, "key");
            if (keyStr == "n") (preKeyN, keyStr) = (keyStr, preKeyN);
            key = ParseOptKey(keyStr);
        }

        var actualKey = proFormat ? ParseOptKey(ConsumeHeaderField(ref str, "actual key")) : null;

        string? actualStyle = null;
        int tempoOrZero = 0;
        int repeatCountOrZero = 0;
        if (proFormat)
        {
            // Skip chart body
            var suffixStr = str;
            SpanParsing.Skip(ref suffixStr, suffixStr.IndexOf('=') + 1);

            actualStyle = ConsumeHeaderField(ref suffixStr, "actual style");
            tempoOrZero = int.Parse(ConsumeHeaderField(ref suffixStr, "actual tempo") ?? "0", NumberStyles.None, CultureInfo.InvariantCulture);
            repeatCountOrZero = int.Parse(ConsumeHeaderField(ref suffixStr, "actual repeats") ?? "0", NumberStyles.None, CultureInfo.InvariantCulture);
        }

        return new SongMetadata
        {
            Title = title,
            ComposerLastFirstName = composer,
            Style = style,
            Key = key,
            ActualKey = actualKey,
            ActualStyle = actualStyle,
            Tempo = tempoOrZero == 0 ? null : tempoOrZero,
            RepeatCount = repeatCountOrZero == 0 ? null : repeatCountOrZero
        };
    }

    private static string? ConsumeHeaderField(ref ReadOnlySpan<char> str, string name)
    {
        var field = SpanParsing.TryConsumeUntil(ref str, '=', skipTerminator: true)
            ?? throw new FormatException($"Missing header field: {name}");
        return field.Length == 0 ? null : field;
    }

    private static DiatonicKey? ParseOptKey(string? str) => str is null ? null : ParseKey(str.AsSpan());

    private static DiatonicKey ParseKey(ReadOnlySpan<char> str)
    {
        var keyTonic = TryConsumeNoteClass(ref str) ?? throw new FormatException();
        var majorOrMinor = SpanParsing.TryConsume(ref str, '-') ? MajorOrMinor.Minor : MajorOrMinor.Major;
        if (str.Length > 0) throw new FormatException();
        return new DiatonicKey(keyTonic, majorOrMinor);
    }
}

public enum UriParserState
{
    BeforeSongMetadata,
    BeforeChartBody,
    End,
}
