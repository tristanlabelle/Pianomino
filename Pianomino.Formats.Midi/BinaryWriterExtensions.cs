using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

internal static class BinaryWriterExtensions
{
    public static void WriteBigEndian(this BinaryWriter writer, uint value)
    {
        if (BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        writer.Write(value);
    }

    public static void WriteBigEndian(this BinaryWriter writer, ushort value)
    {
        if (BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        writer.Write(value);
    }

    public static void WriteBigEndian(this BinaryWriter writer, int value) => WriteBigEndian(writer, unchecked((uint)value));
    public static void WriteBigEndian(this BinaryWriter writer, short value) => WriteBigEndian(writer, unchecked((ushort)value));

    public static void WriteLittleEndian(this BinaryWriter writer, uint value)
    {
        if (!BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        writer.Write(value);
    }

    public static void WriteLittleEndian(this BinaryWriter writer, ushort value)
    {
        if (!BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        writer.Write(value);
    }

    public static void WriteLittleEndian(this BinaryWriter writer, int value) => WriteLittleEndian(writer, unchecked((uint)value));
    public static void WriteLittleEndian(this BinaryWriter writer, short value) => WriteLittleEndian(writer, unchecked((ushort)value));
}
