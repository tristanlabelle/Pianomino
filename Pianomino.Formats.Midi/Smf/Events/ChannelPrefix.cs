using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class ChannelPrefix : MetaEvent
{
    public Channel Channel { get; }
    public override MetaEventTypeByte MetaType => MetaEventTypeByte.ChannelPrefix;

    public ChannelPrefix(Channel channel)
    {
        if (!channel.IsValid()) throw new ArgumentOutOfRangeException(nameof(channel));
        this.Channel = channel;
    }

    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateMeta(MetaEventTypeByte.ChannelPrefix, ImmutableArray.Create((byte)Channel));

    public override string ToString() => $"ChannelPrefix(Ch{Channel.ToNumber()})";
}
