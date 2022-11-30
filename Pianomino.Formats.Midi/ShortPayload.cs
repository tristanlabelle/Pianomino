using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Pianomino.Formats.Midi;

/// <summary>
/// A byte buffer optimized to avoid allocations for 0, 1 and 2-byte payloads.
/// Outwardly immutable.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplayString()}")]
public struct ShortPayload
{
    public static readonly ShortPayload Empty;

    internal ImmutableArray<byte> byteArray; // May be default if lengthType != Variable
    private readonly ShortPayloadLengthType lengthType; // Defaults to ZeroBytes
    internal readonly ushort firstTwoBytes; // Least significant: first byte, most significant: second byte (redundant if byteArray not null)

    public ShortPayload(byte singleByte)
    {
        byteArray = default;
        lengthType = ShortPayloadLengthType.OneByte;
        firstTwoBytes = singleByte;
    }

    public ShortPayload(byte firstByte, byte secondByte)
    {
        byteArray = default;
        lengthType = ShortPayloadLengthType.TwoBytes;
        firstTwoBytes = (ushort)(((uint)secondByte << 8) | (uint)firstByte);
    }

    public ShortPayload(ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException();

        this.byteArray = data;

        switch (data.Length)
        {
            case 0:
                lengthType = ShortPayloadLengthType.ZeroBytes;
                firstTwoBytes = 0;
                break;

            case 1:
                lengthType = ShortPayloadLengthType.OneByte;
                firstTwoBytes = data[0];
                break;

            case 2:
                lengthType = ShortPayloadLengthType.TwoBytes;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;

            default:
                lengthType = ShortPayloadLengthType.Variable;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;
        }
    }

    public ShortPayload(ReadOnlySpan<byte> data)
    {
        switch (data.Length)
        {
            case 0:
                lengthType = ShortPayloadLengthType.ZeroBytes;
                byteArray = default;
                firstTwoBytes = 0;
                break;

            case 1:
                lengthType = ShortPayloadLengthType.OneByte;
                byteArray = default;
                firstTwoBytes = data[0];
                break;

            case 2:
                lengthType = ShortPayloadLengthType.TwoBytes;
                byteArray = default;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;

            default:
                lengthType = ShortPayloadLengthType.Variable;
                var builder = ImmutableArray.CreateBuilder<byte>(initialCapacity: data.Length);
                for (int i = 0; i < data.Length; ++i)
                    builder.Add(data[i]);
                byteArray = builder.MoveToImmutable();
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;
        }
    }

    internal ShortPayload(ShortPayloadLengthType lengthType, ushort firstTwoBytes, ImmutableArray<byte> data)
    {
        this.byteArray = data;
        this.lengthType = lengthType;
        this.firstTwoBytes = firstTwoBytes;
    }

    public int Length => lengthType == ShortPayloadLengthType.Variable ? byteArray.Length : (int)lengthType;
    public byte FirstByteOrZero => (byte)firstTwoBytes;
    public byte SecondByteOrZero => (byte)(firstTwoBytes >> 8);

    public byte this[int index]
    {
        get
        {
            if (!byteArray.IsDefault) return byteArray[index];
            if ((uint)index >= (uint)lengthType) throw new ArgumentOutOfRangeException();
            return (byte)(firstTwoBytes >> (index * 8));
        }
    }

    public ReadOnlySpan<byte> AsSpan() => ToImmutableArray().AsSpan();

    public ImmutableArray<byte> ToImmutableArray()
    {
        if (byteArray.IsDefault)
        {
            byteArray = lengthType switch
            {
                ShortPayloadLengthType.ZeroBytes => ImmutableArray<byte>.Empty,
                ShortPayloadLengthType.OneByte => ImmutableArray.Create((byte)firstTwoBytes),
                ShortPayloadLengthType.TwoBytes => ImmutableArray.Create((byte)(firstTwoBytes >> 8)),
                _ => throw new Exception() // Unreachable
            };
        }

        return byteArray;
    }

    public void CopyTo(Span<byte> buffer)
    {
        int length = Length;
        if (buffer.Length != length) throw new ArgumentException();
        switch (length)
        {
            case 0: return;
            case 1: buffer[0] = FirstByteOrZero; return;
            case 2: buffer[0] = SecondByteOrZero; return;
            default:
                for (int i = 0; i < length; ++i)
                    buffer[i] = byteArray[i];
                return;
        }
    }

    internal string GetDebuggerDisplayString()
    {
        return Length switch
        {
            0 => string.Empty,
            1 => this[0].ToString(),
            2 => $"<{this[0]}, {this[1]}>",
            int n => $"<{n} bytes>"
        };
    }

    public static implicit operator ShortPayload(ImmutableArray<byte> data) => new(data);

    public static implicit operator ImmutableArray<byte>(ShortPayload payload) => payload.ToImmutableArray();

    internal static ushort PackFirstTwoBytes(byte first, byte second)
        => (ushort)((ushort)first | ((ushort)second << 8));

    internal static ushort PackFirstTwoBytes(ImmutableArray<byte> data)
    {
        if (data.Length == 0) return 0;
        return PackFirstTwoBytes(data[0], data.Length >= 2 ? data[1] : (byte)0);
    }
}
