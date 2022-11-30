using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Events;

public sealed class EndOfTrack : MetaEvent
{
    public static EndOfTrack Instance { get; } = new EndOfTrack();

    private EndOfTrack() { }

    public override MetaEventTypeByte MetaType => MetaEventTypeByte.EndOfTrack;

    public override RawEvent ToRaw(Encoding encoding) => RawEvent.EndOfTrack;
    public override string ToString() => "EndOfTrack";
}
