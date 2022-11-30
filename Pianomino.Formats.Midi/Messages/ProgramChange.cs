using System;
using System.Text;

namespace Pianomino.Formats.Midi.Messages;

public sealed class ProgramChange : ChannelMessage
{
    public GeneralMidiProgram Program { get; }

    public ProgramChange(Channel channel, GeneralMidiProgram program)
        : base(channel)
    {
        if (!RawMessage.IsValidPayloadByte((byte)program)) throw new ArgumentOutOfRangeException(nameof(program));
        this.Program = program;
    }

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status, (byte)Program);
    public override string ToString() => $"ProgramChange({ChannelString}, {Program})";

    protected override ChannelMessageType GetChannelType() => ChannelMessageType.ProgramChange;
}
