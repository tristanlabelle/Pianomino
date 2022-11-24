using System;

namespace Pianomino.Formats.Midi;

/// <summary>
/// Represents a midi status byte, which is used to identify the type of a message.
/// </summary>
public enum StatusByte : byte
{
    Invalid = 0, // All status bytes should be >= 0x80

    NoteOff_Channel1 = 0x80,
    NoteOff_Channel2,
    NoteOff_Channel3,
    NoteOff_Channel4,
    NoteOff_Channel5,
    NoteOff_Channel6,
    NoteOff_Channel7,
    NoteOff_Channel8,
    NoteOff_Channel9,
    NoteOff_Channel10,
    NoteOff_Channel11,
    NoteOff_Channel12,
    NoteOff_Channel13,
    NoteOff_Channel14,
    NoteOff_Channel15,
    NoteOff_Channel16,

    NoteOn_Channel1 = 0x90,
    NoteOn_Channel2,
    NoteOn_Channel3,
    NoteOn_Channel4,
    NoteOn_Channel5,
    NoteOn_Channel6,
    NoteOn_Channel7,
    NoteOn_Channel8,
    NoteOn_Channel9,
    NoteOn_Channel10,
    NoteOn_Channel11,
    NoteOn_Channel12,
    NoteOn_Channel13,
    NoteOn_Channel14,
    NoteOn_Channel15,
    NoteOn_Channel16,

    NoteAftertouch_Channel1 = 0xA0,
    NoteAftertouch_Channel2,
    NoteAftertouch_Channel3,
    NoteAftertouch_Channel4,
    NoteAftertouch_Channel5,
    NoteAftertouch_Channel6,
    NoteAftertouch_Channel7,
    NoteAftertouch_Channel8,
    NoteAftertouch_Channel9,
    NoteAftertouch_Channel10,
    NoteAftertouch_Channel11,
    NoteAftertouch_Channel12,
    NoteAftertouch_Channel13,
    NoteAftertouch_Channel14,
    NoteAftertouch_Channel15,
    NoteAftertouch_Channel16,

    ControlChangeOrMode_Channel1 = 0xB0,
    ControlChangeOrMode_Channel2,
    ControlChangeOrMode_Channel3,
    ControlChangeOrMode_Channel4,
    ControlChangeOrMode_Channel5,
    ControlChangeOrMode_Channel6,
    ControlChangeOrMode_Channel7,
    ControlChangeOrMode_Channel8,
    ControlChangeOrMode_Channel9,
    ControlChangeOrMode_Channel10,
    ControlChangeOrMode_Channel11,
    ControlChangeOrMode_Channel12,
    ControlChangeOrMode_Channel13,
    ControlChangeOrMode_Channel14,
    ControlChangeOrMode_Channel15,
    ControlChangeOrMode_Channel16,

    ProgramChange_Channel1 = 0xC0,
    ProgramChange_Channel2,
    ProgramChange_Channel3,
    ProgramChange_Channel4,
    ProgramChange_Channel5,
    ProgramChange_Channel6,
    ProgramChange_Channel7,
    ProgramChange_Channel8,
    ProgramChange_Channel9,
    ProgramChange_Channel10,
    ProgramChange_Channel11,
    ProgramChange_Channel12,
    ProgramChange_Channel13,
    ProgramChange_Channel14,
    ProgramChange_Channel15,
    ProgramChange_Channel16,

    ChannelAftertouch_Channel1 = 0xD0,
    ChannelAftertouch_Channel2,
    ChannelAftertouch_Channel3,
    ChannelAftertouch_Channel4,
    ChannelAftertouch_Channel5,
    ChannelAftertouch_Channel6,
    ChannelAftertouch_Channel7,
    ChannelAftertouch_Channel8,
    ChannelAftertouch_Channel9,
    ChannelAftertouch_Channel10,
    ChannelAftertouch_Channel11,
    ChannelAftertouch_Channel12,
    ChannelAftertouch_Channel13,
    ChannelAftertouch_Channel14,
    ChannelAftertouch_Channel15,
    ChannelAftertouch_Channel16,

    PitchBend_Channel1 = 0xE0,
    PitchBend_Channel2,
    PitchBend_Channel3,
    PitchBend_Channel4,
    PitchBend_Channel5,
    PitchBend_Channel6,
    PitchBend_Channel7,
    PitchBend_Channel8,
    PitchBend_Channel9,
    PitchBend_Channel10,
    PitchBend_Channel11,
    PitchBend_Channel12,
    PitchBend_Channel13,
    PitchBend_Channel14,
    PitchBend_Channel15,
    PitchBend_Channel16,

    SystemExclusive = 0xF0,

    TimeCode = 0xF1,
    SongPosition = 0xF2,
    SongSelect = 0xF3,
    UndefinedF4 = 0xF4,
    UndefinedF5 = 0xF5,
    TuneRequest = 0xF6,
    EndOfExclusive = 0xF7,

    Clock = 0xF8,
    UndefinedF9 = 0xF9,
    Start = 0xFA,
    Continue = 0xFB,
    Stop = 0xFC,
    UndefinedFD = 0xFD,
    ActiveSensing = 0xFE,
    Reset = 0xFF
}

public static class StatusByteEnum
{
    public static bool IsValid(this StatusByte value) => (byte)value >= 0x80;

    public static bool IsChannelMessage(this StatusByte value)
        => (byte)value >= 0x80 && (byte)value <= 0xEF;

    public static bool IsNoteOffMessage(this StatusByte value)
        => (byte)value >= 0x80 && (byte)value <= 0x8F;

    public static bool IsNoteOnMessage(this StatusByte value)
        => (byte)value >= 0x90 && (byte)value <= 0x9F;

    public static bool IsNoteOnOffMessage(this StatusByte value)
        => (byte)value >= 0x80 && (byte)value <= 0x9F;

    public static bool IsNoteMessage(this StatusByte value)
        => (byte)value >= 0x80 && (byte)value <= 0xAF;

    public static StatusByte Create(ChannelMessageType type, Channel channel)
        => (StatusByte)(0x80 + (int)type * 0x10 + (int)channel);

    public static Channel GetChannel(this StatusByte value)
    {
        if (!IsValid(value) || (byte)value >= 0xF0) throw new ArgumentOutOfRangeException();
        return (Channel)((byte)value & 0xF);
    }

    public static bool AsChannelMessage(this StatusByte value, out ChannelMessageType type, out Channel channel)
    {
        if (!IsValid(value)) throw new ArgumentOutOfRangeException();
        if ((byte)value >= 0xF0)
        {
            // System message
            type = default;
            channel = default;
            return false;
        }

        type = (ChannelMessageType)(((byte)value & 0x70) >> 4);
        channel = (Channel)((byte)value & 0xF);
        return true;
    }

    public static bool AsNoteMessage(this StatusByte value, out NoteMessageType type, out Channel channel)
    {
        if (AsChannelMessage(value, out var channelMessageType, out channel) && channelMessageType.IsNoteMessage())
        {
            type = channelMessageType switch
            {
                ChannelMessageType.NoteOff => NoteMessageType.Off,
                ChannelMessageType.NoteOn => NoteMessageType.On,
                ChannelMessageType.NoteAftertouch => NoteMessageType.Aftertouch,
                _ => throw new UnreachableException()
            };

            return true;
        }
        else
        {
            type = default;
            channel = default;
            return false;
        }
    }

    public static bool IsSystemMessage(this StatusByte value) => (byte)value >= 0xF0;

    public static bool IsSystemRealTimeMessage(this StatusByte value) => (byte)value >= 0xF8;

    public static MessageDataLengthType GetDataLengthType(this StatusByte value)
        => (byte)value switch
        {
            < 0x80 => throw new ArgumentOutOfRangeException(),
            < 0xC0 => MessageDataLengthType.TwoBytes, // NoteOn, NoteOff, KeyPressure, ControlChange
                < 0xE0 => MessageDataLengthType.OneByte, // ProgramChange, ChannelPressure
                < 0xF0 => MessageDataLengthType.TwoBytes, // PitchBend
                (byte)StatusByte.SystemExclusive => MessageDataLengthType.Variable,
            (byte)StatusByte.TimeCode => MessageDataLengthType.OneByte,
            (byte)StatusByte.SongPosition => MessageDataLengthType.TwoBytes,
            (byte)StatusByte.SongSelect => MessageDataLengthType.OneByte,
            (byte)StatusByte.UndefinedF4 => MessageDataLengthType.Variable,
            (byte)StatusByte.UndefinedF5 => MessageDataLengthType.Variable,
            (byte)StatusByte.TuneRequest => MessageDataLengthType.ZeroBytes,
            (byte)StatusByte.EndOfExclusive => MessageDataLengthType.ZeroBytes,
            >= 0xF8 => 0 // Real-time
            };

    public static int? GetDataLength(this StatusByte value)
        => GetDataLengthType(value).ToNullableByteCount();

    public static bool IsUndefined(this StatusByte value)
        => value switch
        {
            < (StatusByte)0x80 => throw new ArgumentOutOfRangeException(),
            StatusByte.UndefinedF4 => true,
            StatusByte.UndefinedF5 => true,
            StatusByte.UndefinedF9 => true,
            StatusByte.UndefinedFD => true,
            _ => false
        };
}
