using System;

namespace Pianomino.Formats.iReal;

public readonly struct StaffText
{
    public const byte MaxHeight = 76;
    public const byte BelowHeight = 0;
    public const byte AboveHeight = 76;

    public string Text { get; }
    public byte? Height { get; }

    public StaffText(string text, byte? height)
    {
        if (height > MaxHeight) throw new ArgumentOutOfRangeException(nameof(height));
        this.Text = text;
        this.Height = height;
    }

    public override string ToString() => Text;
}
