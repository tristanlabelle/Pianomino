using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

/// <summary>
/// Implements the logic for tracking the midi running status.
/// </summary>
public struct RunningStatus
{
    private StatusByte current;

    public bool IsValid => current != 0;

    public StatusByte? Current
    {
        get => current == 0 ? null : current;
        set
        {
            if (value.HasValue && !value.Value.IsChannelMessage()) throw new ArgumentOutOfRangeException();
            this.current = value.GetValueOrDefault();
        }
    }

    public void Clear() => current = 0;

    public void OnNewStatus(StatusByte value)
    {
        if (!value.IsValid()) throw new ArgumentOutOfRangeException();

        if (value.IsChannelMessage())
        {
            this.current = value;
        }
        else if (!value.IsSystemRealTimeMessage())
        {
            // "Real-Time messages should not affect Running Status."
            // "Buffer is cleared when a System Exclusive or Common status message is received."
            this.current = 0;
        }
        else if (value == StatusByte.Reset)
        {
            // "Real-Time messages should not affect Running Status."
            // If System Reset is recognized, the following operations should be carried out:
            // ...
            // 6) Clear Running Status
            this.current = 0;
        }
    }
}
