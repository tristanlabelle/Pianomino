using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino;

internal static class SpanExtensions
{
    public static bool StartsWith_Ordinal(this ReadOnlySpan<char> span, string str)
    {
        if (span.Length < str.Length) return false;
        for (int i = 0; i < str.Length; ++i)
            if (span[i] != str[i])
                return false;
        return true;
    }

    public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> span)
    {
        if (span.IsEmpty) return ImmutableArray<T>.Empty;

        var builder = ImmutableArray.CreateBuilder<T>(initialCapacity: span.Length);
        foreach (var item in span) builder.Add(item);
        return builder.MoveToImmutable();
    }
}
