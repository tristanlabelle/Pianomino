using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public interface ILiveMessageSink
{
    void Send(RawMessage message);
    void Send(StatusByte status, ReadOnlySpan<byte> data);
}
