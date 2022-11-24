using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class ChannelMode : ChannelMessage
{
    public ChannelModeOperation Operation { get; }
    public byte Argument { get; }

    public ChannelMode(Channel channel, ChannelModeOperation operation, byte argument = 0)
        : base(channel)
    {
        this.Operation = operation;
        this.Argument = argument;
    }

    protected override ChannelMessageType GetChannelType() => ChannelMessageType.ControlChangeOrMode;

    public override RawMessage ToRaw(Encoding encoding)
        => RawMessage.Create(Status, Operation.ToByte(), Argument);

    public override string ToString() => $"ChannelMode({Operation})";
}
