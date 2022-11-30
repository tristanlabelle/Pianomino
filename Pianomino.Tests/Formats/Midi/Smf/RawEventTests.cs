using System;
using System.Collections.Immutable;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class RawEventTests
{
    [Fact]
    public static void TestRoundTripFromChannelMessage()
    {
        var original = RawMessage.Create(ChannelMessageType.NoteOn, Channel._3, 42, 57);
        var @event = RawEvent.FromMessage(original);
        var roundtripped = @event.ToMessage();
        Assert.Equal(original.Status, roundtripped.Status);
        Assert.Equal(original.Payload[0], roundtripped.Payload[0]);
        Assert.Equal(original.Payload[1], roundtripped.Payload[1]);
    }

    [Fact]
    public static void TestFromSysExMessage()
    {
        var message = RawMessage.CreateSysEx(new byte[] { 0x42 }.ToImmutableArray());
        var @event = RawEvent.FromMessage(message);
        Assert.Equal(EventHeaderByte.Escape_SysEx, @event.HeaderByte);
        Assert.Equal(message.Payload.ToImmutableArray(), @event.Payload.ToImmutableArray());
    }
}
