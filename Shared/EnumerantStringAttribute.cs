using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino;

/// <summary>
/// A general-purpose attribute to associate a string value to an enumerant.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
internal sealed class EnumerantStringAttribute : Attribute
{
    public string Value { get; }

    public EnumerantStringAttribute(string value) => this.Value = value;

    public static string? TryGet<TEnum>(TEnum enumerant) where TEnum : struct
        => EnumerantAttributes.TryGetSingle<TEnum, EnumerantStringAttribute>(enumerant)?.Value;

    public static TEnum? TryFromString_Ordinal<TEnum>(string str) where TEnum : struct
        => FromStringCache_Ordinal<TEnum>.Map.TryGetValue(str, out var value) ? value : null;

    private static class FromStringCache_Ordinal<TEnum> where TEnum : struct
    {
        public static readonly Dictionary<string, TEnum> Map;

        static FromStringCache_Ordinal()
        {
            Map = EnumerantAttributes.CreateDictionary<TEnum, EnumerantStringAttribute, string>(
                attribute => attribute.Value, StringComparer.Ordinal);
        }
    }
}
