using System;

namespace Pianomino.Formats.iReal;

public static class PseudoUriFormat
{
    public const string LegacyScheme = "irealbook";
    public const string LegacyPrefix = "irealbook://";
    public const string ProScheme = "irealb";
    public const string ProPrefix = "irealb://";

    public static bool Test(string str) => str.StartsWith(LegacyPrefix) || str.StartsWith(ProPrefix);
}
