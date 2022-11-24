using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum ChannelMessageType : byte
{
    NoteOff,
    NoteOn,
    NoteAftertouch,
    ControlChangeOrMode,
    ProgramChange,
    ChannelAftertouch,
    PitchBend
}

public static class ChannelMessageTypeEnum
{
    public static bool IsValid(this ChannelMessageType type)
        => type <= ChannelMessageType.PitchBend;

    public static bool IsNoteMessage(this ChannelMessageType type)
        => type >= ChannelMessageType.NoteOff || type <= ChannelMessageType.NoteAftertouch;

    public static NoteMessageType? AsNoteMessage(this ChannelMessageType type) => type switch
    {
        ChannelMessageType.NoteOff => NoteMessageType.Off,
        ChannelMessageType.NoteOn => NoteMessageType.On,
        ChannelMessageType.NoteAftertouch => NoteMessageType.Aftertouch,
        _ => null
    };

    public static bool HasOneDataByte(this ChannelMessageType type)
        => type == ChannelMessageType.ProgramChange || type == ChannelMessageType.ChannelAftertouch;

    public static bool HasTwoDataBytes(this ChannelMessageType type)
        => !HasOneDataByte(type);

    public static int GetDataLength(this ChannelMessageType type)
        => HasTwoDataBytes(type) ? 2 : 1;

    public static StatusByte GetStatusByte(this ChannelMessageType type, Channel channel)
    {
        if (!channel.IsValid()) throw new ArgumentOutOfRangeException();
        return (StatusByte)(0x80 | ((uint)type << 4) | (byte)channel);
    }
}
