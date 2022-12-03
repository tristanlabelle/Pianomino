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
    public static void TestInitialState()
    {
        var decoder = new MessageDecoder();
        Assert.False(decoder.IsPartial);
        Assert.Equal(0, decoder.DecodedCount);
        Assert.Null(decoder.RunningStatus);
    }

    [Fact]
    public static void TestSingleNoteOn()
    {
        const StatusByte status = StatusByte.NoteOn_Channel1;
        const byte note = 60; // Middle C
        const byte velocity = 0x7F; // Maximum

        var decoder = CreateStrictDecoder();

        decoder.Feed((byte)status);
        Assert.Equal(decoder.RunningStatus, status);
        decoder.Feed(note);
        Assert.Equal(0, decoder.DecodedCount);
        decoder.Feed(velocity);
        Assert.Equal(1, decoder.DecodedCount);
        var message = decoder.Dequeue();
        Assert.Equal(0, decoder.DecodedCount);
        Assert.Equal(decoder.RunningStatus, status);

        Assert.Equal(status, message.Status);
        Assert.Equal(2, message.Payload.Length);
        Assert.Equal(note, message.Payload[0]);
        Assert.Equal(velocity, message.Payload[1]);
    }

    [Fact]
    public static void TestSimpleSystemExclusiveMessage()
    {
        var decoder = CreateStrictDecoder();
        decoder.Feed((byte)StatusByte.SystemExclusive);
        decoder.Feed((byte)0);
        decoder.Feed(1);
        decoder.Feed(2);
        decoder.Feed(3);
        decoder.Feed((byte)StatusByte.EndOfExclusive);
        var message = decoder.Dequeue();

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
        var decoder = new MessageDecoder();
        Assert.Null(decoder.RunningStatus);
        decoder.Feed(StatusByte.NoteOn_Channel1);
        Assert.Equal(StatusByte.NoteOn_Channel1, decoder.RunningStatus);
        decoder.Feed(NoteKey.C4.Number);
        decoder.Feed((byte)Velocity.MaxValue);

        var fullMessage = decoder.Dequeue();
        Assert.Equal(StatusByte.NoteOn_Channel1, fullMessage.Status);

        decoder.Feed(NoteKey.C4.Number);
        decoder.Feed((byte)Velocity.Off);
        var runningStatusMessage = decoder.Dequeue();
        Assert.Equal(StatusByte.NoteOn_Channel1, runningStatusMessage.Status);
        Assert.Equal(NoteKey.C4.Number, runningStatusMessage.Payload[0]);
        Assert.Equal((byte)Velocity.Off, runningStatusMessage.Payload[1]);
        Assert.Equal(StatusByte.NoteOn_Channel1, decoder.RunningStatus);
    }

    [Fact]
    public static void TestRunningStatusReset()
    {
        var decoder = CreateStrictDecoder();
        Assert.Null(decoder.RunningStatus);
        decoder.Feed(StatusByte.ProgramChange_Channel1);
        Assert.Equal(StatusByte.ProgramChange_Channel1, decoder.RunningStatus);
        decoder.Feed((byte)GeneralMidiProgram.AcousticGrandPiano);
        Assert.Equal(StatusByte.ProgramChange_Channel1, decoder.RunningStatus);
        var programChangeMessage = decoder.Dequeue();

        Assert.Equal(StatusByte.ProgramChange_Channel1, decoder.RunningStatus);
        decoder.Feed(StatusByte.TuneRequest);
        Assert.Null(decoder.RunningStatus);
    }

    [Fact]
    public static void TestInterleavedSystemRealTimeMessage()
    {
        var decoder = CreateStrictDecoder();
        decoder.Feed(StatusByte.ProgramChange_Channel1);
        decoder.Feed(StatusByte.Clock);
        var clockMessage = decoder.Dequeue();
        Assert.Equal(StatusByte.Clock, clockMessage.Status);
        decoder.Feed((byte)GeneralMidiProgram.AcousticGrandPiano);
        var programChangeMessage = decoder.Dequeue();
        Assert.Equal(StatusByte.ProgramChange_Channel1, programChangeMessage.Status);
    }

    private static MessageDecoder CreateStrictDecoder()
    {
        var decoder = new MessageDecoder();
        decoder.ProblemEncountered += problem => throw new UnreachableException();
        return decoder;
    }
}
