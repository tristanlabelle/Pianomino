using Pianomino.Formats.Midi.Messages;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class EscapeEvent : Event
{
    public bool HasSysExPrefix { get; }
    public ImmutableArray<byte> Payload { get; }

    public EscapeEvent(bool sysExPrefix, ImmutableArray<byte> payload)
    {
        this.HasSysExPrefix = sysExPrefix;
        this.Payload = payload;
    }

    public override EventHeaderByte HeaderByte => HasSysExPrefix ? EventHeaderByte.Escape_SysEx : EventHeaderByte.Escape;
    public override bool IsWireCompatible => true;

    public override Message ToMessage() => throw new NotImplementedException();
    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateEscape(HasSysExPrefix, Payload);
    public override string ToString() => throw new NotImplementedException();
}
