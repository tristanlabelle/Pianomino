using System;
using System.Collections.Immutable;
using System.Linq;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// A MIDI message as appears in a standard MIDI file.
/// Can be a channel event, an escape event or a meta event.
/// </summary>
public readonly struct RawEvent
{
    public static RawEvent EndOfTrack => new(MetaEventTypeByte.EndOfTrack, ImmutableArray<byte>.Empty);

    private readonly ImmutableArray<byte> variableLengthPayload;
    private readonly EventHeaderByte headerByte;
    private readonly MetaEventTypeByte metaTypeByte;
    private readonly ushort firstTwoPayloadBytes;

    private RawEvent(StatusByte status, ShortPayload payload)
    {
        this.variableLengthPayload = payload.byteArray;
        this.headerByte = (EventHeaderByte)(byte)status;
        this.metaTypeByte = 0;
        this.firstTwoPayloadBytes = payload.firstTwoBytes;
    }

    private RawEvent(MetaEventTypeByte metaType, ShortPayload payload)
    {
        this.variableLengthPayload = payload.byteArray;
        this.headerByte = EventHeaderByte.Meta;
        this.metaTypeByte = metaType;
        this.firstTwoPayloadBytes = payload.firstTwoBytes;
    }

    private RawEvent(bool sysExPrefix, ShortPayload payload)
    {
        this.variableLengthPayload = payload.byteArray;
        this.headerByte = sysExPrefix ? EventHeaderByte.Escape_SysEx : EventHeaderByte.Escape;
        this.metaTypeByte = 0;
        this.firstTwoPayloadBytes = payload.firstTwoBytes;
    }

    public EventHeaderByte HeaderByte => headerByte;
    public bool IsChannel => HeaderByte.IsChannelStatus();
    public bool IsEscape => HeaderByte.IsEscape();
    public bool IsMeta => HeaderByte == EventHeaderByte.Meta;

    public ShortPayload Payload => IsChannel
        ? new(((StatusByte)HeaderByte).GetPayloadLengthType(), firstTwoPayloadBytes, variableLengthPayload)
        : new(variableLengthPayload);

    public MetaEventTypeByte GetMetaType() => IsMeta
        ? metaTypeByte : throw new InvalidOperationException();
    public StatusByte GetChannelStatus() => IsChannel
        ? (StatusByte)HeaderByte : throw new InvalidOperationException();

    public RawMessage ToMessage() => IsChannel
        ? new((StatusByte)HeaderByte, variableLengthPayload, firstTwoPayloadBytes)
        : throw new InvalidOperationException();

    public static RawEvent FromMessage(in RawMessage message)
    {
        if (message.IsChannelMessage)
            return new(message.Status, message.Payload);
        else if (message.IsSystemExclusive)
            return new(sysExPrefix: true, message.Payload);
        else
        {
            // Common system and system realtime messages can be packed in an escape event.
            if (message.Payload.Length == 0)
                return new(sysExPrefix: false, new ShortPayload((byte)message.Status));
            else if (message.Payload.Length == 1)
                return new(sysExPrefix: false, new ShortPayload((byte)message.Status, message.Payload.FirstByteOrZero));
            else
                throw new NotImplementedException();
        }
    }

    public static RawEvent CreateChannel(StatusByte status, byte firstPayloadByte, byte secondPayloadByte = 0)
    {
        if (!status.IsChannelMessage()) throw new ArgumentOutOfRangeException(nameof(status));
        if (!RawMessage.IsValidPayloadByte(firstPayloadByte)) throw new ArgumentOutOfRangeException(nameof(firstPayloadByte));
        if (status.GetPayloadLengthType() == ShortPayloadLengthType.TwoBytes && !RawMessage.IsValidPayloadByte(secondPayloadByte))
            throw new ArgumentOutOfRangeException(nameof(secondPayloadByte));

        return new(status, new ShortPayload(firstPayloadByte, secondPayloadByte));
    }

    public static RawEvent CreateEscape(bool sysExPrefix, ImmutableArray<byte> payload)
        => new(sysExPrefix, payload);

    public static RawEvent CreateMeta(MetaEventTypeByte type, ImmutableArray<byte> payload)
        => new(type, payload);
}
