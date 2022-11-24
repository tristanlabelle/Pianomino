using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class ChannelPrefix : MetaMessage
{
    public Channel Channel { get; }

    public ChannelPrefix(Channel channel)
    {
        if (!channel.IsValid()) throw new ArgumentOutOfRangeException(nameof(channel));
        this.Channel = channel;
    }

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(MetaMessageTypeByte.ChannelPrefix, ImmutableArray.Create((byte)Channel));

    public override string ToString() => $"ChannelPrefix(Ch{Channel.ToNumber()})";

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.ChannelPrefix;
}
