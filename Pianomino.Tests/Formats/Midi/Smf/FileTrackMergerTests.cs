using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class FileTrackMergerTests
{
    [Fact]
    public static void TestMergeTracks()
    {
        var dataBuilder = new FileModel.BuilderSink();

        {
            var sink = (IFileSink)new TrackMerger(dataBuilder);
            sink.Begin(TrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote);

            sink.BeginTrack();
            sink.AddEvent(0, new Events.TextEvent(MetaEventTypeByte.TrackName, "Hello").ToRaw());
            sink.AddEvent(2, new Events.TextEvent(MetaEventTypeByte.CuePoint, "World").ToRaw());
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

        AssertTicksAndHeaderByte(track.Events[0], 0, EventHeaderByte.Meta);
        AssertTicksAndHeaderByte(track.Events[1], 0, StatusByte.NoteOn_Channel1);
        AssertTicksAndHeaderByte(track.Events[2], 1, StatusByte.NoteOff_Channel1);
        AssertTicksAndHeaderByte(track.Events[3], 2, EventHeaderByte.Meta);
        AssertTicksAndHeaderByte(track.Events[4], 2, StatusByte.NoteOn_Channel2);
        AssertTicksAndHeaderByte(track.Events[5], 3, StatusByte.NoteOff_Channel2);
    }

    private static void AssertTicksAndHeaderByte(FileModel.TrackEvent @event, long ticks, EventHeaderByte headerByte)
    {
        Assert.Equal(ticks, @event.TimeInTicks);
        Assert.Equal(headerByte, @event.Event.HeaderByte);
    }

    private static void AssertTicksAndHeaderByte(FileModel.TrackEvent @event, long ticks, StatusByte headerByte)
        => AssertTicksAndHeaderByte(@event, ticks, (EventHeaderByte)(byte)headerByte);
}
