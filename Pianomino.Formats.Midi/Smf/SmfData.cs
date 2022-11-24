using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// Represents the contents of a Standard MIDI File.
/// </summary>
public sealed partial record SmfData
{
    public readonly struct Event
    {
        public long TimeInTicks { get; init; }
        public RawSmfMessage Message { get; init; }
    }

    public readonly struct Track
    {
        public ImmutableArray<Event> Events { get; init; }
    }

    public bool AreTracksIndependent { get; init; }
    public TimeDivision TimeDivision { get; init; }
    public ImmutableArray<Track> Tracks { get; init; }
}
