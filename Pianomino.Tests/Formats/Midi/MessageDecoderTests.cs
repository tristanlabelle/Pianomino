using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pianomino.Formats.Midi;

public static class MessageDecoderTests
{
    [Fact]
    public static void TestSingleNoteOn()
    {
        const StatusByte status = StatusByte.NoteOn_Channel1;
        const byte note = 60; // Middle C
        const byte velocity = 0x7F; // Maximum

        var decoder = new MessageDecoder();

        FeedNoOutput(decoder, (byte)status);
        FeedNoOutput(decoder, note);
        var message = FeedNoWarning(decoder, velocity)!.Value;

        Assert.Equal(status, message.Status);
        Assert.Equal(2, message.Payload.Length);
        Assert.Equal(note, message.Payload[0]);
        Assert.Equal(velocity, message.Payload[1]);
    }

    [Fact]
    public static void TestSystemExclusiveMessage()
    {
        var decoder = new MessageDecoder();
        FeedNoOutput(decoder, (byte)StatusByte.SystemExclusive);
        FeedNoOutput(decoder, (byte)0);
        FeedNoOutput(decoder, (byte)1);
        FeedNoOutput(decoder, (byte)2);
        FeedNoOutput(decoder, (byte)3);
        var message = FeedNoWarning(decoder, (byte)StatusByte.EndOfExclusive)!.Value;

        Assert.Equal(StatusByte.SystemExclusive, message.Status);
        Assert.Equal(4, message.Payload.Length);
        Assert.Equal(0, message.Payload[0]);
        Assert.Equal(1, message.Payload[1]);
        Assert.Equal(2, message.Payload[2]);
        Assert.Equal(3, message.Payload[3]);
    }

    [Fact]
    public static void TestRunningStatus()
    {
        const StatusByte status = StatusByte.NoteOn_Channel1;
        const byte note = 60; // Middle C
        const byte velocity = 0x7F; // Maximum

        var decoder = new MessageDecoder();
        Assert.Null(decoder.RunningStatus);

        FeedNoOutput(decoder, (byte)status);
        Assert.Equal(status, decoder.RunningStatus);
        FeedNoOutput(decoder, note);
        Assert.Equal(status, FeedNoWarning(decoder, velocity)!.Value.Status);

        FeedNoOutput(decoder, note);
        var message = FeedNoWarning(decoder, 0)!.Value;
        Assert.Equal(status, message.Status);
        Assert.Equal(note, message.Payload[0]);
        Assert.Equal(0, message.Payload[1]);
    }

    [Fact]
    public static void TestRunningStatusReset()
    {
        var decoder = new MessageDecoder();
        Assert.Null(decoder.RunningStatus);
        FeedNoOutput(decoder, (byte)StatusByte.ProgramChange_Channel1);
        Assert.Equal(StatusByte.ProgramChange_Channel1, decoder.RunningStatus);
        Assert.NotNull(FeedNoWarning(decoder, 0));
        Assert.Equal(StatusByte.ProgramChange_Channel1, decoder.RunningStatus);

        Assert.NotNull(FeedNoWarning(decoder, (byte)StatusByte.TuneRequest));
        Assert.Null(decoder.RunningStatus);
    }

    [Fact]
    public static void TestInterleavedSystemRealTimeMessage()
    {
        var decoder = new MessageDecoder();
        FeedNoOutput(decoder, (byte)StatusByte.ProgramChange_Channel1);
        Assert.Equal(StatusByte.Clock, FeedNoWarning(decoder, (byte)StatusByte.Clock)!.Value.Status);
        Assert.Equal(StatusByte.ProgramChange_Channel1, FeedNoWarning(decoder, 0)!.Value.Status);
    }

    private static void FeedNoOutput(MessageDecoder decoder, byte @byte)
    {
        var message = decoder.Feed(@byte, out var warning);
        Assert.Null(message);
        Assert.Null(warning);
    }

    private static RawMessage? FeedNoWarning(MessageDecoder decoder, byte @byte)
    {
        var message = decoder.Feed(@byte, out var warning);
        Assert.Null(warning);
        return message;
    }
}
