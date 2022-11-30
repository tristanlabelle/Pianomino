using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class PitchBend : ChannelMessage
{
    public const short MinimumValue = -0x4000;
    public const short MaximumValue = 0x3FFF;

    public short Value { get; }

    public PitchBend(Channel channel, short value)
        : base(channel)
    {
        if (value < MinimumValue || value > MaximumValue) throw new ArgumentOutOfRangeException(nameof(value));
        this.Value = value;
    }

    public PitchBend(Channel channel, float value)
        : this(channel, ValueFloatToShort(value)) { }

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status, (byte)(Value & 0x7F), (byte)(Value >> 7));

    public override string ToString() => $"PitchBend({ChannelString}, {Value})";

    protected override ChannelMessageType GetChannelType() => ChannelMessageType.ProgramChange;

    public static short ValueBytesToShort(byte first, byte second)
    {
        if (!RawMessage.IsValidPayloadByte(first) || !RawMessage.IsValidPayloadByte(second)) throw new ArgumentOutOfRangeException();
        return (short)((((int)second << 7) | first) + MinimumValue);
    }

    public static float ValueShortToFloat(short value) => throw new NotImplementedException();

    public static (byte First, byte Second) ValueShortToBytes(short value)
    {
        if (value < MinimumValue || value > MaximumValue) throw new ArgumentOutOfRangeException();
        value -= MinimumValue;
        return ((byte)(value & 0x7F), (byte)(value >> 7));
    }

    public static short ValueFloatToShort(float value) => throw new NotImplementedException();
}
