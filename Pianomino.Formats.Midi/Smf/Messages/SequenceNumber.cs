using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class SequenceNumber : MetaMessage
{
    public static SequenceNumber Auto { get; } = new SequenceNumber(value: null);

    public ushort? Number { get; }

    public SequenceNumber(ushort? value) => this.Number = value;

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(MetaMessageTypeByte.SequenceNumber,
            Number is ushort number ? ImmutableArray.Create((byte)(number >> 8), (byte)number) : ImmutableArray<byte>.Empty);

    public override string ToString() => $"SequenceNumber({Number?.ToString() ?? "Auto"})";
    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.SequenceNumber;

    public static SequenceNumber? TryFromData(ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException();
        return data.Length switch
        {
            0 => Auto,
            2 => new((ushort)(((ushort)data[0] << 8) | data[1])),
            _ => throw new ArgumentException()
        };
    }

    public static bool IsValidData(ImmutableArray<byte> data)
        => !data.IsDefault || data.Length == 0 || data.Length == 2;
}
