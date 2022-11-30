using Pianomino.Theory;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class TimeSignature : MetaEvent
{
    public const int PayloadLength = 4;

    private readonly byte numerator;
    private readonly byte denominatorLog2;
    private readonly byte clocksPerMetronomeTick;
    private readonly byte thirtySecondNotesPerMidiQuarterNote;

    private TimeSignature(byte nn, byte dd, byte cc, byte bb)
    {
        this.numerator = nn;
        this.denominatorLog2 = dd;
        this.clocksPerMetronomeTick = cc;
        this.thirtySecondNotesPerMidiQuarterNote = bb;
    }

    public TimeSignature(StandardMeter meter, int clocksPerMetronomeTick, int thirtySecondNotesPerMidiQuarterNote = 8)
    {
        this.numerator = checked((byte)meter.TimeSignatureNumerator);
        this.denominatorLog2 = checked((byte)meter.TimeSignatureDenominatorLog2);
        this.clocksPerMetronomeTick = checked((byte)clocksPerMetronomeTick);
        this.thirtySecondNotesPerMidiQuarterNote = checked((byte)thirtySecondNotesPerMidiQuarterNote);
    }

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.TimeSignature;
    public StandardMeter Meter => new(numerator, NoteUnit.FromTimeSignatureDenominator(1 << denominatorLog2));
    public int ClocksPerMetronomeTick => clocksPerMetronomeTick;
    public int ThirtySecondNotesPerMidiQuarterNote => thirtySecondNotesPerMidiQuarterNote;

    public override RawEvent ToRaw(Encoding encoding)
        => RawEvent.CreateMeta(MetaEventTypeByte.TimeSignature,
            ImmutableArray.Create(numerator, denominatorLog2, clocksPerMetronomeTick, thirtySecondNotesPerMidiQuarterNote));

    public override string ToString() => $"TimeSignature({Meter})";

    public static TimeSignature FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != PayloadLength) throw new ArgumentException();
        return FromBytes(data[0], data[1], data[2], data[3]);
    }

    public static TimeSignature FromBytes(byte nn, byte dd, byte cc, byte bb)
        => new(nn, dd, cc, bb);
}
