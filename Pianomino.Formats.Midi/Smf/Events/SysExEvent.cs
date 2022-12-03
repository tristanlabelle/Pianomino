using Pianomino.Formats.Midi.Messages;
using System;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class SysExEvent : Event
{
    public SysExMessage Message { get; }

    public SysExEvent(SysExMessage message) => this.Message = message;

    public override EventHeaderByte HeaderByte => EventHeaderByte.Escape_SysEx;
    public override bool IsWireCompatible => true;

    public override Message ToMessage() => Message;
    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateEscape(sysExPrefix: true, Message.ToRaw(encoding).Payload);
    public override string ToString() => Message.ToString();
}
