using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public enum SmfMessageType
{
    Channel, // 80 to EF
    SysEx_Unit, // F0 ... F7
    SysEx_BeginPacket, // F0 ... ~F7
    SysEx_ContinuationPacket, // [F0/F7] F7 ... ~F7
    SysEx_EndPacket, // [F0/F7] F7 ... F7
    Escape, // [~F0/F7] F7 ...
    Meta // FF
}

public static class SmfMessageTypeEnum
{
    public static SmfMessageType GetSysEx(bool first, bool last)
    {
        return first
            ? (last ? SmfMessageType.SysEx_Unit : SmfMessageType.SysEx_BeginPacket)
            : (last ? SmfMessageType.SysEx_EndPacket : SmfMessageType.SysEx_ContinuationPacket);
    }

    public static bool IsWireCompatible(this SmfMessageType type)
        => type == SmfMessageType.Channel || type == SmfMessageType.SysEx_Unit;
}
