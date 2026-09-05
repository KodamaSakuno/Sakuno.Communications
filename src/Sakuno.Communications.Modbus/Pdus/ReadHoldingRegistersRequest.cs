using System.Buffers;
using System.Buffers.Binary;

namespace Sakuno.Communications.Modbus.Pdus;

/// <summary>
/// 读保持寄存器请求 PDU
/// </summary>
public class ReadHoldingRegistersRequest : ModbusRequestPdu
{
    /// <summary>
    /// 要读取的首个寄存器起始地址
    /// </summary>
    public ushort StartingAddress { get; }
    /// <summary>
    /// 要读取的寄存器数量
    /// </summary>
    public ushort Quantity { get; }

    /// <summary>
    /// 读保持寄存器请求 PDU
    /// </summary>
    /// <param name="startingAddress">要读取的首个寄存器起始地址</param>
    /// <param name="quantity">要读取的寄存器数量</param>
    /// <exception cref="ArgumentOutOfRangeException">请求的寄存器数量不是 1~125</exception>
    public ReadHoldingRegistersRequest(ushort startingAddress, ushort quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, 125);

        StartingAddress = startingAddress;
        Quantity = quantity;
    }

    /// <summary>
    /// 把读保持寄存器请求 PDU 编码到给定的缓冲区写入器里，并通知其写入了 5 个字节
    /// </summary>
    /// <param name="writer">要被写入的缓冲区</param>
    public override void Write(IBufferWriter<byte> writer)
    {
        var buffer = writer.GetSpan(5);

        buffer[0] = (byte)FunctionCode.ReadHoldingRegisters;
        BinaryPrimitives.WriteUInt16BigEndian(buffer[1..], StartingAddress);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[3..], Quantity);

        writer.Advance(5);
    }
}
