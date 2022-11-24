using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public readonly struct TimeCode
{
    public const int ByteLength = 5;

    private readonly byte hoursAndFrequency;
    private readonly byte minutes;
    private readonly byte seconds;
    private readonly byte frames;
    private readonly byte fractionalFrames;

    private TimeCode(byte hr, byte mn, byte sc, byte fr, byte ff)
    {
        this.hoursAndFrequency = hr;
        this.minutes = mn;
        this.seconds = sc;
        this.frames = fr;
        this.fractionalFrames = ff;
    }

    public TimeCode(TimeCodeFrequency frequency, int hours, int minutes, int seconds, int frames = 0, int fractionalFrames = 0)
    {
        throw new NotImplementedException();
    }

    public TimeCodeFrequency Frequency => (TimeCodeFrequency)((hoursAndFrequency >> 5) & 3);
    public int Hours => hoursAndFrequency & 0x1F;
    public int Minutes => minutes & 0x3F;
    public int Seconds => seconds & 0x3F;
    public int Frames => frames & 0x1F;
    public int FractionalFrames => fractionalFrames & 0x7F;

    public override string ToString() => $"{Hours:D2}:{Minutes:D2}:{Seconds:D2}:{Frames:D2}.{FractionalFrames:D2} @ {Frequency.GetString()}";

    public static TimeCode FromBytes(byte hr, byte mn, byte sc, byte fr, byte ff)
        => new TimeCode(hr, mn, sc, fr, ff);
}

public enum TimeCodeFlags
{
    ColorFramed,
    NegativeFrame,
}

public enum TimeCodeFrequency
{
    Frames24,
    Frames25,
    DropFrame30,
    Frames30
}

public static class TimeCodeFrequencyEnum
{
    public static string GetString(this TimeCodeFrequency value) => value switch
    {
        TimeCodeFrequency.Frames24 => "24fps",
        TimeCodeFrequency.Frames25 => "25fps",
        TimeCodeFrequency.DropFrame30 => "30fps (drop)",
        TimeCodeFrequency.Frames30 => "30fps",
        _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TimeCodeFrequency))
    };
}
