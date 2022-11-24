using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

internal static class BinaryReaderExtensions
{
    public static uint ReadBigEndianUInt32(this BinaryReader reader)
    {
        uint value = reader.ReadUInt32();
        if (BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        return value;
    }

    public static ushort ReadBigEndianUInt16(this BinaryReader reader)
    {
        ushort value = reader.ReadUInt16();
        if (BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        return value;
    }

    public static int ReadBigEndianInt32(this BinaryReader reader) => unchecked((int)ReadBigEndianUInt32(reader));
    public static short ReadBigEndianInt16(this BinaryReader reader) => unchecked((short)ReadBigEndianUInt16(reader));

    public static uint ReadLittleEndianUInt32(this BinaryReader reader)
    {
        uint value = reader.ReadUInt32();
        if (!BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        return value;
    }

    public static ushort ReadLittleEndianUInt16(this BinaryReader reader)
    {
        ushort value = reader.ReadUInt16();
        if (!BitConverter.IsLittleEndian)
            value = Endianness.SwapBytes(value);
        return value;
    }

    public static int ReadLittleEndianInt32(this BinaryReader reader) => unchecked((int)ReadLittleEndianUInt32(reader));
    public static short ReadLittleEndianInt16(this BinaryReader reader) => unchecked((short)ReadLittleEndianUInt16(reader));
}
