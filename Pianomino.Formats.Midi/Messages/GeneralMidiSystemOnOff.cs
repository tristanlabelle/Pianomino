using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class GeneralMidiSystemOnOff : UniversalSysExMessage
{
    public static GeneralMidiSystemOnOff AllCallOn { get; } = new GeneralMidiSystemOnOff(UniversalSysExHeader.AllCallDeviceId, on: true);
    public static GeneralMidiSystemOnOff AllCallOff { get; } = new GeneralMidiSystemOnOff(UniversalSysExHeader.AllCallDeviceId, on: false);

    public bool IsOn { get; }

    public override UniversalSysExKind Kind => UniversalSysExKind.GeneralMidiSystem;

    public override byte SubId2 => IsOn ? (byte)1 : (byte)2;

    public GeneralMidiSystemOnOff(byte deviceId, bool on) : base(deviceId) => this.IsOn = on;

    public override RawMessage ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString() => $"GeneralMidiSystem({DeviceIdString}, {(IsOn ? "On" : "Off")}";
}
