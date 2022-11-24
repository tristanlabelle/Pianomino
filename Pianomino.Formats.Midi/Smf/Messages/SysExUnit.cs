using Pianomino.Formats.Midi.Messages;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SysExUnit : SmfMessage
{
    public SysExMessage WireMessage { get; }

    public SysExUnit(SysExMessage wireMessage)
        => this.WireMessage = wireMessage;

    public override byte Status => (byte)StatusByte.SystemExclusive;
    public override bool IsWireCompatible => true;

    public override Message ToWireMessage() => WireMessage;
    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.FromWireMessage(WireMessage.ToRaw(encoding));
    public override string ToString() => WireMessage.ToString();

    protected override SmfMessageType GetMessageType() => SmfMessageType.SysEx_Unit;
}
