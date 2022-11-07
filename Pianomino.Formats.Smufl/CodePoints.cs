using System;
using System.Text;

namespace Pianomino.Formats.Smufl;

/// <summary>
/// Provides constants for SMuFL Unicode code points.
/// </summary>
public static class CodePoints
{
    // Repeats
    public const char RepeatDots = '\uE043';
    public const char RepeatDot = '\uE044';
    public const char DalSegno = '\uE045';
    public const char DaCapo = '\uE046';
    public const char Segno = '\uE047';
    public const char Coda = '\uE048';

    // Noteheads
    public const char NoteheadDoubleWhole = '\uE0A0';
    public const char NoteheadDoubleWholeSquare = '\uE0A1';
    public const char NoteheadWhole = '\uE0A2';
    public const char NoteheadHalf = '\uE0A3';
    public const char NoteheadBlack = '\uE0A4';

    // Rests
    public const char RestMaxima = '\uE4E0';
    public const char RestLonga = '\uE4E1';
    public const char RestDoubleWhole = '\uE4E2';
    public const char RestWhole = '\uE4E3';
    public const char RestHalf = '\uE4E4';
    public const char RestQuarter = '\uE4E5';
    public const char Rest8th = '\uE4E6';
    public const char Rest16th = '\uE4E7';
    public const char Rest32nd = '\uE4E8';
    public const char Rest64th = '\uE4E9';
    public const char Rest128th = '\uE4EA';
    public const char Rest256th = '\uE4EB';
    public const char Rest512nd = '\uE4EC';
    public const char Rest1024th = '\uE4ED';

    // Individual notes
    public const char AugmentationDot = '\uE1E7';

    // Clefs
    public const char GClef = '\uE050';
    public const char CClef = '\uE05B';
    public const char FClef = '\uE061';
    public const char UnpitchedPercussionClef1 = '\uE068';
    public const char UnpitchedPercussionClef2 = '\uE069';

    // Time signatures
    public const char TimeSig0 = '\uE080';
    public const char TimeSig1 = '\uE081';
    public const char TimeSig2 = '\uE082';
    public const char TimeSig3 = '\uE083';
    public const char TimeSig4 = '\uE084';
    public const char TimeSig5 = '\uE085';
    public const char TimeSig6 = '\uE086';
    public const char TimeSig7 = '\uE087';
    public const char TimeSig8 = '\uE088';
    public const char TimeSig9 = '\uE089';
    public const char TimeSigCommon = '\uE08A';
    public const char TimeSigCutCommon = '\uE08B';
    public const char TimeSigPlus = '\uE08C';
    public const char TimeSigPlusSmall = '\uE08D';
    public const char TimeSigFractionalSlash = '\uE08E';
    public const char TimeSigEquals = '\uE08F';

    // Flags
    public const char Flag8thUp = '\uE240';
    public const char Flag8thDown = '\uE241';
    public const char Flag16thUp = '\uE242';
    public const char Flag16thDown = '\uE243';
    public const char Flag32ndUp = '\uE244';
    public const char Flag32ndDown = '\uE245';
    public const char Flag64thUp = '\uE246';
    public const char Flag64thDown = '\uE247';
    public const char Flag128thUp = '\uE248';
    public const char Flag128thDown = '\uE249';
    public const char Flag256thUp = '\uE24A';
    public const char Flag256thDown = '\uE24B';
    public const char Flag512ndUp = '\uE24C';
    public const char Flag512ndDown = '\uE24D';
    public const char Flag1024thUp = '\uE24E';
    public const char Flag1024thDown = '\uE24F';

    // Standard accidentals
    public const char AccidentalFlat = '\uE260';
    public const char AccidentalNatural = '\uE261';
    public const char AccidentalSharp = '\uE262';
    public const char AccidentalDoubleSharp = '\uE263';
    public const char AccidentalDoubleFlat = '\uE264';
    public const char AccidentalTripleSharp = '\uE265';
    public const char AccidentalTripleFlat = '\uE266';
    public const char AccidentalNaturalFlat = '\uE267';
    public const char AccidentalNaturalSharp = '\uE268';
    public const char AccidentalSharpSharp = '\uE269';

    // Articulation
    public const char AccentAbove = '\uE4A0';
    public const char AccentBelow = '\uE4A1';
    public const char ArticStaccatoAbove = '\uE4A2';
    public const char ArticStaccatoBelow = '\uE4A3';
    public const char ArticTenutoAbove = '\uE4A4';
    public const char ArticTenutoBelow = '\uE4A5';

    public static bool IsValid(char codePoint) => char.GetUnicodeCategory(codePoint) == System.Globalization.UnicodeCategory.PrivateUse;

    public static char GetFlag(NoteUnit noteUnit, FlagDirection direction)
        => GetFlag(noteUnit.GetNoteFlagCount(), direction);

    public static char GetFlag(int count, FlagDirection direction)
    {
        if (count <= 0 || count > NoteUnitEnum.Shortest.GetNoteFlagCount())
            throw new ArgumentOutOfRangeException(nameof(count));

        var flag8th = direction == FlagDirection.Up ? Flag8thUp : Flag8thDown;
        return (char)(flag8th + (count - 1) * 2);
    }

    public static char GetRest(NoteUnit noteUnit)
    {
        if (noteUnit < NoteUnitEnum.Shortest || noteUnit > NoteUnitEnum.Longest)
            throw new ArgumentOutOfRangeException(nameof(noteUnit));
        return (char)(RestWhole - noteUnit.GetLog2());
    }

    public static char GetNotehead(NoteUnit noteUnit)
    {
        if (noteUnit <= NoteUnit.Quarter) return NoteheadBlack;
        if (noteUnit == NoteUnit.Half) return NoteheadHalf;
        if (noteUnit == NoteUnit.Whole) return NoteheadWhole;
        if (noteUnit == NoteUnit.DoubleWhole) return NoteheadDoubleWhole;
        throw new ArgumentOutOfRangeException(nameof(noteUnit));
    }

    public static char GetAccidental(int alteration) => alteration switch
    {
        -3 => AccidentalTripleFlat,
        -2 => AccidentalDoubleFlat,
        -1 => AccidentalFlat,
        0 => AccidentalNatural,
        1 => AccidentalSharp,
        2 => AccidentalDoubleSharp,
        3 => AccidentalTripleSharp,
        _ => throw new ArgumentException(),
    };

    public static char GetTimeSignatureDigit(int value)
    {
        if ((uint)value >= 10) throw new ArgumentOutOfRangeException(nameof(value));
        return (char)(TimeSig0 + value);
    }
}
