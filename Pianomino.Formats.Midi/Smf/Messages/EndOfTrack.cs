using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class EndOfTrack : MetaMessage
{
    public static EndOfTrack Instance { get; } = new EndOfTrack();

    private EndOfTrack() { }

    public override RawSmfMessage ToRaw(Encoding encoding) => RawSmfMessage.EndOfTrack;
    public override string ToString() => "EndOfTrack";
    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.EndOfTrack;
}
