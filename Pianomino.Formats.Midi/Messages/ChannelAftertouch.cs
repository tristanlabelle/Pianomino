using System;
using System.Text;

namespace Pianomino.Formats.Midi.Messages;

public sealed class ChannelAftertouch : ChannelMessage
{
    public Velocity Pressure { get; }

    public ChannelAftertouch(Channel channel, Velocity pressure)
        : base(channel)
    {
        this.Pressure = pressure;
    }

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status, Pressure);
    public override string ToString() => $"ChannelAftertouch({ChannelString}, {Pressure})";
    protected override ChannelMessageType GetChannelType() => ChannelMessageType.ProgramChange;
}
