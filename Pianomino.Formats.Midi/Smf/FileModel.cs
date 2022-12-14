using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pianomino.Formats.Midi.Smf;

/// <summary>
/// Represents the contents of a Standard MIDI File.
/// </summary>
public sealed partial record FileModel
{
    public readonly struct TrackEvent
    {
        public long TimeInTicks { get; init; }
        public RawEvent Event { get; init; }
    }

    public readonly struct Track
    {
        public ImmutableArray<TrackEvent> Events { get; init; }
    }

    public bool AreTracksIndependent { get; init; }
    public TimeDivision TimeDivision { get; init; }
    public ImmutableArray<Track> Tracks { get; init; }
}
