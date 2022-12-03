using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class FileWriterTests
{
    [Fact]
    public static void TestAutoEndOfTrackEvent()
    {
        var stream = new MemoryStream();

        using (var writer = new FileWriter(stream, transferOwnership: false, TrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote))
        {
            writer.BeginTrack();
            writer.EndTrack(); // Should be automatically added

            writer.BeginTrack();
            writer.WriteMeta(0, MetaEventTypeByte.EndOfTrack, ReadOnlySpan<byte>.Empty);
            writer.EndTrack(); // Should not be added
        }

        stream.Position = 0;
        var reader = new FileReader(stream, validationErrorHandler: FileReader.Strict);

        for (int i = 0; i < 2; ++i)
        {
            Assert.Equal(FileReaderState.StartOfTrack, reader.Read());
            Assert.Equal(FileReaderState.Event, reader.Read());
            Assert.Equal(MetaEventTypeByte.EndOfTrack, reader.GetEvent().GetMetaType());
            Assert.Equal(FileReaderState.EndOfTrack, reader.Read());
        }

        Assert.Equal(FileReaderState.EndOfFile, reader.Read());
    }

    [Fact]
    public static void TestWrittenFilePassesValidation()
    {
        var stream = new MemoryStream();

        using (var writer = new FileWriter(stream, transferOwnership: false, TrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote))
        {
            writer.BeginTrack();
            writer.Write(0, new Events.SequenceNumber(1).ToRaw());
            writer.EndTrack();

            writer.BeginTrack();
            writer.WriteChannel(0, StatusByte.NoteOn_Channel1, NoteKey.C4, Velocity.MezzoForte);
            writer.WriteChannel(1, StatusByte.NoteOff_Channel1, NoteKey.C4, Velocity.Zero);
            writer.EndTrack();
        }

        stream.Position = 0;
        FileReader.Read(stream, validationErrorHandler: FileReader.Strict);
    }
}
