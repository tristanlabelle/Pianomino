using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class Handshake : UniversalSysExMessage
{
    public override UniversalSysExKind Kind { get; }
    public byte PacketNumber { get; }

    public Handshake(UniversalSysExKind kind, byte deviceID, byte packetNumber)
        : base(deviceID)
    {
        if (kind != UniversalSysExKind.Handshaking_Ack
            && kind != UniversalSysExKind.Handshaking_Cancel
            && kind != UniversalSysExKind.Handshaking_Nak
            && kind != UniversalSysExKind.Handshaking_Wait
            && kind != UniversalSysExKind.Handshaking_EndOfFile)
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!RawMessage.IsValidPayloadByte(packetNumber))
            throw new ArgumentOutOfRangeException(nameof(packetNumber));

        this.Kind = kind;
        this.PacketNumber = packetNumber;
    }

    public override byte SubId2 => PacketNumber;

    public override RawMessage ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString() => "Handshake";
}
