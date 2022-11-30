using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public enum EventHeaderByte : byte
{
    Escape_SysEx = 0xF0,
    Escape = 0xF7,
    Meta = 0xFF
}

public static class EventHeaderByteEnum
{
    public static bool IsRunningStatus(this EventHeaderByte value)
        => (byte)value < 0x80;

    public static bool IsChannelStatus(this EventHeaderByte value)
        => (byte)value >= 0x80 && (byte)value < 0xF0;

    public static StatusByte? AsChannelStatus(this EventHeaderByte value)
        => IsChannelStatus(value) ? (StatusByte)(byte)value : null;

    public static bool IsEscape(this EventHeaderByte value)
        => value is EventHeaderByte.Escape or EventHeaderByte.Escape_SysEx;
}
