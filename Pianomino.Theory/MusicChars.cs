using System;
using System.Text;

namespace Pianomino;

public static class MusicChars
{
    public const char AsciiFlat = 'b';
    public const char AsciiSharp = '#';
    public const char Flat = '♭';
    public const char Sharp = '♯';
    public const char Natural = '♮';
    public static readonly Rune DoubleFlat = Rune.GetRuneAt("𝄫", 0);
    public static readonly Rune DoubleSharp = Rune.GetRuneAt("𝄪", 0);

    public const char MajorChordTriangle = '△';
    public const char HalfDiminishedChordSlashedO = 'ø';

    public const char QuarterNoteEmoji = '♩';
    public const char EighthNoteEmoji = '♪';

    public static readonly Rune DalSegno = Rune.GetRuneAt("𝄉", 0);
    public static readonly Rune DaCapo = Rune.GetRuneAt("𝄊", 0);
    public static readonly Rune Segno = Rune.GetRuneAt("𝄋", 0);
    public static readonly Rune Coda = Rune.GetRuneAt("𝄌", 0);

    public static readonly Rune GClef = Rune.GetRuneAt("𝄞", 0);
    public static readonly Rune CClef = Rune.GetRuneAt("𝄡", 0);
    public static readonly Rune FClef = Rune.GetRuneAt("𝄢", 0);

    public static readonly Rune CommonTime = Rune.GetRuneAt("𝄴", 0);
    public static readonly Rune CutTime = Rune.GetRuneAt("𝄵", 0);

    public static readonly Rune WholeRest = Rune.GetRuneAt("𝄻", 0);
    public static readonly Rune HalfRest = Rune.GetRuneAt("𝄼", 0);
    public static readonly Rune QuarterRest = Rune.GetRuneAt("𝄽", 0);
    public static readonly Rune EightRest = Rune.GetRuneAt("𝄾", 0);
    public static readonly Rune SixteenthRest = Rune.GetRuneAt("𝄿", 0);
    public static readonly Rune ThirtySecondRest = Rune.GetRuneAt("𝅀", 0);

    public static readonly Rune WholeNote = Rune.GetRuneAt("𝅝", 0);
    public static readonly Rune HalfNote = Rune.GetRuneAt("𝅗𝅥", 0);
    public static readonly Rune QuarterNote = Rune.GetRuneAt("𝅘𝅥", 0);
    public static readonly Rune EightNote = Rune.GetRuneAt("𝅘𝅥𝅮", 0);
    public static readonly Rune SixteenthNote = Rune.GetRuneAt("𝅘𝅥𝅯", 0);
    public static readonly Rune ThirtySecondNote = Rune.GetRuneAt("𝅘𝅥𝅰", 0);

    public static readonly Rune AugmentationDot = Rune.GetRuneAt("𝅭", 0);

    public static readonly Rune Mezzo = Rune.GetRuneAt("𝆐", 0);
    public static readonly Rune Piano = Rune.GetRuneAt("𝆏", 0);
    public static readonly Rune Forte = Rune.GetRuneAt("𝆑", 0);

    public static readonly Rune SingleBarline = Rune.GetRuneAt("𝄀", 0);
    public static readonly Rune DoubleBarline = Rune.GetRuneAt("𝄁", 0);
    public static readonly Rune FinalBarline = Rune.GetRuneAt("𝄂", 0);
    public static readonly Rune ReverseFinalBarline = Rune.GetRuneAt("𝄃", 0);
    public static readonly Rune DashedBarline = Rune.GetRuneAt("𝄄", 0);
    public static readonly Rune LeftRepeatBarline = Rune.GetRuneAt("𝄆", 0);
    public static readonly Rune RightRepeatBarline = Rune.GetRuneAt("𝄇", 0);
    public static readonly Rune BarlineRepeatDots = Rune.GetRuneAt("𝄈", 0);

    public static readonly Rune StaffBracket = Rune.GetRuneAt("𝄕", 0);
    public static readonly Rune StaffBrace = Rune.GetRuneAt("𝄔", 0);

    public static readonly Rune ChordSimile = Rune.GetRuneAt("𝄍", 0);
    public static readonly Rune MeasureSimile = Rune.GetRuneAt("𝄎", 0);
    public static readonly Rune DoubleMeasureSimile = Rune.GetRuneAt("𝄏", 0);
}
