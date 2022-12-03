using System;
using System.Collections.Generic;

namespace Pianomino.Formats.Midi.Smf;

public sealed class TrackMerger : IFileSink
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
    private readonly IFileSink? flushSink;
    private readonly SortedDictionary<Key, Event> events = new();
    private long ticks;
    private FileSinkState state;

    public TrackMerger(IFileSink? flushSink = null)
    {
        this.flushSink = flushSink;
    }

    public TrackMerger(TimeDivision timeDivision, IFileSink? flushSink = null)
        : this(flushSink)
    {
        this.TimeDivision = timeDivision;
        Begin(timeDivision);
    }

    public IReadOnlyCollection<Event> Events => events.Values;

    public void Begin(TimeDivision timeDivision)
    {
        if (state != FileSinkState.Initial) throw new InvalidOperationException();

        TimeDivision = timeDivision;
        state = FileSinkState.BetweenTracks;
    }

    public void BeginTrack()
    {
        if (state != FileSinkState.BetweenTracks) throw new InvalidOperationException();

        state = FileSinkState.InTrack;
        ticks = 0;
    }

    public void AddEvent(uint timeDelta, in RawEvent message)
    {
        if (state != FileSinkState.InTrack) throw new InvalidOperationException();

        ticks += timeDelta;

        if (message.IsMeta && message.GetMetaType() == MetaEventTypeByte.EndOfTrack)
            state = FileSinkState.AtEndOfTrackEvent;
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
        if (state is not FileSinkState.InTrack and not FileSinkState.AtEndOfTrackEvent)
            throw new InvalidOperationException();
        state = FileSinkState.BetweenTracks;
    }

    public void End()
    {
        if (state == FileSinkState.Ended) return;
        if (state != FileSinkState.BetweenTracks) EndTrack();
        state = FileSinkState.Ended;

        if (flushSink is not null)
            Feed(flushSink);
    }

    public void Feed(IFileSink sink)
    {
        if (state is not FileSinkState.BetweenTracks and not FileSinkState.Ended)
            throw new InvalidOperationException();

        sink.Begin(TrackFormat.Single, TimeDivision ?? throw new UnreachableException());
        FeedTrack(sink);
        sink.End();
    }

    public void FeedTrack(IFileSink sink)
    {
        if (state is not FileSinkState.BetweenTracks and not FileSinkState.Ended)
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

    void IFileSink.Begin(TrackFormat trackFormat, TimeDivision timeDivision) => Begin(timeDivision);
}
