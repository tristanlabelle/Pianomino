using Pianomino.Theory;
using System;
using System.Text;

namespace Pianomino.Formats.Midi.Messages;

public sealed class NoteMessage : ChannelMessage
{
    public new NoteMessageType Type { get; }
    public NoteKey Key { get; }
    public Velocity Velocity { get; }

    public NoteMessage(NoteMessageType type, Channel channel, NoteKey key, Velocity velocity)
        : base(channel)
    {
        this.Type = type;
        this.Key = key;
        this.Velocity = velocity;
    }

    public bool HasOnEffect => Type == NoteMessageType.On && !Velocity.IsZero;
    public bool HasOffEffect => Type == NoteMessageType.Off || (Type == NoteMessageType.On && Velocity.IsZero);

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status, Key.Number, Velocity);
    public override string ToString() => $"Note{Type}({ChannelString}, {Key}, {Velocity})";

    protected override ChannelMessageType GetChannelType() => Type switch
    {
        NoteMessageType.On => ChannelMessageType.NoteOn,
        NoteMessageType.Off => ChannelMessageType.NoteOff,
        NoteMessageType.Aftertouch => ChannelMessageType.NoteAftertouch,
        _ => throw new Exception() // Unreachable
    };
}
