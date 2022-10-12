using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Pianomino.Theory;

public static class NoteValueTests
{
    [Fact]
    public static void TestDuration()
    {
        Assert.Equal(NoteUnit.Eighth.Duration, NoteValue.Eighth.Duration);
        Assert.Equal(NoteUnit.Eighth.Duration * 3, NoteUnit.Quarter.ToNoteValue(dotCount: 1).Duration);
    }

    [Fact]
    public static void TestDurationSplitting()
    {
        Assert.Equal(NoteValue.Quarter, NoteValue.SplitDuration(NoteValue.Quarter.Duration).Single());
        Assert.Equal(NoteValue.Eighth, NoteValue.SplitDuration(NoteValue.Eighth.Duration).Single());

        var dottedHalfNote = NoteUnit.Half.ToNoteValue(dotCount: 1);
        Assert.Equal(dottedHalfNote, NoteValue.SplitDuration(dottedHalfNote.Duration).Single());

        Assert.Equal(new[] { NoteValue.Half, NoteUnit.Eighth.ToNoteValue(dotCount: 1) },
            NoteValue.SplitDuration(NoteUnit.Sixteenth.Duration * 11));
    }
}
