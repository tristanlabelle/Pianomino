﻿using Pianomino.Theory;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Formats.Midi.Smf.Messages;

public sealed class KeySignature : MetaMessage
{
    public const int DataLength = 2;

    public DiatonicKey DiatonicKey { get; }

    public KeySignature(DiatonicKey key) => this.DiatonicKey = key;

    public int SharpCount => DiatonicKey.SharpsInSignature;
    public MajorOrMinor MajorOrMinor => DiatonicKey.MajorOrMinor;
    public bool IsMajor => DiatonicKey.IsMajor;
    public bool IsMinor => DiatonicKey.IsMinor;

    private string SharpsString => DiatonicKey.SharpsInSignature switch
    {
        0 => "0",
        < 0 => -DiatonicKey.SharpsInSignature + "b",
        > 0 => DiatonicKey.SharpsInSignature + "#"
    };

    protected override MetaMessageTypeByte GetMetaEventType() => MetaMessageTypeByte.KeySignature;

    public override RawSmfMessage ToRaw(Encoding encoding)
        => RawSmfMessage.CreateMeta(MetaMessageTypeByte.KeySignature,
            ImmutableArray.Create((byte)(sbyte)SharpCount, IsMajor ? (byte)0 : (byte)1));

    public override string ToString() => $"KeySignature({SharpsString}, {(IsMinor ? "Minor" : "Major")})";

    public static KeySignature FromBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length != 2) throw new FormatException();
        return FromBytes(data[0], data[1]);
    }

    public static KeySignature FromBytes(byte sf, byte mi)
    {
        var majorOrMinor = mi switch
        {
            0 => MajorOrMinor.Major,
            1 => MajorOrMinor.Minor,
            _ => throw new ArgumentOutOfRangeException(nameof(mi))
        };

        return new(DiatonicKey.FromSharpsInSignature(sf, majorOrMinor));
    }
}
