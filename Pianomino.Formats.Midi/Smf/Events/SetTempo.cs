using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class SetTempo : MetaEvent
{
    public const int PayloadLength = 3;

    public Tempo Tempo { get; }

    public SetTempo(Tempo tempo) => this.Tempo = tempo;

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.SetTempo;

    public override RawEvent ToRaw(Encoding encoding)
    {
        int value = Tempo.MicrosecondsPerQuarterNote;
        return RawEvent.CreateMeta(MetaEventTypeByte.SetTempo,
            ImmutableArray.Create((byte)(value >> 16), (byte)(value >> 8), (byte)value));
    }

    public override string ToString() => $"SetTempo({Tempo})";

    public static SetTempo FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != PayloadLength) throw new ArgumentException();
        return FromBytes(bytes[0], bytes[1], bytes[2]);
    }

    public static SetTempo FromBytes(byte first, byte second, byte third)
        => new(Tempo.FromMicrosecondsPerQuarterNote(((int)first << 16) | ((int)second << 8) | (int)third));
}
