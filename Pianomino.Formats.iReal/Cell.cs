using System;

namespace Pianomino.Formats.iReal;

public sealed class Cell
{
    [Flags]
    private enum FieldBits : ushort
    {
        None = 0,
        TimeSignature = 1 << 0,
        StaffText = 1 << 1,
        ShiftDown = 1 << 2,
        ChordSizeChange = 1 << 3,
        RehearsalMark = 1 << 4,
        SegnoCodaOrFermata = 1 << 5,
        StartBarline = 1 << 6,
        EndBarline = 1 << 7,
        EndingNumber = 1 << 8,
        Ending = 1 << 9,
        IsEnd = 1 << 10,
    }

    public CellSymbol? Symbol { get; set; }
    private ChordSymbol? alternateChordSymbol;
    private StaffText staffText;
    private TimeSignature timeSignature;
    private VerticalSpace shiftDown;
    private ChordSize chordSizeChange;
    private RehearsalMark rehearsalMark;
    private SegnoCodaOrFermata segnoCodaOrFermata;
    private Barline startBarline;
    private Barline endBarline;
    private Ending ending;
    private FieldBits fieldBits;

    public ChordSymbol? AlternateChordSymbol
    {
        get => alternateChordSymbol;
        set => alternateChordSymbol = value;
    }

    public StaffText? StaffText
    {
        get => Get(staffText, FieldBits.StaffText);
        set
        {
            if (value is not null && value.Value.Text is null)
                throw new ArgumentNullException(nameof(StaffText));
            Set(ref staffText, value, FieldBits.StaffText);
        }
    }

    public TimeSignature? TimeSignature
    {
        get => Get(timeSignature, FieldBits.TimeSignature);
        set => Set(ref timeSignature, value, FieldBits.TimeSignature);
    }

    public VerticalSpace? ShiftDown
    {
        get => Get(shiftDown, FieldBits.ShiftDown);
        set => Set(ref shiftDown, value, FieldBits.ShiftDown);
    }

    public ChordSize? ChordSizeChange
    {
        get => Get(chordSizeChange, FieldBits.ChordSizeChange);
        set => Set(ref chordSizeChange, value, FieldBits.ChordSizeChange);
    }

    public RehearsalMark? RehearsalMark
    {
        get => Get(rehearsalMark, FieldBits.RehearsalMark);
        set => Set(ref rehearsalMark, value, FieldBits.RehearsalMark);
    }

    public SegnoCodaOrFermata? SegnoCodaOrFermata
    {
        get => Get(segnoCodaOrFermata, FieldBits.SegnoCodaOrFermata);
        set => Set(ref segnoCodaOrFermata, value, FieldBits.SegnoCodaOrFermata);
    }

    public Barline? StartBarline
    {
        get => Get(startBarline, FieldBits.StartBarline);
        set
        {
            if (value is Barline.ClosingDouble or Barline.ClosingRepeat or Barline.Final)
                throw new ArgumentOutOfRangeException(nameof(StartBarline));
            Set(ref startBarline, value, FieldBits.StartBarline);
        }
    }

    public Barline? EndBarline
    {
        get => Get(endBarline, FieldBits.EndBarline);
        set
        {
            if (value is Barline.OpeningDouble or Barline.OpeningRepeat)
                throw new ArgumentOutOfRangeException(nameof(StartBarline));
            Set(ref endBarline, value, FieldBits.EndBarline);
        }
    }

    public Ending? Ending
    {
        get => Get(ending, FieldBits.Ending);
        set => Set(ref ending, value, FieldBits.Ending);
    }

    public bool IsEnd
    {
        get => (fieldBits & FieldBits.IsEnd) == FieldBits.IsEnd;
        set => fieldBits = value ? (fieldBits | FieldBits.IsEnd) : (fieldBits & ~FieldBits.IsEnd);
    }

    private T? Get<T>(T value, FieldBits field) where T : struct
    {
        return (fieldBits & field) == field ? value : null;
    }

    private void Set<T>(ref T target, T? value, FieldBits field) where T : struct
    {
        target = value.GetValueOrDefault();
        fieldBits = value.HasValue ? (fieldBits | field) : (fieldBits & ~field);
    }

    public override string ToString() => Symbol?.ToString() ?? " ";
}
