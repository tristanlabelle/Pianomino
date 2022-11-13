using Pianomino.Theory;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Pianomino.Formats.iReal;

public static class UriParserTests
{
    [Fact]
    public static void TestParseSongMetadata()
    {
        var metadata = PseudoUriParser.ParseFirstSongMetadata("irealbook://A=B=C=D=n=[C]");
        Assert.Equal("A", metadata.Title);
        Assert.Equal("B", metadata.ComposerLastFirstName);
        Assert.Equal("C", metadata.Style);
        Assert.Equal(DiatonicKey.Major(NoteLetter.D), metadata.Key);
    }

    [Fact]
    public static void TestParseProSongMetadata()
    {
        var metadata = PseudoUriParser.ParseFirstSongMetadata("irealb://A=B==C=D==1r34LbKcu7|C|==1=2===");
        Assert.Equal("A", metadata.Title);
        Assert.Equal("B", metadata.ComposerLastFirstName);
        Assert.Equal("C", metadata.Style);
        Assert.Equal(DiatonicKey.Major(NoteLetter.D), metadata.Key);
        Assert.Equal(1, metadata.Tempo);
        Assert.Equal(2, metadata.RepeatCount);
    }

    [Fact]
    public static void TestParseProPlaylist()
    {
        PseudoUriParser parser = new("irealb://Foo=Alice==Swing=C==1r34LbKcu7|C|==0=0===Bar=Bob==Medium=C==1r34LbKcu7|C|==0=0===List");
        Assert.True(parser.IsProFormat);
        Assert.Equal("List", parser.PlaylistName);
        parser.ParseSongMetadata();
        parser.ParseChartBody(NullSink.Instance);
        parser.ParseSongMetadata();
        parser.SkipChartBody();
        Assert.Equal(UriParserState.End, parser.State);
    }

    [Fact]
    public static void TestUrlDecoding()
    {
        var header = PseudoUriParser.ParseFirstSongMetadata("irealbook://A=B%20C=D=E=n=[C]");
        Assert.Equal("B C", header.ComposerLastFirstName);
    }

    [Fact]
    public static void TestParseKeys()
    {
        Assert.Equal(
            DiatonicKey.Major(NoteLetter.C),
            PseudoUriParser.ParseFirstSongMetadata("irealbook://_=_=_=C=n=[C]").Key);
        Assert.Equal(
            DiatonicKey.Major(NoteLetter.B.Flat()),
            PseudoUriParser.ParseFirstSongMetadata("irealbook://_=_=_=Bb=n=[C]").Key);
        Assert.Equal(
            DiatonicKey.Minor(NoteLetter.C.Sharp()),
            PseudoUriParser.ParseFirstSongMetadata("irealbook://_=_=_=C#-=n=[C]").Key);
    }

    [Fact]
    public static void TestParseBodyConstructs()
    {
        PseudoUriParser.ParseBody("|[]{}Z", NullSink.Instance); // Barlines
        PseudoUriParser.ParseBody("T44T12", NullSink.Instance); // Time signatures
        PseudoUriParser.ParseBody("*A*B*C*D*v*i", NullSink.Instance); // Rehearsal marks
        PseudoUriParser.ParseBody("SQf", NullSink.Instance); // Segno, coda, fermata
        PseudoUriParser.ParseBody("N1N2N3N0", NullSink.Instance); // Endings
        PseudoUriParser.ParseBody("<foo><*42bar>", NullSink.Instance); // Staff text
        PseudoUriParser.ParseBody("Y|YY|YYY", NullSink.Instance); // Vertical space
        PseudoUriParser.ParseBody("sl", NullSink.Instance); // Chord size
        PseudoUriParser.ParseBody("n", NullSink.Instance); // No chord
        PseudoUriParser.ParseBody("p", NullSink.Instance); // Repeat chord
        PseudoUriParser.ParseBody("p,p,", NullSink.Instance); // Packed repeat chord
        PseudoUriParser.ParseBody("x", NullSink.Instance); // Single measure repeat
        PseudoUriParser.ParseBody("r", NullSink.Instance); // Double measure repeat
        PseudoUriParser.ParseBody("U", NullSink.Instance); // End
    }

    [Fact]
    public static void TestParseChordFormats()
    {
        PseudoUriParser.ParseBody("C", NullSink.Instance);
        PseudoUriParser.ParseBody("Bb", NullSink.Instance);
        PseudoUriParser.ParseBody("C#", NullSink.Instance);
        PseudoUriParser.ParseBody("C-", NullSink.Instance);
        PseudoUriParser.ParseBody("C/G", NullSink.Instance);
        PseudoUriParser.ParseBody("C(G)", NullSink.Instance);
        PseudoUriParser.ParseBody("C(G-)", NullSink.Instance);
        PseudoUriParser.ParseBody("C(G/D)", NullSink.Instance);
        PseudoUriParser.ParseBody("C#-/G#(Ab-/Eb)", NullSink.Instance);
        PseudoUriParser.ParseBody("C(G),", NullSink.Instance);
        PseudoUriParser.ParseBody("W/C", NullSink.Instance);
        PseudoUriParser.ParseBody("C*crap*", NullSink.Instance);
    }

    [Fact]
    public static void TestParseChords()
    {
        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C");
            Assert.Equal(NoteLetter.C, chordSymbol.Root);
            Assert.Equal(ChordBaseQuality.ImplicitMajor, chordSymbol.Quality.BaseQuality);
            Assert.Null(chordSymbol.Bass);
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("Bb");
            Assert.Equal(NoteLetter.B.Flat(), chordSymbol.Root);
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C-");
            Assert.Equal(ChordBaseQuality.Minor, chordSymbol.Quality.BaseQuality);
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C9");
            Assert.Equal(ChordBaseQuality.ImplicitMajor, chordSymbol.Quality.BaseQuality);
            Assert.Equal(JazzChordExtension.Ninth, chordSymbol.Quality.Extension);
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C-^9");
            Assert.Equal(ChordBaseQuality.Minor, chordSymbol.Quality.BaseQuality);
            Assert.Equal(JazzChordExtension.MajorNinth, chordSymbol.Quality.Extension);
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C7#9");
            Assert.Equal(JazzChordExtension.Seventh, chordSymbol.Quality.Extension);
            Assert.Equal(ChordTweak.Sharp(9), Assert.Single(chordSymbol.Quality.Tweaks));
        }

        {
            var chordSymbol = PseudoUriParser.ParseChordSymbol("C/G");
            Assert.Equal(NoteLetter.C, chordSymbol.Root);
            Assert.Equal(NoteLetter.G, chordSymbol.Bass);
        }
    }

    [Fact]
    public static void TestParseChordQualities()
    {
        var qualities = Regex.Split(@"5 2 add9 + o h sus ^ -
				^7 -7 7 7sus h7 o7 ^9 ^13 6 69 ^7#11 ^9#11 ^7#5 -6 -69 -^7 -^9 -9 -11 -7b5 h9
				-b6 -#5 9 7b9 7#9 7#11 7b5 7#5 9#11 9b5 9#5 7b13 7#9#5 7#9b5 7#9#11 7b9#11 7b9b5
				7b9#5 7b9#9 7b9b13 7alt 13 13#11 13b9 13#9 7b9sus 7susadd3 9sus 13sus 7b13sus 11", @"\s+");

        Assert.Equal(ChordBaseQuality.ImplicitMajor, PseudoUriParser.ParseChordQuality("").BaseQuality);
        foreach (var quality in qualities)
            PseudoUriParser.ParseChordQuality(quality);
    }
}
