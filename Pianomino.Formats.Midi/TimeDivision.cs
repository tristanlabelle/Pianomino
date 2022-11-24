using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public readonly struct TimeDivision
{
    public static TimeDivision OneTickPerQuarterNote => FromTicksPerQuarterNote(1);

    public ushort RawValue { get; }

    private TimeDivision(ushort rawValue) => this.RawValue = rawValue;

    public TimeDivisionFormat Format => (TimeDivisionFormat)(RawValue >> 15);
    public bool IsTicksPerQuarterNote => Format == TimeDivisionFormat.TicksPerQuarterNote;
    public bool IsSmpteTimeCode => Format == TimeDivisionFormat.SmpteTimeCode;

    public int TicksPerQuarterNote => IsTicksPerQuarterNote ? RawValue : throw new InvalidOperationException();
    public SmpteFormatByte SmpteFormat => IsSmpteTimeCode ? (SmpteFormatByte)(RawValue >> 8) : throw new InvalidOperationException();
    public int SmpteTicksPerFrame => IsSmpteTimeCode ? (RawValue & 0xFF) : throw new InvalidOperationException();

    public override string ToString() => IsTicksPerQuarterNote
        ? $"{TicksPerQuarterNote} ticks/quarter note"
        : $"{SmpteTicksPerFrame} ticks/frame @ {-(sbyte)RawValue} frames/second";

    public static TimeDivision FromRawValue(ushort value) => new(value);

    public static TimeDivision FromTicksPerQuarterNote(int value)
    {
        if ((uint)value >= 0x8000) throw new ArgumentOutOfRangeException(nameof(value));
        return new((ushort)value);
    }

    public static TimeDivision FromSmpteTimeCode(SmpteFormatByte format, int ticksPerFrame)
    {
        if (((byte)format & 0x80) == 0) throw new ArgumentOutOfRangeException(nameof(format));
        if ((uint)ticksPerFrame - 1 >= 0xFF) throw new ArgumentOutOfRangeException(nameof(ticksPerFrame));
        return new((ushort)(((ushort)format << 8) | ticksPerFrame));
    }
}

public enum TimeDivisionFormat
{
    TicksPerQuarterNote,
    SmpteTimeCode
}

public enum SmpteFormatByte : byte
{
    FramesPerSecond_24 = (-24) & 0xFF,
    FramesPerSecond_25 = (-25) & 0xFF,
    FramesPerSecond_30DropFrame = (-29) & 0xFF,
    FramesPerSecond_30 = (-30) & 0xFF,
}

public static class SmpteFormatByteEnum
{
    public static bool IsDefined(this SmpteFormatByte value)
        => value == SmpteFormatByte.FramesPerSecond_24
        || value == SmpteFormatByte.FramesPerSecond_25
        || value == SmpteFormatByte.FramesPerSecond_30DropFrame
        || value == SmpteFormatByte.FramesPerSecond_30;
}
