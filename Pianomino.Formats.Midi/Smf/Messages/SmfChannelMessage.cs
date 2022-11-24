using Pianomino.Formats.Midi.Messages;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SmfChannelMessage : SmfMessage
{
    public ChannelMessage WireMessage { get; }

    public SmfChannelMessage(ChannelMessage wireMessage)
        => this.WireMessage = wireMessage;

    public override byte Status => (byte)WireMessage.Status;
    public override bool IsWireCompatible => true;

    public override Message ToWireMessage() => WireMessage;
    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.FromWireMessage(WireMessage.ToRaw(encoding));
    public override string ToString() => WireMessage.ToString();

    protected override SmfMessageType GetMessageType() => SmfMessageType.Channel;
}
