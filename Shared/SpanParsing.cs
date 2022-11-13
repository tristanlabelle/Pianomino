using System;
using System.Globalization;
using System.Text;

namespace Pianomino;

/// <summary>
/// Provides string parsing building blocks based on <see cref="ReadOnlySpan{T}"/>.
/// </summary>
internal static class SpanParsing
{
    public static void Skip(ref ReadOnlySpan<char> str, int length)
    {
        str = str[length..];
    }

    public static bool SkipWhiteSpace(ref ReadOnlySpan<char> str)
    {
        int length = str.Length;
        str = str.TrimStart();
        return str.Length != length;
    }

    public static bool TrySkipUntil(ref ReadOnlySpan<char> str, char terminator, bool skipTerminator)
    {
        int index = str.IndexOf(terminator);
        if (index < 0) return false;
        Skip(ref str, skipTerminator ? index + 1 : index);
        return true;
    }

    public static int SkipZeroOrMore(ref ReadOnlySpan<char> str, char c)
    {
        int count = 0;
        while (count < str.Length && str[count] == c)
            count++;
        Skip(ref str, count);
        return count;
    }

    public static int SkipZeroOrMore(ref ReadOnlySpan<char> str, string token)
    {
        int count = 0;
        while (TryConsume(ref str, token)) count++;
        return count;
    }

    public static char ConsumeChar(ref ReadOnlySpan<char> str)
    {
        if (str.IsEmpty) throw new ArgumentException(message: null, paramName: nameof(str));
        var c = str[0];
        Skip(ref str, 1);
        return c;
    }

    public static char? TryConsumeChar(ref ReadOnlySpan<char> str)
    {
        if (str.IsEmpty) return null;
        var c = str[0];
        Skip(ref str, 1);
        return c;
    }

    public static Rune? TryConsumeRune(ref ReadOnlySpan<char> str)
    {
        if (str.IsEmpty) return null;
        Rune.DecodeFromUtf16(str, out Rune rune, out int charsConsumed);
        Skip(ref str, charsConsumed);
        return rune;
    }

    public static string? TryConsumeAsciiLetters(ref ReadOnlySpan<char> str)
    {
        int length = 0;
        while (length < str.Length && str[length] is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
            length++;
        if (length == 0) return null;

        var result = new string(str[0..length]);
        Skip(ref str, length);
        return result;
    }

    public static string? TryConsumeLetters(ref ReadOnlySpan<char> str)
    {
        int length = 0;
        while (length < str.Length && char.IsLetter(str[length]))
            length++;
        if (length == 0) return null;

        var result = new string(str[0..length]);
        Skip(ref str, length);
        return result;
    }

    public static string ConsumeNonWhiteSpaces(ref ReadOnlySpan<char> str)
    {
        int length = 0;
        while (length < str.Length && !char.IsWhiteSpace(str[length]))
            length++;
        return length == 0 ? string.Empty : ConsumeString(ref str, length);
    }

    public static byte? TryConsumeDigit(ref ReadOnlySpan<char> str)
    {
        if (str.Length == 0 || str[0] is < '0' or > '9') return null;
        return (byte)(ConsumeChar(ref str) - '0');
    }

    public static int? TryConsumeInteger(ref ReadOnlySpan<char> str, bool allowSign)
    {
        int length = 0;
        if (length < str.Length && str[0] == '-' && allowSign) length++;
        while (length < str.Length && str[length] is >= '0' and <= '9')
            length++;
        if (length == 0 || str[length - 1] == '-') return null;

        var result = int.Parse(str[0..length], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        Skip(ref str, length);
        return result;
    }

    public static string ConsumeString(ref ReadOnlySpan<char> str, int length)
    {
        if (length < 0 || length > str.Length) throw new ArgumentOutOfRangeException(nameof(length));
        var result = new string(str[0..length]);
        Skip(ref str, length);
        return result;
    }

    public static bool TryConsume(ref ReadOnlySpan<char> str, char c)
    {
        if (str.Length == 0 || str[0] != c) return false;
        Skip(ref str, 1);
        return true;
    }

    public static bool TryConsume(ref ReadOnlySpan<char> str, string token)
    {
        if (!str.StartsWith(token)) return false;
        Skip(ref str, token.Length);
        return true;
    }

    public static string? TryConsumeUntil(ref ReadOnlySpan<char> str, char terminator, bool skipTerminator)
    {
        int length = 0;
        while (true)
        {
            if (length == str.Length) return null;
            if (str[length] == terminator) break;
            length++;
        }

        var result = ConsumeString(ref str, length);
        if (skipTerminator) Skip(ref str, 1);
        return result;
    }

    public static char PeekOrNul(ReadOnlySpan<char> str) => str.Length > 0 ? str[0] : default;

    public static char AtOrNul(ReadOnlySpan<char> str, int ahead) => ahead < str.Length ? str[ahead] : default;

    public static Rune PeekRuneOrNul(ReadOnlySpan<char> str, out int length)
    {
        if (str.IsEmpty)
        {
            length = 0;
            return default;
        }

        Rune.DecodeFromUtf16(str, out Rune rune, out length);
        return rune;
    }
}
