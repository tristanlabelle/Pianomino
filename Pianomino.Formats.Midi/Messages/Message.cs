using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

/// <summary>
/// Base class for immutable MIDI messages,
/// whether over the wire or in standard midi files.
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
                ChannelMessageType.NoteOff => new NoteMessage(NoteMessageType.Off, channel, (NoteKey)message.Data[0], (Velocity)message.Data[1]),
                ChannelMessageType.NoteOn => new NoteMessage(NoteMessageType.On, channel, (NoteKey)message.Data[0], (Velocity)message.Data[1]),
                ChannelMessageType.NoteAftertouch => new NoteMessage(NoteMessageType.Aftertouch, channel, (NoteKey)message.Data[0], (Velocity)message.Data[1]),
                ChannelMessageType.ControlChangeOrMode => ControllerEnum.IsValidByte(message.Data[0])
                    ? new ControlChange(channel, ControllerEnum.FromByte(message.Data[0])!.Value, message.Data[1])
                    : new ChannelMode(channel, ChannelModeOperationEnum.FromByte(message.Data[0])!.Value, message.Data[1]),
                ChannelMessageType.ProgramChange => new ProgramChange(channel, (GeneralMidiProgram)message.Data[0]),
                ChannelMessageType.ChannelAftertouch => new ChannelAftertouch(channel, (Velocity)message.Data[0]),
                ChannelMessageType.PitchBend => new PitchBend(channel, PitchBend.ValueBytesToShort(message.Data[0], message.Data[1])),
                _ => throw new ArgumentException()
            };
        }
        else
        {
            return message.Status switch
            {
                StatusByte.SystemExclusive => ToSysEx(message.Data.ToImmutableArray(), sysexFactory, encoding),
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

    protected static bool IsValidDataByte(byte value) => RawMessage.IsValidDataByte(value);

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
