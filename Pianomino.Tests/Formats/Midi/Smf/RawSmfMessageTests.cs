using System;
using System.Collections.Immutable;
using Xunit;

namespace Pianomino.Formats.Midi.Smf;

public static class RawSmfMessageTests
{
    [Fact]
    public static void TestRoundTripFromWireChannelMessage()
    {
        var original = RawMessage.Create(ChannelMessageType.NoteOn, Channel._3, 42, 57);
        var fileMessage = RawSmfMessage.FromWireMessage(original);
        var roundtripped = fileMessage.ToWireMessage();
        Assert.Equal(original.Status, roundtripped.Status);
        Assert.Equal(original.Data[0], roundtripped.Data[0]);
        Assert.Equal(original.Data[1], roundtripped.Data[1]);
    }

    [Fact]
    public static void TestRoundTripFromWireSysExMessage()
    {
        var original = RawMessage.CreateSysEx(new byte[] { 0x42 }.ToImmutableArray());
        var fileMessage = RawSmfMessage.FromWireMessage(original);
        var roundtripped = fileMessage.ToWireMessage();
        Assert.Equal(original.Status, roundtripped.Status);
        Assert.Equal(original.Data.Length, roundtripped.Data.Length);
        Assert.Equal(original.Data[0], roundtripped.Data[0]);
    }
}
