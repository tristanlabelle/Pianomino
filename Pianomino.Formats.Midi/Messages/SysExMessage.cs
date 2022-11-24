using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Messages;

/// <summary>
/// Base class for MIDI system exclusive messages.
/// </summary>
public abstract class SysExMessage : Message
{
    private protected SysExMessage() { }

    public override StatusByte Status => StatusByte.SystemExclusive;

    public abstract ManufacturerId? TryGetManufacturerId();

    public static SysExMessage? TryCreateKnown(
        ManufacturerId? manufacturerId, ImmutableArray<byte> data, Encoding? encoding)
    {
        if (manufacturerId == ManufacturerId.UniversalSysEx_NonRealTime || manufacturerId == ManufacturerId.UniversalSysEx_RealTime)
        {
            var universalHeader = UniversalSysExHeader.FromBytes(data.AsSpan());
            return universalHeader.Kind switch
            {
                UniversalSysExKind.Handshaking_Ack
                    or UniversalSysExKind.Handshaking_Nak
                    or UniversalSysExKind.Handshaking_Cancel
                    or UniversalSysExKind.Handshaking_Wait
                    or UniversalSysExKind.Handshaking_EndOfFile
                    => new Handshake(universalHeader.Kind, universalHeader.DeviceId, universalHeader.SubId2),
                UniversalSysExKind.GeneralMidiSystem when universalHeader.SubId2 is 1 or 2
                    => new GeneralMidiSystemOnOff(universalHeader.DeviceId, on: universalHeader.SubId2 == 1),
                UniversalSysExKind.DeviceControl when data.Length == UniversalSysExHeader.SizeInBytes + DeviceControl.DataLength
                    => DeviceControl.FromBytes(universalHeader.DeviceId, universalHeader.SubId2, data[4], data[5]),
                _ => null
            };
        }

        return null;
    }
}

public delegate SysExMessage? SysExMessageFactory(
    ManufacturerId? manufacturerId, ImmutableArray<byte> data, Encoding? encoding);
