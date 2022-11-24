using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class SmfWriterTests
{
    [Fact]
    public static void TestAutoEndOfTrackEvent()
    {
        var stream = new MemoryStream();

        using (var writer = new SmfWriter(stream, transferOwnership: false, SmfTrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote))
        {
            writer.BeginTrack();
            writer.EndTrack(); // Should be automatically added

            writer.BeginTrack();
            writer.WriteMeta(0, MetaMessageTypeByte.EndOfTrack, ReadOnlySpan<byte>.Empty);
            writer.EndTrack(); // Should not be added
        }

        stream.Position = 0;
        var reader = new SmfReader(stream, validationErrorHandler: SmfReader.Strict);

        for (int i = 0; i < 2; ++i)
        {
            Assert.Equal(SmfReaderState.StartOfTrack, reader.Read());
            Assert.Equal(SmfReaderState.Event, reader.Read());
            Assert.Equal(MetaMessageTypeByte.EndOfTrack, reader.GetEventMessage().GetMetaType());
            Assert.Equal(SmfReaderState.EndOfTrack, reader.Read());
        }

        Assert.Equal(SmfReaderState.EndOfFile, reader.Read());
    }

    [Fact]
    public static void TestWrittenFilePassesValidation()
    {
        var stream = new MemoryStream();

        using (var writer = new SmfWriter(stream, transferOwnership: false, SmfTrackFormat.Simultaneous, TimeDivision.OneTickPerQuarterNote))
        {
            writer.BeginTrack();
            writer.Write(0, new Messages.SequenceNumber(1).ToRaw());
            writer.EndTrack();

            writer.BeginTrack();
            writer.WriteChannel(0, StatusByte.NoteOn_Channel1, NoteKey.C4, Velocity.MezzoForte);
            writer.WriteChannel(1, StatusByte.NoteOff_Channel1, NoteKey.C4, Velocity.Zero);
            writer.EndTrack();
        }

        stream.Position = 0;
        SmfReader.Read(stream, validationErrorHandler: SmfReader.Strict);
    }
}
