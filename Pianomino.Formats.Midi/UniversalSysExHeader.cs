using System;

namespace Pianomino.Formats.Midi;

public readonly struct UniversalSysExHeader
{
    public const byte NonRealTimeManufacturerIdByte = 0x7E;
    public const byte RealTimeManufacturerIdByte = 0x7F;

    public const byte AllCallDeviceId = 0x7F;

    public const int SizeInBytes = 4;

    public UniversalSysExKind Kind { get; }
    public byte DeviceId { get; }
    public byte SubId2 { get; }

    public UniversalSysExHeader(UniversalSysExKind kind, byte deviceId, byte subId2)
    {
        if (!RawMessage.IsValidPayloadByte(deviceId))
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        if (!RawMessage.IsValidPayloadByte(subId2))
            throw new ArgumentOutOfRangeException(nameof(subId2));

        this.Kind = kind;
        this.DeviceId = deviceId;
        this.SubId2 = subId2;
    }

    public UniversalSysExHeader(bool realTime, byte deviceId, byte subId1, byte subId2)
    {
        if (!RawMessage.IsValidPayloadByte(deviceId))
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        if (!RawMessage.IsValidPayloadByte(subId1))
            throw new ArgumentOutOfRangeException(nameof(subId1));
        if (!RawMessage.IsValidPayloadByte(subId2))
            throw new ArgumentOutOfRangeException(nameof(subId2));

        this.Kind = (UniversalSysExKind)subId1;
        if (realTime) this.Kind |= UniversalSysExKind.RealTimeBit;

        this.DeviceId = deviceId;
        this.SubId2 = subId2;
    }

    public ManufacturerId ManufacturerId => Kind.GetManufacturerId();
    public byte ManufacturerIdByte => Kind.GetManufacturerIdByte();
    public bool IsAllCall => DeviceId == AllCallDeviceId;
    public bool IsRealTime => Kind.IsRealTime();
    public byte SubId1 => Kind.GetSubId1();

    public static UniversalSysExHeader FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length < SizeInBytes) throw new ArgumentException();
        if (data[0] != NonRealTimeManufacturerIdByte && data[0] != RealTimeManufacturerIdByte) throw new FormatException();
        return new UniversalSysExHeader(realTime: data[0] == RealTimeManufacturerIdByte, data[1], data[2], data[3]);
    }
}
