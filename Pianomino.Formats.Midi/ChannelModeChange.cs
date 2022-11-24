using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum ChannelModeOperation : byte
{
    AllSoundOff = 0,
    ResetAllControllers = 1,
    LocalControl = 2,
    AllNotesOff = 3,
    OmniModeOff = 4,
    OmniModeOn = 5,
    MonoModeOn = 6,
    PolyModeOn = 7
}

public static class ChannelModeOperationEnum
{
    public const byte BaseByteValue = 120;

    public static bool IsValidByte(byte value) => value >= BaseByteValue && value < 0x80;
    public static ChannelModeOperation? FromByte(byte value)
        => IsValidByte(value) ? (ChannelModeOperation)(value - BaseByteValue) : null;
    public static byte ToByte(this ChannelModeOperation value) => (byte)((int)value + BaseByteValue);

    public static bool IsValid(this ChannelModeOperation value) => (byte)value < 8;
}
