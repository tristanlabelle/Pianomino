using System;
using System.Collections.Generic;

namespace Pianomino.Formats.Midi.Smf;

public sealed class SmfTrackMerger : ISmfSink
{
    public readonly struct Event
    {
        public long Ticks { get; }
        public RawEvent Message { get; }

        public Event(long ticks, in RawEvent message)
        {
            this.Ticks = ticks;
            this.Message = message;
        }
    }

    private readonly struct Key : IComparable<Key>
    {
        public long Ticks { get; }
        public int ID { get; }

        public Key(long ticks, int id) => (Ticks, ID) = (ticks, id);

        public int CompareTo(Key other)
        {
            int comparison = Ticks.CompareTo(other.Ticks);
            if (comparison != 0) return comparison;
            return ID.CompareTo(other.ID);
        }
    }

    public TimeDivision? TimeDivision { get; private set; }
    private readonly ISmfSink? flushSink;
    private readonly SortedDictionary<Key, Event> events = new();
    private long ticks;
    private SmfSinkState state;

    public SmfTrackMerger(ISmfSink? flushSink = null)
    {
        this.flushSink = flushSink;
    }

    public SmfTrackMerger(TimeDivision timeDivision, ISmfSink? flushSink = null)
        : this(flushSink)
    {
        this.TimeDivision = timeDivision;
        Begin(timeDivision);
    }

    public IReadOnlyCollection<Event> Events => events.Values;

    public void Begin(TimeDivision timeDivision)
    {
        if (state != SmfSinkState.Initial) throw new InvalidOperationException();

        TimeDivision = timeDivision;
        state = SmfSinkState.BetweenTracks;
    }

    public void BeginTrack()
    {
        if (state != SmfSinkState.BetweenTracks) throw new InvalidOperationException();

        state = SmfSinkState.InTrack;
        ticks = 0;
    }

    public void AddEvent(uint timeDelta, in RawEvent message)
    {
        if (state != SmfSinkState.InTrack) throw new InvalidOperationException();

        ticks += timeDelta;

        if (message.IsMeta && message.GetMetaType() == MetaEventTypeByte.EndOfTrack)
            state = SmfSinkState.AtEndOfTrackEvent;
        else
            events.Add(new Key(ticks, events.Count), new Event(ticks, in message));
    }

    public void AddChannelEvent(uint timeDelta, StatusByte status, byte firstDataByte, byte secondDataByte = 0)
        => AddEvent(timeDelta, RawEvent.CreateChannel(status, firstDataByte, secondDataByte));

    public void AddEscapeEvent(uint timeDelta, bool sysExPrefix, ReadOnlySpan<byte> data)
        => AddEvent(timeDelta, RawEvent.CreateEscape(sysExPrefix, data.ToImmutableArray()));

    public void AddMetaEvent(uint timeDelta, MetaEventTypeByte type, ReadOnlySpan<byte> data)
        => AddEvent(timeDelta, RawEvent.CreateMeta(type, data.ToImmutableArray()));

    public void EndTrack()
    {
        if (state is not SmfSinkState.InTrack and not SmfSinkState.AtEndOfTrackEvent)
            throw new InvalidOperationException();
        state = SmfSinkState.BetweenTracks;
    }

    public void End()
    {
        if (state == SmfSinkState.Ended) return;
        if (state != SmfSinkState.BetweenTracks) EndTrack();
        state = SmfSinkState.Ended;

        if (flushSink is not null)
            Feed(flushSink);
    }

    public void Feed(ISmfSink sink)
    {
        if (state is not SmfSinkState.BetweenTracks and not SmfSinkState.Ended)
            throw new InvalidOperationException();

        sink.Begin(SmfTrackFormat.Single, TimeDivision ?? throw new UnreachableException());
        FeedTrack(sink);
        sink.End();
    }

    public void FeedTrack(ISmfSink sink)
    {
        if (state is not SmfSinkState.BetweenTracks and not SmfSinkState.Ended)
            throw new InvalidOperationException();

        sink.BeginTrack();

        long previousTicks = 0;
        foreach (var @event in Events)
        {
            uint timeDelta = checked((uint)(@event.Ticks - previousTicks));
            sink.AddEvent(timeDelta, @event.Message);
            previousTicks += timeDelta;
        }

        sink.EndTrack();
    }

    void ISmfSink.Begin(SmfTrackFormat trackFormat, TimeDivision timeDivision) => Begin(timeDivision);
}
