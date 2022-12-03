using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

partial record FileModel
{
    public sealed class BuilderSink : IFileSink
    {
        private readonly ImmutableArray<TrackEvent>.Builder eventArrayBuilder = ImmutableArray.CreateBuilder<TrackEvent>();
        private readonly ImmutableArray<Track>.Builder trackArrayBuilder = ImmutableArray.CreateBuilder<Track>();
        private TrackFormat? trackFormat; // Null = either Single or Simultaneous
        private FileModel? result;
        private long ticks;
        public FileSinkState State { get; private set; }

        public BuilderSink() { }

        public BuilderSink(TimeDivision timeDivision, bool areTracksIndependent = false)
        {
            // Will be overriden in Dispose
            result = new FileModel
            {
                TimeDivision = timeDivision,
                AreTracksIndependent = areTracksIndependent
            };
        }

        public void Begin(TimeDivision timeDivision, bool areTracksIndependent = false)
            => Begin(areTracksIndependent ? TrackFormat.Independent : null, timeDivision);

        public void Begin(TrackFormat trackFormat, TimeDivision timeDivision)
            => Begin((TrackFormat?)trackFormat, timeDivision);

        private void Begin(TrackFormat? trackFormat, TimeDivision timeDivision)
        {
            if (State != FileSinkState.Initial) throw new InvalidOperationException();

            this.trackFormat = trackFormat;
            result = new FileModel { TimeDivision = timeDivision };

            State = FileSinkState.BetweenTracks;
        }

        public void BeginTrack()
        {
            if (State != FileSinkState.BetweenTracks) throw new InvalidOperationException();
            if (trackArrayBuilder.Count == 1 && trackFormat == TrackFormat.Single)
                throw new InvalidOperationException();

            State = FileSinkState.InTrack;
            ticks = 0;
        }

        public void AddEvent(uint timeDelta, in RawEvent message)
        {
            if (State != FileSinkState.InTrack) throw new InvalidOperationException();

            ticks += timeDelta;
            if (message.IsMeta && message.GetMetaType() == MetaEventTypeByte.EndOfTrack)
            {
                State = FileSinkState.AtEndOfTrackEvent;
            }
            else
            {
                eventArrayBuilder.Add(new TrackEvent { TimeInTicks = ticks, Event = message });
            }
        }

        public void AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0)
            => AddEvent(timeDelta, RawEvent.CreateChannel(status, firstDataByte, secondDataByte));

        public void AddEscapeEvent(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data)
            => AddEvent(timeDelta, RawEvent.CreateEscape(sysExPrefix, data.ToImmutableArray()));

        public void AddMetaEvent(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
            => AddEvent(timeDelta, RawEvent.CreateMeta(type, data.ToImmutableArray()));

        public void EndTrack()
        {
            if (State is not FileSinkState.AtEndOfTrackEvent and not FileSinkState.InTrack)
                throw new InvalidOperationException();
            trackArrayBuilder.Add(new Track { Events = eventArrayBuilder.ToImmutable() });
            eventArrayBuilder.Clear();
            State = FileSinkState.BetweenTracks;
        }

        public void End()
        {
            if (State == FileSinkState.Ended) return;
            if (State != FileSinkState.BetweenTracks) EndTrack();
            State = FileSinkState.Ended;

            trackArrayBuilder.Capacity = trackArrayBuilder.Count;
            result = new FileModel
            {
                AreTracksIndependent = trackFormat == TrackFormat.Independent,
                TimeDivision = result!.TimeDivision,
                Tracks = trackArrayBuilder.MoveToImmutable()
            };

            trackArrayBuilder.Clear();
        }

        public FileModel Build()
        {
            End();
            return result ?? throw new UnreachableException();
        }
    }
}
