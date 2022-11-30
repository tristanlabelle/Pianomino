using Pianomino.Formats.Midi.Messages;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class ChannelEvent : SmfEvent
{
    public ChannelMessage Message { get; }

    public ChannelEvent(ChannelMessage message)
        => this.Message = message;

    public override EventHeaderByte HeaderByte => (EventHeaderByte)(byte)Message.Status;
    public override bool IsWireCompatible => true;

    public override Message ToMessage() => Message;
    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.FromMessage(Message.ToRaw(encoding));
    public override string ToString() => Message.ToString();
}
