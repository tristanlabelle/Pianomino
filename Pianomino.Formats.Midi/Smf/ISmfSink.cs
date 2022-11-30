using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public interface ISmfSink
{
    void Begin(SmfTrackFormat format, TimeDivision timeDivision);

    void BeginTrack();
    void AddEvent(uint timeDelta, in RawEvent message);
    void AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0);
    void AddEscapeEvent(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data);
    void AddMetaEvent(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data);
    void EndTrack();

    void End();
}

public enum SmfSinkState : byte
{
    Initial,
    BetweenTracks,
    InTrack,
    AtEndOfTrackEvent,
    Ended
}
