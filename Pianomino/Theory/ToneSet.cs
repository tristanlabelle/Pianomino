using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Pianomino.Theory;

/// <summary>
/// A set of tones within a 12-tone octave.
/// </summary>
public readonly struct ToneSet : IEquatable<ToneSet>, IReadOnlyList<ChromaticDegree>
{
    public const short ChromaticMask = 0b111_111_111_111;

    public static ToneSet Empty => default;
    public static ToneSet RootOnly => new(1);
    public static ToneSet All => new(ChromaticMask);

    private readonly short mask; // 12 bits, 1 = set

    private ToneSet(short mask)
    {
        if ((mask & ChromaticMask) != mask)
            throw new ArgumentOutOfRangeException(paramName: nameof(mask));
        this.mask = mask;
    }

    public ChromaticDegree this[int index]
    {
        get
        {
            for (int i = 0; i < 12; ++i)
            {
                if (((mask >> i) & 1) == 1)
                {
                    if (index == 0) return (ChromaticDegree)i;
                    index--;
                }
            }
            throw new ArgumentOutOfRangeException(message: null, paramName: nameof(index));
        }
    }

    public short Mask => mask;
    public int Count => Bits.CountOnes(mask);
    public bool IsEmpty => mask == 0;
    public bool IsRooted => (mask & 1) == 1;
    public bool IsHeptatonic => Count == 7;

    public bool Contains(ChromaticDegree tone) => (mask & (1 << (int)tone)) != 0;

    public int? IndexOf(ChromaticDegree value)
    {
        int i = 0;
        foreach (var tone in this)
        {
            if (tone == value) return i;
            i++;
        }
        return null;
    }

    public ToneSet With(ChromaticDegree tone)
        => new((short)((int)Mask | (1 << (int)tone)));

    public ToneSet Without(ChromaticDegree tone)
        => new((short)((int)Mask & ~(1 << (int)tone)));

    /// <summary>
    /// Gets the offset of a tone considering this set as an octave-repeating scale.
    /// </summary>
    /// <param name="index">The index of the tone, may be outside of the range of the tone set.</param>
    /// <returns>The offset from the root of the tone set.</returns>
    public int ScaleOffsetAt(int index)
    {
        if (IsEmpty) throw new ArgumentOutOfRangeException(nameof(index));
        var result = IntMath.EuclidianDivMod(index, Count);
        return result.Quotient * ChromaticDegreeEnum.Count + (int)this[result.Remainder];
    }

    public ToneSet GetInversion(int index = 1)
    {
        index = IntMath.EuclidianMod(index, Count);
        if (index == 0) return this;

        return RotateBy(-(int)this[index]);
    }

    public ToneSet RotateBy(int semiTones)
    {
        semiTones = IntMath.EuclidianMod(semiTones, ChromaticDegreeEnum.Count);
        if (semiTones == 0) return this;

        int newMask = mask;
        newMask = ((newMask >> (12 - semiTones)) | (mask << semiTones)) & ChromaticMask;
        return new((short)newMask);
    }

    public IntervalPattern ToHeptatonicIntervalPattern()
    {
        if (!IsHeptatonic) throw new InvalidOperationException();
        var intervalBuilder = ImmutableArray.CreateBuilder<Interval>(initialCapacity: 7);
        for (int i = 0; i < 12; ++i)
            if (Contains((ChromaticDegree)i))
                intervalBuilder.Add(Interval.FromDiatonicChromaticDeltas(intervalBuilder.Count, i));
        return new(intervalBuilder.MoveToImmutable());
    }

    public Enumerator GetEnumerator() => new(mask);

    public bool Equals(ToneSet other) => Mask == other.Mask;
    public override bool Equals(object? obj) => obj is ToneSet other && Equals(other);
    public override int GetHashCode() => Mask;
    public static bool Equals(ToneSet lhs, ToneSet rhs) => lhs.Equals(rhs);
    public static bool operator ==(ToneSet lhs, ToneSet rhs) => Equals(lhs, rhs);
    public static bool operator !=(ToneSet lhs, ToneSet rhs) => !Equals(lhs, rhs);

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append('[');
        foreach (var tone in this)
        {
            if (stringBuilder.Length > 1) stringBuilder.Append(' ');
            stringBuilder.Append(tone.ToString());
        }

        return stringBuilder.Append(']').ToString();
    }

    public static ToneSet FromTones(ReadOnlySpan<ChromaticDegree> tones)
    {
        ToneSet scale = default;
        foreach (var tone in tones)
            scale = scale.With(tone);
        return scale;
    }

    public static ToneSet FromTones(IEnumerable<ChromaticDegree> tones)
    {
        ToneSet scale = default;
        foreach (var tone in tones)
            scale = scale.With(tone);
        return scale;
    }

    public static ToneSet FromTones(params ChromaticDegree[] tones)
        => FromTones(tones.AsSpan());

    public static ToneSet FromMask(short mask) => new(mask);

    public static ToneSet Union(ToneSet first, ToneSet second) => new((short)(first.mask | second.mask));
    public static ToneSet Intersection(ToneSet first, ToneSet second) => new((short)(first.mask & second.mask));
    public static ToneSet Complement(ToneSet value) => new((short)(~value.mask & ChromaticMask));

    IEnumerator<ChromaticDegree> IEnumerable<ChromaticDegree>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<ChromaticDegree>
    {
        private readonly short mask;
        private int index;

        internal Enumerator(short mask)
        {
            this.mask = mask;
            this.index = -1;
        }

        public ChromaticDegree Current => (ChromaticDegree)index;

        public bool MoveNext()
        {
            while (true)
            {
                index++;
                if (index >= 12)
                {
                    index = 12;
                    return false;
                }

                if (((mask >> index) & 1) == 1)
                    return true;
            }
        }

        object IEnumerator.Current => Current;
        void IEnumerator.Reset() => index = -1;
        void IDisposable.Dispose() { }
    }

    public static class Scales
    {
        public static ToneSet Chromatic => new(ChromaticMask);
        public static ToneSet WholeTone => new(0b01_01_01_01_01_01);
        public static ToneSet WholeToneHalfTone => new(0b101_101_101_101);
        public static ToneSet HalfToneWholeTone => new(0b011_011_011_011);

        public static ToneSet MajorPentatonic => new(0b001010010101);
        public static ToneSet MinorPentatonic => new(0b010010101001);
        public static ToneSet MinorBlues => new(0b010011101001);
        public static ToneSet MajorBlues => new(0b001010011101);

        public static ToneSet Ionian => new(0b1010101_10101);
        public static ToneSet Dorian => new(0b01_1010101_101);
        public static ToneSet Phrygian => new(0b0101_1010101_1);
        public static ToneSet Lydian => new(0b10101_1010101);
        public static ToneSet Mixolydian => new(0b01_10101_10101);
        public static ToneSet Aeolian => new(0b0101_10101_101);
        public static ToneSet Locrian => new(0b010101_10101_1);

        public static ImmutableArray<ToneSet> DiatonicModes => TypeLoadExceptionWorkaround.DiatonicModes;

        public static ToneSet Major => Ionian;
        public static ToneSet Minor => Aeolian;
        public static ToneSet HarmonicMinor => new(0b1001_10101_101);

        public static ToneSet MelodicMinor { get; } = new(0b101010101_101);
        public static ToneSet DorianFlat2 { get; } = new(0b01_101010101_1);
        public static ToneSet PhrygianSharp6 => DorianFlat2;
        public static ToneSet LydianAugmented { get; } = new(0b101_101010101);
        public static ToneSet LydianDominant { get; } = new(0b01_101_1010101);
        public static ToneSet MixolydianFlat6 { get; } = new(0b0101_101_10101);
        public static ToneSet AeolianFlat5 { get; } = new(0b010101_101_101);
        public static ToneSet LocrianSharp2 => AeolianFlat5;
        public static ToneSet LocrianFlat4 { get; } = new(0b01010101_101_1);
        public static ToneSet SuperLocrian => LocrianFlat4;

        public static ImmutableArray<ToneSet> MelodicMinorModes => TypeLoadExceptionWorkaround.MelodicMinorModes;

        // Works around .NET runtime issue https://github.com/dotnet/runtime/issues/6924
        private static class TypeLoadExceptionWorkaround
        {
            public static ImmutableArray<ToneSet> DiatonicModes { get; } = ImmutableArray.Create(
                Ionian, Dorian, Phrygian, Lydian, Mixolydian, Aeolian, Locrian);
            public static ImmutableArray<ToneSet> MelodicMinorModes { get; } = ImmutableArray.Create(
                MelodicMinor, DorianFlat2, LydianAugmented, LydianDominant, MixolydianFlat6, AeolianFlat5, LocrianFlat4);
        }
    }
}
