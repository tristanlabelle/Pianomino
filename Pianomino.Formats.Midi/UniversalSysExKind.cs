using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum UniversalSysExKind : byte
{
    /// <summary>
    /// Distinguishes between non real-time (7E) and real-time (7F)
    /// universal system exclusive messages.
    /// </summary>
    RealTimeBit = 0x80,

    SampleDump_DumpHeader = 0x01,
    SampleDump_DataPacket = 0x02,
    SampleDump_DumpRequest = 0x03,
    SampleDump_MultipleLoopPoint = 0x05,
    DeviceInquiry = 0x06,
    FileDump = 0x07,
    BulkTuning = 0x08,
    GeneralMidiSystem = 0x09,
    DownloadableSounds = 0x0A,
    FileReference = 0x0B,
    VisualControl = 0x0C,
    CapabilityInquiry = 0xD,

    Handshaking_EndOfFile = 0x7B,
    Handshaking_Wait = 0x7C,
    Handshaking_Cancel = 0x7D,
    Handshaking_Nak = 0x7E,
    Handshaking_Ack = 0x7F,

    TimeCodeFull = 0x01 | RealTimeBit,
    ShowControl = 0x02 | RealTimeBit,
    NotationInformation = 0x03 | RealTimeBit,
    DeviceControl = 0x04 | RealTimeBit,
    Cueing = 0x05 | RealTimeBit,
    MachineControl_Command = 0x06 | RealTimeBit,
    MachineControl_Response = 0x07 | RealTimeBit,
    TuningStandard = 0x08 | RealTimeBit,
    ControllerDestination = 0x09 | RealTimeBit,
    KeyBasedInstrumentControl = 0x0A | RealTimeBit,
    ScalablePolyphony = 0x0B | RealTimeBit,
    MobilePhoneControl = 0x0C | RealTimeBit,
}

[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class UniversalSysExKindEnum
{
    public static bool IsRealTime(this UniversalSysExKind value)
        => (value & UniversalSysExKind.RealTimeBit) != 0;

    public static ManufacturerId GetManufacturerId(this UniversalSysExKind value)
        => IsRealTime(value) ? ManufacturerId.UniversalSysEx_RealTime : ManufacturerId.UniversalSysEx_NonRealTime;

    public static byte GetManufacturerIdByte(this UniversalSysExKind value)
        => IsRealTime(value) ? UniversalSysExHeader.RealTimeManufacturerIdByte : UniversalSysExHeader.NonRealTimeManufacturerIdByte;

    public static byte GetSubId1(this UniversalSysExKind value)
        => (byte)(value & ~UniversalSysExKind.RealTimeBit);
}
