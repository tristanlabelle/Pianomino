using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public enum MetaEventTypeByte : byte
{
    SequenceNumber = 0x00,
    TextEvent = 0x01,
    CopyrightNotice = 0x02,
    TrackName = 0x03,
    InstrumentName = 0x04,
    Lyric = 0x05,
    Marker = 0x06,
    CuePoint = 0x07,
    ProgramName = 0x08,
    DeviceName = 0x09,
    // 0x0A to 0x0F might be text messages
    ChannelPrefix = 0x20,
    Port = 0x21,
    // TrackLoop = 0x2E, ?
    EndOfTrack = 0x2F,
    // MLiveTag = 0x4B, ?
    SetTempo = 0x51,
    SmpteOffset = 0x54,
    TimeSignature = 0x58,
    KeySignature = 0x59,
    XmfPatchTypePrefix = 0x60,
    SequencerSpecific = 0x7F
}

public static class MetaMessageTypeByteEnum
{
    public static bool IsValid(this MetaEventTypeByte type) => (byte)type < 0x80;

    public static bool IsTimeZeroOnly(this MetaEventTypeByte type)
        => type is MetaEventTypeByte.SequenceNumber or MetaEventTypeByte.CopyrightNotice
        or MetaEventTypeByte.SmpteOffset;

    public static bool IsFirstTrackOnly(this MetaEventTypeByte type)
        => type is MetaEventTypeByte.SequenceNumber or MetaEventTypeByte.CopyrightNotice
        or MetaEventTypeByte.SmpteOffset;

    public static bool IsKnownTextMessage(this MetaEventTypeByte type)
        => type is MetaEventTypeByte.TextEvent or MetaEventTypeByte.CopyrightNotice
        or MetaEventTypeByte.TrackName or MetaEventTypeByte.InstrumentName
        or MetaEventTypeByte.Lyric or MetaEventTypeByte.Marker or MetaEventTypeByte.CuePoint
        or MetaEventTypeByte.ProgramName or MetaEventTypeByte.DeviceName;
}
