using Pianomino.Theory;
using System;

namespace Pianomino.Formats.iReal;

public delegate IChartSink? ChartSinkFactory(SongMetadata header, out bool final);

public interface IChartSink
{
    void AddBarline(Barline barline); // |,[,],{,}
    void AddTimeSignature(TimeSignature value); // T44
    void AddRehearsalMark(RehearsalMark mark); // *A
    void AddSegnoCodaOrFermata(SegnoCodaOrFermata value); // S, Q, f
    void AddEnding(Ending ending); // Zero to mark the end of the ending sequence
    void AddStaffText(StaffText staffText); // <Hello>
    void AddShiftDown(VerticalSpace amount); // Y, YY, YYY
    void AddChordSizeChange(ChordSize size); // s/l
    void AddCellSymbol(CellSymbol? symbol); // ' ', C-^7#9/G, n,p,x,r
    void AddAlternateChordSymbol(ChordSymbol symbol); // (C-^7#9/G)
    void AddEnd(); // U
    void End();
}

public enum Ending : byte
{
    One,
    Two,
    Three,
    Final,
}

public enum RehearsalMark : byte
{
    SectionA,
    SectionB,
    SectionC,
    SectionD,
    Verse,
    Intro
}

public enum SegnoCodaOrFermata : byte
{
    Segno,
    Coda,
    Fermata
}

public enum VerticalSpace : byte
{
    Small,
    Medium,
    Large
}

public enum ChordSize : byte
{
    Large,
    Small
}
