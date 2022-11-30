using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

/// <summary>
/// Base class for immutable MIDI messages.
/// </summary>
public abstract class Message
{
    private protected Message() { }

    public abstract StatusByte Status { get; }

    public abstract RawMessage ToRaw(Encoding encoding);

    public override abstract string ToString();

    public static Message Create(in RawMessage message,
        SysExMessageFactory sysexFactory,
        Encoding encoding)
    {
        if (message.Status.AsChannelMessage(out var channelMessageType, out var channel))
        {
            return channelMessageType switch
            {
                ChannelMessageType.NoteOff => new NoteMessage(NoteMessageType.Off, channel, (NoteKey)message.Payload[0], (Velocity)message.Payload[1]),
                ChannelMessageType.NoteOn => new NoteMessage(NoteMessageType.On, channel, (NoteKey)message.Payload[0], (Velocity)message.Payload[1]),
                ChannelMessageType.NoteAftertouch => new NoteMessage(NoteMessageType.Aftertouch, channel, (NoteKey)message.Payload[0], (Velocity)message.Payload[1]),
                ChannelMessageType.ControlChangeOrMode => ControllerEnum.IsValidByte(message.Payload[0])
                    ? new ControlChange(channel, ControllerEnum.FromByte(message.Payload[0])!.Value, message.Payload[1])
                    : new ChannelMode(channel, ChannelModeOperationEnum.FromByte(message.Payload[0])!.Value, message.Payload[1]),
                ChannelMessageType.ProgramChange => new ProgramChange(channel, (GeneralMidiProgram)message.Payload[0]),
                ChannelMessageType.ChannelAftertouch => new ChannelAftertouch(channel, (Velocity)message.Payload[0]),
                ChannelMessageType.PitchBend => new PitchBend(channel, PitchBend.ValueBytesToShort(message.Payload[0], message.Payload[1])),
                _ => throw new ArgumentException()
            };
        }
        else
        {
            return message.Status switch
            {
                StatusByte.SystemExclusive => ToSysEx(message.Payload, sysexFactory, encoding),
                StatusByte.TimeCode => throw new NotImplementedException(),
                StatusByte.SongPosition => throw new NotImplementedException(),
                StatusByte.SongSelect => throw new NotImplementedException(),
                StatusByte.UndefinedF4 => throw new NotImplementedException(),
                StatusByte.UndefinedF5 => throw new NotImplementedException(),
                StatusByte.TuneRequest => throw new NotImplementedException(),
                StatusByte.EndOfExclusive => throw new NotImplementedException(),
                var x when x.IsSystemRealTimeMessage() => SystemRealTimeMessage.Get(x),
                _ => throw new ArgumentException()
            };
        }
    }

    public static Message Create(in RawMessage message, Encoding encoding)
        => Create(message, SysExMessage.TryCreateKnown, encoding);

    protected static bool IsValidDataByte(byte value) => RawMessage.IsValidPayloadByte(value);

    protected static void AppendBytesString(StringBuilder stringBuilder, ImmutableArray<byte> bytes)
    {
        bool first = true;
        foreach (var b in bytes)
        {
            if (!first) stringBuilder.Append(' ');
            stringBuilder.AppendFormat("{0:X2}", b);
            first = false;
        }
    }

    private static SysExMessage ToSysEx(ImmutableArray<byte> data, SysExMessageFactory factory, Encoding? encoding)
    {
        return factory(ManufacturerId.TryFromData(data.AsSpan()), data, encoding) ?? new UnknownSysExMessage(data);
    }
}
