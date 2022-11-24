using System;
using System.Collections.Generic;

namespace Pianomino.Formats.Midi;

/// <summary>
/// Tracks the state associated with a midi channel through messages received.
/// </summary>
public sealed class ChannelState
{
    public ChannelNotesState Notes { get; } = new();
    private readonly byte?[] controllerValues = new byte?[ControllerEnum.ExclusiveMaxValue];
    public short PitchBend { get; set; }
    public GeneralMidiProgram? Program { get; set; }
    public bool? OmniMode { get; set; }
    public bool? PolyMode { get; set; }
    public bool? LocalControl { get; set; }

    public byte?[] ControllerValues => controllerValues;

    public float PitchBendFloat
    {
        get => Messages.PitchBend.ValueShortToFloat(PitchBend);
        set => PitchBend = Messages.PitchBend.ValueFloatToShort(PitchBend);
    }

    public byte? GetControllerValue(Controller controller) => controllerValues[(int)controller];

    public void Update(RawMessage message, Channel? channelFilter)
    {
        if (!message.AsChannelMessage(out var type, out var channel) || channel != (channelFilter ?? channel)) return;
        Update(type, message.Data.FirstByteOrZero, message.Data.SecondByteOrZero);
    }

    public void Update(ChannelMessageType type, byte firstByte, byte secondByte)
    {
        if (!RawMessage.IsValidDataByte(firstByte)) throw new ArgumentOutOfRangeException(nameof(firstByte));
        if (!RawMessage.IsValidDataByte(secondByte)) throw new ArgumentOutOfRangeException(nameof(secondByte));

        if (type.AsNoteMessage() is NoteMessageType noteType)
        {
            Notes.HandleMessage(noteType, (NoteKey)firstByte, (Velocity)secondByte);
        }
        else if (type == ChannelMessageType.ControlChangeOrMode)
        {
            Controller controller = (Controller)firstByte;
            if (controller.IsValid())
            {
                controllerValues[firstByte] = secondByte;

                // When an MSB is received, the receiver should set its concept of the LSB to zero.
                if (controller.GetLsbIfMsb() is Controller lsbController)
                    controllerValues[(byte)lsbController] = 0;
            }
            else
            {
                var op = (ChannelModeOperation)firstByte;
                if (op == ChannelModeOperation.AllNotesOff)
                    Notes.Reset();
                else if (op == ChannelModeOperation.ResetAllControllers)
                    Array.Clear(controllerValues, 0, length: controllerValues.Length);
                else if (op == ChannelModeOperation.OmniModeOff)
                    OmniMode = false;
                else if (op == ChannelModeOperation.OmniModeOn)
                    OmniMode = true;
                else if (op == ChannelModeOperation.MonoModeOn)
                    PolyMode = false;
                else if (op == ChannelModeOperation.PolyModeOn)
                    PolyMode = true;
                else if (op == ChannelModeOperation.LocalControl)
                    LocalControl = (secondByte != 0);
            }
        }
        else if (type == ChannelMessageType.PitchBend)
            PitchBend = Messages.PitchBend.ValueBytesToShort(firstByte, secondByte);
        else if (type == ChannelMessageType.ProgramChange)
            Program = (GeneralMidiProgram)firstByte;
    }

    public void Reset()
    {
        Notes.Reset();
        Array.Clear(controllerValues, 0, controllerValues.Length);
        PitchBend = 0;
        Program = default;
        OmniMode = default;
        PolyMode = default;
        LocalControl = default;
    }
}
