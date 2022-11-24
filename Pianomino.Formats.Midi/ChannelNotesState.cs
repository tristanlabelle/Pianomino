using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pianomino.Formats.Midi;

public sealed class ChannelNotesState
{
    private readonly List<NoteOn> notesOn = new();

    public IReadOnlyCollection<NoteOn> NotesOn => notesOn;
    public int OnCount => notesOn.Count;

    public ReadOnlySpan<NoteOn> GetCurrentNotesOnAsSpan() => CollectionsMarshal.AsSpan(notesOn);

    public Velocity? GetState(NoteKey key)
        => Find(key) is int index and >= 0 ? notesOn[index].Velocity : null;

    public bool IsOn(NoteKey key, out Velocity velocity)
    {
        if (Find(key) is int index and >= 0)
        {
            velocity = notesOn[index].Velocity;
            return true;
        }
        else
        {
            velocity = default;
            return false;
        }
    }

    public bool IsOn(NoteKey key) => Find(key) >= 0;
    public bool IsOff(NoteKey key) => Find(key) < 0;

    public bool HandleOn(NoteKey key, Velocity velocity)
    {
        if (velocity.IsZero) return HandleOff(key);

        for (int i = 0; i < notesOn.Count; ++i)
        {
            var noteOn = notesOn[i];
            if (noteOn.Key >= key)
            {
                if (noteOn.Key == key)
                {
                    notesOn[i] = new(key, velocity);
                    return true;
                }
                else
                {
                    notesOn.Insert(i, new(key, velocity));
                    return true;
                }
            }
        }

        notesOn.Add(new(key, velocity));
        return true;
    }

    public bool HandleOff(NoteKey key)
    {
        int index = Find(key);
        if (index < 0) return false;
        notesOn.RemoveAt(index);
        return true;
    }

    public bool HandleAftertouch(NoteKey key, Velocity velocity)
    {
        int index = Find(key);
        if (index < 0) return false;
        notesOn[index] = new(key, velocity);
        return true;
    }

    public void Reset()
    {
        notesOn.Clear();
    }

    public bool HandleMessage(NoteMessageType type, NoteKey key, Velocity velocity)
    {
        if (type == NoteMessageType.On) return HandleOn(key, velocity);
        else if (type == NoteMessageType.Off) return HandleOff(key);
        else if (type == NoteMessageType.Aftertouch) return HandleAftertouch(key, velocity);
        else throw new ArgumentOutOfRangeException(nameof(type));
    }

    public void CopyFrom(ChannelNotesState other)
    {
        notesOn.Clear();
        notesOn.AddRange(other.notesOn);
    }

    private int Find(NoteKey key)
    {
        for (int i = 0; i < notesOn.Count; ++i)
            if (notesOn[i].Key == key)
                return i;
        return -1;
    }
}

public readonly struct NoteOn
{
    public NoteKey Key { get; }
    public Velocity Velocity { get; }

    public NoteOn(NoteKey key, Velocity velocity)
    {
        this.Key = key;
        this.Velocity = velocity;
    }
}