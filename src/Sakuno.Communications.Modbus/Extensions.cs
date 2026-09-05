using System.Buffers.Binary;

namespace Sakuno.Communications.Modbus;

internal static class Extensions
{
    public static ushort[] ToRegistersData(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % 2 is not 0)
            throw new ArgumentException("Bytes length should be even");

        var converted = new ushort[bytes.Length / 2];

        for (var i = 0; i < converted.Length; i++)
            converted[i] = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(i * 2, 2));

        return converted;
    }
}
