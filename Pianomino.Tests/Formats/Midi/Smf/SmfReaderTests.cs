using System;
using System.Collections.Immutable;
using System.IO;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class SmfReaderTests
{
    [Fact]
    public static void TestReadStandardExampleFileFormat0()
    {
        var reader = new SmfReader(new MemoryStream(new byte[]
        {
            0x4D, 0x54, 0x68, 0x64, // MThd
            0x00, 0x00, 0x00, 0x06, // chunk length
            0x00, 0x00, // format 0
            0x00, 0x01, // one track
            0x00, 0x60, // 96 per quarter-note
            0x4D, 0x54, 0x72, 0x6B, // MTrk
            0x00, 0x00, 0x00, 0x3B, // chunk length (59)
            0x00, 0xFF, 0x58, 0x04, 0x04, 0x02, 0x18, 0x08, // time signature
            0x00, 0xFF, 0x51, 0x03, 0x07, 0xA1, 0x20, // tempo
            0x00, 0xC0, 0x05,
            0x00, 0xC1, 0x2E,
            0x00, 0xC2, 0x46,
            0x00, 0x92, 0x30, 0x60,
            0x00, 0x3C, 0x60, // running status
            0x60, 0x91, 0x43, 0x40,
            0x60, 0x90, 0x4C, 0x20,
            0x81, 0x40, 0x82, 0x30, 0x40, // two-byte, delta-time
            0x00, 0x3C, 0x40, // running status
            0x00, 0x81, 0x43, 0x40,
            0x00, 0x80, 0x4C, 0x40,
            0x00, 0xFF, 0x2F, 0x00 // end of track
        }), allowRiffHeader: false, SmfReader.Strict);

        Assert.Equal(SmfReaderState.PostHeader, reader.State);
        Assert.False(reader.IsRmidFile);
        Assert.Equal(SmfTrackFormat.Single, reader.TrackFormat);
        Assert.Equal(1, reader.TrackCount);
        Assert.Equal(TimeDivisionFormat.TicksPerQuarterNote, reader.TimeDivision.Format);
        Assert.Equal(96, reader.TimeDivision.TicksPerQuarterNote);

        Assert.Equal(SmfReaderState.StartOfTrack, reader.Read());

        (int DeltaTime, RawEvent Event)[] expectedEvents = new (int DeltaTime, RawEvent Event)[]
        {
            (0x00, RawEvent.CreateMeta(MetaEventTypeByte.TimeSignature, new byte[] { 0x04, 0x02, 0x18, 0x08 }.ToImmutableArray())),
            (0x00, RawEvent.CreateMeta(MetaEventTypeByte.SetTempo, new byte[] { 0x07, 0xA1, 0x20 }.ToImmutableArray())),
            (0x00, RawEvent.CreateChannel(StatusByte.ProgramChange_Channel1, (byte)GeneralMidiProgram.ElectricPiano2)),
            (0x00, RawEvent.CreateChannel(StatusByte.ProgramChange_Channel2, (byte)GeneralMidiProgram.OrchestralHarp)),
            (0x00, RawEvent.CreateChannel(StatusByte.ProgramChange_Channel3, (byte)GeneralMidiProgram.Bassoon)),
            (0x00, RawEvent.CreateChannel(StatusByte.NoteOn_Channel3, 0x30, 0x60)),
            (0x00, RawEvent.CreateChannel(StatusByte.NoteOn_Channel3, 0x3C, 0x60)),
            (0x60, RawEvent.CreateChannel(StatusByte.NoteOn_Channel2, 0x43, 0x40)),
            (0x60, RawEvent.CreateChannel(StatusByte.NoteOn_Channel1, 0x4C, 0x20)),
            (0xC0, RawEvent.CreateChannel(StatusByte.NoteOff_Channel3, 0x30, 0x40)),
            (0x00, RawEvent.CreateChannel(StatusByte.NoteOff_Channel3, 0x3C, 0x40)),
            (0x00, RawEvent.CreateChannel(StatusByte.NoteOff_Channel2, 0x43, 0x40)),
            (0x00, RawEvent.CreateChannel(StatusByte.NoteOff_Channel1, 0x4C, 0x40)),
            (0x00, RawEvent.CreateMeta(MetaEventTypeByte.EndOfTrack, ImmutableArray<byte>.Empty))
        };

        foreach (var expectedEvent in expectedEvents)
        {
            Assert.Equal(SmfReaderState.Event, reader.Read());
            var actualEvent = reader.GetEvent();
            Assert.Equal(expectedEvent.DeltaTime, reader.GetEventTimeDelta());
            Assert.Equal(expectedEvent.Event.HeaderByte, actualEvent.HeaderByte);
            Assert.Equal(expectedEvent.Event.Payload.Length, actualEvent.Payload.Length);
        }

        Assert.Equal(SmfReaderState.EndOfTrack, reader.Read());
        Assert.Equal(SmfReaderState.EndOfFile, reader.Read());
    }

    [Fact]
    public static void TestWithRmidHeader()
    {
        var data = new byte[]
        {
                (byte)'R', (byte)'I', (byte)'F', (byte)'F',
                0x00, 0x00, 0x00, 0x1A, // Length (26)
				(byte)'R', (byte)'M', (byte)'I', (byte)'D',
                (byte)'M', (byte)'T', (byte)'h', (byte)'d',
                0x00, 0x00, 0x00, 0x06, // chunk length
				0x00, 0x00, // format 0
				0x00, 0x01, // one track
				0x00, 0x60, // 96 per quarter-note
				(byte)'M', (byte)'T', (byte)'r', (byte)'k',
                0x00, 0x00, 0x00, 0x04, // chunk length (4)
				0x00, 0xFF, 0x2F, 0x00 // end of track
        };

        var reader = new SmfReader(new MemoryStream(data), allowRiffHeader: true, SmfReader.Strict);
        Assert.True(reader.IsRmidFile);
        Assert.Equal(SmfTrackFormat.Single, reader.TrackFormat);
        Assert.Equal(1, reader.TrackCount);
    }
}
