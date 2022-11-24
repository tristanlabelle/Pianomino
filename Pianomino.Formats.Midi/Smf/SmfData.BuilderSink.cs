using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

partial record SmfData
{
    public sealed class BuilderSink : ISmfSink
    {
        private readonly ImmutableArray<Event>.Builder eventArrayBuilder = ImmutableArray.CreateBuilder<Event>();
        private readonly ImmutableArray<Track>.Builder trackArrayBuilder = ImmutableArray.CreateBuilder<Track>();
        private SmfTrackFormat? trackFormat; // Null = either Single or Simultaneous
        private SmfData? result;
        private long ticks;
        public SmfSinkState State { get; private set; }

        public BuilderSink() { }

        public BuilderSink(TimeDivision timeDivision, bool areTracksIndependent = false)
        {
            // Will be overriden in Dispose
            result = new SmfData
            {
                TimeDivision = timeDivision,
                AreTracksIndependent = areTracksIndependent
            };
        }

        public void Begin(TimeDivision timeDivision, bool areTracksIndependent = false)
            => Begin(areTracksIndependent ? SmfTrackFormat.Independent : null, timeDivision);

        public void Begin(SmfTrackFormat trackFormat, TimeDivision timeDivision)
            => Begin((SmfTrackFormat?)trackFormat, timeDivision);

        private void Begin(SmfTrackFormat? trackFormat, TimeDivision timeDivision)
        {
            if (State != SmfSinkState.Initial) throw new InvalidOperationException();

            this.trackFormat = trackFormat;
            result = new SmfData { TimeDivision = timeDivision };

            State = SmfSinkState.BetweenTracks;
        }

        public void BeginTrack()
        {
            if (State != SmfSinkState.BetweenTracks) throw new InvalidOperationException();
            if (trackArrayBuilder.Count == 1 && trackFormat == SmfTrackFormat.Single)
                throw new InvalidOperationException();

            State = SmfSinkState.InTrack;
            ticks = 0;
        }

        public void AddEvent(uint timeDelta, in RawSmfMessage message)
        {
            if (State != SmfSinkState.InTrack) throw new InvalidOperationException();

            ticks += timeDelta;
            if (message.IsMeta && message.GetMetaType() == MetaMessageTypeByte.EndOfTrack)
            {
                State = SmfSinkState.AtEndOfTrackEvent;
            }
            else
            {
                eventArrayBuilder.Add(new Event { TimeInTicks = ticks, Message = message });
            }
        }

        public void AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0)
            => AddEvent(timeDelta, RawSmfMessage.CreateChannel(status, firstDataByte, secondDataByte));

        public void AddSysExEvent(uint timeDelta, bool continuation, ReadOnlySpan<byte> data, bool terminated)
            => AddEvent(timeDelta, RawSmfMessage.CreateSystemExclusive(data.ToImmutableArray(), first: !continuation, last: terminated));

        public void AddEscapeEvent(uint timeDelta, ReadOnlySpan<byte> data)
            => AddEvent(timeDelta, RawSmfMessage.CreateEscape(data.ToImmutableArray()));

        public void AddMetaEvent(uint timeDelta, MetaMessageTypeByte type, ReadOnlySpan<byte> data)
            => AddEvent(timeDelta, RawSmfMessage.CreateMeta(type, data.ToImmutableArray()));

        public void EndTrack()
        {
            if (State is not SmfSinkState.AtEndOfTrackEvent and not SmfSinkState.InTrack)
                throw new InvalidOperationException();
            trackArrayBuilder.Add(new Track { Events = eventArrayBuilder.ToImmutable() });
            eventArrayBuilder.Clear();
            State = SmfSinkState.BetweenTracks;
        }

        public void End()
        {
            if (State == SmfSinkState.Ended) return;
            if (State != SmfSinkState.BetweenTracks) EndTrack();
            State = SmfSinkState.Ended;

            trackArrayBuilder.Capacity = trackArrayBuilder.Count;
            result = new SmfData
            {
                AreTracksIndependent = trackFormat == SmfTrackFormat.Independent,
                TimeDivision = result!.TimeDivision,
                Tracks = trackArrayBuilder.MoveToImmutable()
            };

            trackArrayBuilder.Clear();
        }

        public SmfData Build()
        {
            End();
            return result ?? throw new UnreachableException();
        }
    }
}
