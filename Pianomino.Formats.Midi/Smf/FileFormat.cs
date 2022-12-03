using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

internal static class FileFormat
{
    public const uint RmidGroupID = 0x52_49_46_46; // RIFF
    public const uint RmidTypeID = 0x52_4D_49_44; // RMID

    public enum ChunkType : uint
    {
        Header = 0x4D_54_68_64, // MThd
        Track = 0x4D_54_72_6B // MTrk
    }

    public readonly struct ChunkHeader
    {
        public const int SizeInBytes = 8;

        public ChunkType Type { get; init; }
        public uint Length { get; init; }
    }

    public readonly struct HeaderChunk
    {
        public const int Length = 6;

        public TrackFormat TrackFormat { get; init; }
        public ushort TrackCount { get; init; }
        public TimeDivision TimeDivision { get; init; }
    }
}
