using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class ControlChange : ChannelMessage
{
    public Controller Controller { get; }
    public byte Value { get; }

    public ControlChange(Channel channel, Controller controller, byte value)
        : base(channel)
    {
        if (!controller.IsValid()) throw new ArgumentOutOfRangeException(nameof(controller));
        if (!RawMessage.IsValidDataByte(value)) throw new ArgumentOutOfRangeException(nameof(value));
        this.Controller = controller;
        this.Value = value;
    }

    public ControlChange(Channel channel, Controller controller, float value)
        : this(channel, controller, Velocity.FloatToByte(value)) { }

    public float ValueFloat => Velocity.ByteToFloat(Value);

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status, Controller.ToByte(), Value);
    public override string ToString() => $"ControlChange({ChannelString}, {Controller}, {Value})";
    protected override ChannelMessageType GetChannelType() => ChannelMessageType.ControlChangeOrMode;
}
