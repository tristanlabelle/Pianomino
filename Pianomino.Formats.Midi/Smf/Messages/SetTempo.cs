using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SetTempo : MetaMessage
{
    public const int DataLength = 3;

    public Tempo Tempo { get; }

    public SetTempo(Tempo tempo) => this.Tempo = tempo;

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.SetTempo;

    public override RawSmfMessage ToRaw(Encoding encoding)
    {
        int value = Tempo.MicrosecondsPerQuarterNote;
        return RawSmfMessage.CreateMeta(MetaMessageTypeByte.SetTempo,
            ImmutableArray.Create((byte)(value >> 16), (byte)(value >> 8), (byte)value));
    }

    public override string ToString() => $"SetTempo({Tempo})";

    public static SetTempo FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != DataLength) throw new ArgumentException();
        return FromBytes(bytes[0], bytes[1], bytes[2]);
    }

    public static SetTempo FromBytes(byte first, byte second, byte third)
        => new(Tempo.FromMicrosecondsPerQuarterNote(((int)first << 16) | ((int)second << 8) | (int)third));
}
