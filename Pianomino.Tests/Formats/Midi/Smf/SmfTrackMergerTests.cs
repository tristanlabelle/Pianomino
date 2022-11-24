using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class SmfTrackMergerTests
{
    [Fact]
    public static void TestMergeTracks()
    {
        var dataBuilder = new SmfData.BuilderSink();

        {
            var sink = (ISmfSink)new SmfTrackMerger(dataBuilder);
            sink.Begin(SmfTrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote);

            sink.BeginTrack();
            sink.AddEvent(0, new Messages.TextEvent(MetaMessageTypeByte.TrackName, "Hello").ToRaw());
            sink.AddEvent(2, new Messages.TextEvent(MetaMessageTypeByte.CuePoint, "World").ToRaw());
            sink.EndTrack();

            sink.BeginTrack();
            sink.AddChannelEvent(0, StatusByte.NoteOn_Channel1, NoteKey.C4, 0x60);
            sink.AddChannelEvent(1, StatusByte.NoteOff_Channel1, NoteKey.C4, 0);
            sink.AddChannelEvent(1, StatusByte.NoteOn_Channel2, NoteKey.C4 + 2, 0x60);
            sink.AddChannelEvent(1, StatusByte.NoteOff_Channel2, NoteKey.C4 + 2, 0);
            sink.EndTrack();

            sink.End();
        }

        var data = dataBuilder.Build();
        Assert.False(data.AreTracksIndependent);
        Assert.Equal(TimeDivision.OneTickPerQuarterNote, data.TimeDivision);

        var track = Assert.Single(data.Tracks);
        Assert.Equal(6, track.Events.Length);

        AssertTicksStatus(track.Events[0], 0, SmfStatusBytes.Meta);
        AssertTicksStatus(track.Events[1], 0, StatusByte.NoteOn_Channel1);
        AssertTicksStatus(track.Events[2], 1, StatusByte.NoteOff_Channel1);
        AssertTicksStatus(track.Events[3], 2, SmfStatusBytes.Meta);
        AssertTicksStatus(track.Events[4], 2, StatusByte.NoteOn_Channel2);
        AssertTicksStatus(track.Events[5], 3, StatusByte.NoteOff_Channel2);
    }

    private static void AssertTicksStatus(SmfData.Event @event, long expectedTicks, StatusByte expectedStatus)
    {
        Assert.Equal(expectedTicks, @event.TimeInTicks);
        Assert.Equal(expectedStatus, (StatusByte)@event.Message.Status);
    }
}
