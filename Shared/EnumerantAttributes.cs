using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pianomino;

internal static class EnumerantAttributes
{
    public static TAttribute? TryGetSingle<TEnum, TAttribute>(TEnum enumerant)
        where TEnum : struct
        where TAttribute : Attribute
        => SingleCache<TEnum, TAttribute>.Map.GetValueOrDefault(enumerant);

    public static Dictionary<TKey, TEnum> CreateDictionary<TEnum, TAttribute, TKey>(
        Func<TAttribute, TKey> keySelector, IEqualityComparer<TKey>? keyComparer)
        where TEnum : struct
        where TAttribute : Attribute
        where TKey : notnull
    {
        var fields = typeof(TKey).GetFields(BindingFlags.Public | BindingFlags.Static);
        Dictionary<TKey, TEnum> dictionary = new(capacity: fields.Length, keyComparer);
        foreach (var field in fields)
            if (field.GetCustomAttribute<TAttribute>() is TAttribute attribute)
                dictionary.Add(keySelector(attribute), (TEnum)field.GetValue(null)!);

        return dictionary;
    }

    private static class SingleCache<TEnum, TAttribute>
        where TEnum : struct
        where TAttribute : Attribute
    {
        public static readonly Dictionary<TEnum, TAttribute> Map = new();

        static SingleCache()
        {
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttributes<TAttribute>().SingleOrDefault();
                if (attribute is not null) Map.Add((TEnum)field.GetValue(null)!, attribute);
            }
        }
    }
}
