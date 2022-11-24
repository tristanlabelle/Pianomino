using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public readonly struct Tempo : IEquatable<Tempo>, IComparable<Tempo>
{
    public const int DefaultQuarterNotesPerMinute = 120;
    public const int DefaultMicrosecondsPerQuarterNote = MicrosecondsPerMinute / DefaultQuarterNotesPerMinute;
    public const int MaxMicrosecondsPerQuarterNote = 0xFF_FF_FF;
    private const int MicrosecondsPerMinute = 60_000_000;

    public static Tempo QuarterNotesPerMinute120 => new();

    private readonly int microsecondsPerQuarterNoteMinusDefault;

    private Tempo(int microsecondsPerQuarterNote)
    {
        if ((microsecondsPerQuarterNote & ~0xFF_FF_FF) != 0)
            throw new ArgumentOutOfRangeException(nameof(microsecondsPerQuarterNote));
        this.microsecondsPerQuarterNoteMinusDefault = microsecondsPerQuarterNote - DefaultMicrosecondsPerQuarterNote;
    }

    public int MicrosecondsPerQuarterNote => microsecondsPerQuarterNoteMinusDefault + DefaultMicrosecondsPerQuarterNote;
    public float QuarterNotesPerMinute => MicrosecondsPerQuarterNoteToQuarterNotesPerMinute(MicrosecondsPerQuarterNote);

    public override int GetHashCode() => microsecondsPerQuarterNoteMinusDefault;
    public override bool Equals(object? obj) => obj is Tempo other && Equals(other);
    public bool Equals(Tempo other) => microsecondsPerQuarterNoteMinusDefault == other.microsecondsPerQuarterNoteMinusDefault;
    public int CompareTo(Tempo other) => microsecondsPerQuarterNoteMinusDefault.CompareTo(other.microsecondsPerQuarterNoteMinusDefault);
    public override string ToString() => FormattableString.Invariant($"♩={(int)MathF.Round(QuarterNotesPerMinute)}");

    public static Tempo FromMicrosecondsPerQuarterNote(int value) => new(value);
    public static Tempo FromQuarterNotesPerMinute(float value) => new(QuarterNotesPerMinuteToMicrosecondsPerQuarterNote(value));

    public static bool Equals(Tempo lhs, Tempo rhs) => lhs.Equals(rhs);
    public static bool operator ==(Tempo lhs, Tempo rhs) => Equals(lhs, rhs);
    public static bool operator !=(Tempo lhs, Tempo rhs) => !Equals(lhs, rhs);

    public static int Compare(Tempo lhs, Tempo rhs) => lhs.CompareTo(rhs);
    public static bool operator <(Tempo lhs, Tempo rhs) => Compare(lhs, rhs) < 0;
    public static bool operator <=(Tempo lhs, Tempo rhs) => Compare(lhs, rhs) <= 0;
    public static bool operator >(Tempo lhs, Tempo rhs) => Compare(lhs, rhs) > 0;
    public static bool operator >=(Tempo lhs, Tempo rhs) => Compare(lhs, rhs) >= 0;

    public static int QuarterNotesPerMinuteToMicrosecondsPerQuarterNote(float value)
        => checked((int)(MicrosecondsPerMinute / value));

    public static float MicrosecondsPerQuarterNoteToQuarterNotesPerMinute(int value)
        => (float)MicrosecondsPerMinute / value;
}
