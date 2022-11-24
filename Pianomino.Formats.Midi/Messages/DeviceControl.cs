using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class DeviceControl : UniversalSysExMessage
{
    public const int DataLength = 2;
    public const ushort InclusiveMaxValue = 0x3FFF;

    public DeviceController Controller { get; }
    public ushort Value { get; }

    public DeviceControl(byte deviceId, DeviceController controller, ushort value)
        : base(deviceId)
    {
        if (value > InclusiveMaxValue) throw new ArgumentOutOfRangeException(nameof(value));
        this.Controller = controller;
        this.Value = value;
    }

    public override UniversalSysExKind Kind => UniversalSysExKind.DeviceControl;
    public override byte SubId2 => (byte)Controller;

    public override RawMessage ToRaw(Encoding encoding) => throw new NotImplementedException();

    public override string ToString() => $"DeviceControl({Controller}, {Value})";

    public override ManufacturerId? TryGetManufacturerId() => ManufacturerId.UniversalSysEx_RealTime;

    public static DeviceControl FromBytes(byte deviceId, byte subId2, byte firstData, byte secondData)
    {
        return new DeviceControl(deviceId, (DeviceController)subId2, (ushort)(((int)secondData << 7) | (int)firstData));
    }
}
