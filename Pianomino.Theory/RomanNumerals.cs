using System;

namespace Pianomino;

public static class RomanNumerals
{
    private static readonly string[] asciiUpper = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
    private static readonly string[] asciiLower = { "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x" };
    private static readonly string[] unicodeUpper = { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ" };
    private static readonly string[] unicodeLower = { "ⅰ", "ⅱ", "ⅲ", "ⅳ", "ⅴ", "ⅵ", "ⅶ", "ⅷ", "ⅸ", "ⅹ" };

    public const char UnicodeUpperI = 'Ⅰ';
    public const char UnicodeUpperII = 'Ⅱ';
    public const char UnicodeUpperIII = 'Ⅲ';
    public const char UnicodeUpperIV = 'Ⅳ';
    public const char UnicodeUpperV = 'Ⅴ';
    public const char UnicodeUpperVI = 'Ⅵ';
    public const char UnicodeUpperVII = 'Ⅶ';
    public const char UnicodeUpperVIII = 'Ⅷ';
    public const char UnicodeUpperIX = 'Ⅸ';
    public const char UnicodeUpperX = 'Ⅹ';
    public const char UnicodeUpperXI = 'Ⅺ';
    public const char UnicodeUpperXII = 'Ⅻ';
    public const char UnicodeUpperL = 'Ⅼ';
    public const char UnicodeUpperC = 'Ⅽ';
    public const char UnicodeUpperD = 'Ⅾ';
    public const char UnicodeUpperM = 'Ⅿ';

    public const char UnicodeLowerI = 'ⅰ';
    public const char UnicodeLowerII = 'ⅱ';
    public const char UnicodeLowerIII = 'ⅲ';
    public const char UnicodeLowerIV = 'ⅳ';
    public const char UnicodeLowerV = 'ⅴ';
    public const char UnicodeLowerVI = 'ⅵ';
    public const char UnicodeLowerVII = 'ⅶ';
    public const char UnicodeLowerVIII = 'ⅷ';
    public const char UnicodeLowerIX = 'ⅸ';
    public const char UnicodeLowerX = 'ⅹ';
    public const char UnicodeLowerXI = 'ⅺ';
    public const char UnicodeLowerXII = 'ⅻ';
    public const char UnicodeLowerL = 'ⅼ';
    public const char UnicodeLowerC = 'ⅽ';
    public const char UnicodeLowerD = 'ⅾ';
    public const char UnicodeLowerM = 'ⅿ';

    public static string Get(int value, RomanNumeralFlags flags = default)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (value > 10) throw new NotImplementedException();
        return (flags.IsUnicode()
            ? (flags.IsUpper() ? unicodeUpper : unicodeLower)
            : (flags.IsUpper() ? asciiUpper : asciiLower))[value - 1];
    }

    public static RomanNumeralFlags GetUpperUnicodeFlags(bool upper, bool unicode)
       => (upper ? default : RomanNumeralFlags.LowerCase) | (unicode ? RomanNumeralFlags.Unicode : RomanNumeralFlags.Default);

    public static bool IsUpper(this RomanNumeralFlags value) => (value & RomanNumeralFlags.LowerCase) == 0;
    public static bool IsUnicode(this RomanNumeralFlags value) => (value & RomanNumeralFlags.Unicode) != 0;

    public static bool HasAll(this RomanNumeralFlags value, RomanNumeralFlags mask)
        => (value & mask) == mask;

    public static bool HasAny(this RomanNumeralFlags value, RomanNumeralFlags mask)
        => (value & mask) != 0;
}

[Flags]
public enum RomanNumeralFlags : byte
{
    Default = 0,
    LowerCase = 1,
    Unicode = 2,

    UpperAscii = Default,
    LowerAscii = LowerCase,
    UpperUnicode = Unicode,
    LowerUnicode = LowerCase | Unicode,
}
