using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

/// <summary>
/// A byte buffer optimized to avoid allocations for 0, 1 and 2-byte payloads.
/// Outwardly immutable.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplayString()}")]
public struct MessageData
{
    public static readonly MessageData Empty;

    internal ImmutableArray<byte> byteArray; // May be default if lengthType != Variable
    private readonly MessageDataLengthType lengthType; // Defaults to ZeroBytes
    internal readonly ushort firstTwoBytes; // Least significant: first byte, most significant: second byte (redundant if byteArray not null)

    public MessageData(byte firstByte)
    {
        byteArray = default;
        lengthType = MessageDataLengthType.OneByte;
        firstTwoBytes = firstByte;
    }

    public MessageData(byte firstByte, byte secondByte)
    {
        byteArray = default;
        lengthType = MessageDataLengthType.TwoBytes;
        firstTwoBytes = (ushort)(((uint)secondByte << 8) | (uint)firstByte);
    }

    public MessageData(ImmutableArray<byte> data)
    {
        if (data.IsDefault) throw new ArgumentException();

        this.byteArray = data;

        switch (data.Length)
        {
            case 0:
                lengthType = MessageDataLengthType.ZeroBytes;
                firstTwoBytes = 0;
                break;

            case 1:
                lengthType = MessageDataLengthType.OneByte;
                firstTwoBytes = data[0];
                break;

            case 2:
                lengthType = MessageDataLengthType.TwoBytes;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;

            default:
                lengthType = MessageDataLengthType.Variable;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;
        }
    }

    public MessageData(ReadOnlySpan<byte> data)
    {
        switch (data.Length)
        {
            case 0:
                lengthType = MessageDataLengthType.ZeroBytes;
                byteArray = default;
                firstTwoBytes = 0;
                break;

            case 1:
                lengthType = MessageDataLengthType.OneByte;
                byteArray = default;
                firstTwoBytes = data[0];
                break;

            case 2:
                lengthType = MessageDataLengthType.TwoBytes;
                byteArray = default;
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;

            default:
                lengthType = MessageDataLengthType.Variable;
                var builder = ImmutableArray.CreateBuilder<byte>(initialCapacity: data.Length);
                for (int i = 0; i < data.Length; ++i)
                    builder.Add(data[i]);
                byteArray = builder.MoveToImmutable();
                firstTwoBytes = (ushort)(((uint)data[1] << 8) | (uint)data[0]);
                break;
        }
    }

    internal MessageData(MessageDataLengthType lengthType, ushort firstTwoBytes, ImmutableArray<byte> data)
    {
        this.byteArray = data;
        this.lengthType = lengthType;
        this.firstTwoBytes = firstTwoBytes;
    }

    public int Length => lengthType == MessageDataLengthType.Variable ? byteArray.Length : (int)lengthType;
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
                MessageDataLengthType.ZeroBytes => ImmutableArray<byte>.Empty,
                MessageDataLengthType.OneByte => ImmutableArray.Create((byte)firstTwoBytes),
                MessageDataLengthType.TwoBytes => ImmutableArray.Create((byte)(firstTwoBytes >> 8)),
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

    public static implicit operator MessageData(ImmutableArray<byte> data) => new(data);

    internal static ushort PackFirstTwoBytes(byte first, byte second)
        => (ushort)((ushort)first | ((ushort)second << 8));

    internal static ushort PackFirstTwoBytes(ImmutableArray<byte> data)
    {
        if (data.Length == 0) return 0;
        return PackFirstTwoBytes(data[0], data.Length >= 2 ? data[1] : (byte)0);
    }
}
