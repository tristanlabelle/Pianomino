using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public abstract class ChannelMessage : Message
{
    private protected ChannelMessage(Channel channel)
    {
        this.Channel = channel;
    }

    public override StatusByte Status => Type.GetStatusByte(Channel);
    public ChannelMessageType Type => GetChannelType();
    public Channel Channel { get; }
    protected string ChannelString => "Ch" + Channel.ToNumber();

    protected abstract ChannelMessageType GetChannelType();
}
