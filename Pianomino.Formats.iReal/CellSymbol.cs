using Pianomino.Theory;
using System;
using System.Text;

namespace Pianomino.Formats.iReal;

public class CellSymbol
{
    public static CellSymbol NoChord { get; } = new();
    public static CellSymbol ChordRepeatSlash { get; } = new();
    public static CellSymbol SingleMeasureRepeat { get; } = new();
    public static CellSymbol DoubleMeasureRepeat { get; } = new();

    private protected CellSymbol() { }

    public override string ToString()
    {
        if (this == NoChord) return "N.C.";
        if (this == ChordRepeatSlash) return "/";
        if (this == SingleMeasureRepeat) return "%";
        if (this == DoubleMeasureRepeat) return "‰";
        throw new NotImplementedException();
    }
}

public sealed class ChordSymbol : CellSymbol
{
    public NoteClass? Root { get; }
    public ChordQuality Quality { get; }
    public NoteClass? Bass { get; }

    public ChordSymbol(NoteClass root, ChordQuality quality, NoteClass? bass = null)
    {
        this.Root = root;
        this.Quality = quality;
        this.Bass = bass;
    }

    private readonly struct BassChangeTag { }

    private ChordSymbol(NoteClass bass, BassChangeTag _)
    {
        this.Root = null;
        this.Quality = default;
        this.Bass = Bass;
    }

    public static ChordSymbol BassChange(NoteClass bass) => new(bass, default(BassChangeTag));

    public bool IsBassChange => !Root.HasValue;

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        if (Root is NoteClass root)
        {
            stringBuilder.Append(root);
            Quality.AppendString(stringBuilder);
        }

        if (Bass is NoteClass bass)
            stringBuilder.Append('/').Append(bass);

        return stringBuilder.ToString();
    }
}
