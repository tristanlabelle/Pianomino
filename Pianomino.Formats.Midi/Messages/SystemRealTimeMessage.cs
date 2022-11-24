using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public sealed class SystemRealTimeMessage : Message
{
    private static readonly SystemRealTimeMessage[] instances = new[]
    {
            new SystemRealTimeMessage(StatusByte.Clock),
            new SystemRealTimeMessage(StatusByte.UndefinedF9),
            new SystemRealTimeMessage(StatusByte.Start),
            new SystemRealTimeMessage(StatusByte.Continue),
            new SystemRealTimeMessage(StatusByte.Stop),
            new SystemRealTimeMessage(StatusByte.UndefinedFD),
            new SystemRealTimeMessage(StatusByte.ActiveSensing),
            new SystemRealTimeMessage(StatusByte.Reset),
        };

    public override StatusByte Status { get; }

    private SystemRealTimeMessage(StatusByte status) => this.Status = status;

    public override RawMessage ToRaw(Encoding encoding) => RawMessage.Create(Status);
    public override string ToString() => Status == StatusByte.Reset ? "Reset" : Status.ToString();

    public static SystemRealTimeMessage Get(StatusByte status)
    {
        if (!status.IsSystemRealTimeMessage()) throw new ArgumentOutOfRangeException();
        return instances[(int)status - (int)StatusByte.Clock];
    }
}
