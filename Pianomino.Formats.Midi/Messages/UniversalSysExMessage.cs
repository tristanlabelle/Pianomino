using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public abstract class UniversalSysExMessage : SysExMessage
{
    public byte DeviceId { get; }

    protected UniversalSysExMessage(byte deviceId)
    {
        if (!RawMessage.IsValidPayloadByte(deviceId))
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        this.DeviceId = deviceId;
    }

    public abstract UniversalSysExKind Kind { get; }
    public bool IsAllCall => DeviceId == UniversalSysExHeader.AllCallDeviceId;
    public byte SysExId => Kind.GetManufacturerIdByte();
    public bool IsRealTime => Kind.IsRealTime();
    public byte SubId1 => Kind.GetSubId1();
    public abstract byte SubId2 { get; }
    protected string DeviceIdString => IsAllCall ? "AllCall" : DeviceId.ToString("X2");

    public UniversalSysExHeader Header => new(Kind, DeviceId, SubId2);

    public override ManufacturerId? TryGetManufacturerId() => IsRealTime
        ? ManufacturerId.UniversalSysEx_RealTime : ManufacturerId.UniversalSysEx_NonRealTime;
}
