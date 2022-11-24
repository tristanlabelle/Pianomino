using Pianomino.Theory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class TimeSignature : MetaMessage
{
    public const int DataLength = 4;

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

    public StandardMeter Meter => new(numerator, NoteUnit.FromTimeSignatureDenominator(1 << denominatorLog2));
    public int ClocksPerMetronomeTick => clocksPerMetronomeTick;
    public int ThirtySecondNotesPerMidiQuarterNote => thirtySecondNotesPerMidiQuarterNote;

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(MetaMessageTypeByte.TimeSignature,
            ImmutableArray.Create(numerator, denominatorLog2, clocksPerMetronomeTick, thirtySecondNotesPerMidiQuarterNote));

    public override string ToString() => $"TimeSignature({Meter})";

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.TimeSignature;

    public static TimeSignature FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != DataLength) throw new ArgumentException();
        return FromBytes(data[0], data[1], data[2], data[3]);
    }

    public static TimeSignature FromBytes(byte nn, byte dd, byte cc, byte bb)
        => new(nn, dd, cc, bb);
}
