using System;
using System.Collections.Generic;

namespace Pianomino.Formats.iReal;

public enum KnownStyle : byte
{
    [KnownStyle("Afoxe", 100)] Afoxe,
    [KnownStyle("Afro", 110)] Afro,
    [KnownStyle("Baião", 100)] Baiao,
    [KnownStyle("Ballad", 60)] Ballad,
    [KnownStyle("Bossa Nova", 140)] BossaNova,
    [KnownStyle("Calypso", 100)] Calypso,
    [KnownStyle("Chacarera", 100)] Chacarera,
    [KnownStyle("Even8ths", 140)] Even8ths,
    [KnownStyle("Funk", 140)] Funk,
    [KnownStyle("Latin", 180)] Latin,
    [KnownStyle("Latin-Swing", 180)] LatinSwing,
    [KnownStyle("Medium Swing", 100)] MediumSwing,
    [KnownStyle("Medium Up Swing", 160)] MediumUpSwing,
    [KnownStyle("Pop", 115)] Pop,
    [KnownStyle("Pop Ballad", 60)] PopBallad,
    [KnownStyle("Reggae", 90)] Reggae,
    [KnownStyle("RnB", 95)] RnB,
    [KnownStyle("Rock", 115)] Rock,
    [KnownStyle("Rock Pop", 115)] RockPop,
    [KnownStyle("Samba", 200)] Samba,
    [KnownStyle("Samba Funk", 200)] SambaFunk,
    [KnownStyle("Shuffle", 130)] Shuffle,
    [KnownStyle("Slow Bossa", 140)] SlowBossa,
    [KnownStyle("Slow Swing", 80)] SlowSwing,
    [KnownStyle("Up Tempo Swing", 240)] UpTempoSwing,
    [KnownStyle("Waltz", 100)] Waltz
}

public static class KnownStyleEnum
{
    public static KnownStyle? TryFromText(string text) => Cache.TextToValue.GetValueOrDefault(text);

    public static string GetText(this KnownStyle style) => GetAttribute(style).Text;

    public static int GetDefaultTempo(this KnownStyle style) => GetAttribute(style).DefaultTempo;

    private static KnownStyleAttribute GetAttribute(KnownStyle style)
         => EnumerantAttributes.TryGetSingle<KnownStyle, KnownStyleAttribute>(style) ?? throw new ArgumentOutOfRangeException(nameof(style));

    private static class Cache
    {
        public static readonly Dictionary<string, KnownStyle> TextToValue;

        static Cache()
        {
            TextToValue = EnumerantAttributes.CreateDictionary<KnownStyle, KnownStyleAttribute, string>(
                attribute => attribute.Text, StringComparer.Ordinal);
        }
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class KnownStyleAttribute : Attribute
{
    public string Text { get; }
    public int DefaultTempo { get; }
    
    public KnownStyleAttribute(string text, int defaultTempo)
    {
        this.Text = text;
        this.DefaultTempo = defaultTempo;
    }
}
