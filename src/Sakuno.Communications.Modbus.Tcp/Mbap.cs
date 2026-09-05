using System.Buffers;
using System.Buffers.Binary;

namespace Sakuno.Communications.Modbus.Tcp;

/// <summary>
/// Modbus TCP 的 ADU 头，7 字节定长
/// </summary>
public readonly struct Mbap
{
    /// <summary>用于客户端匹配相应的请求与响应，在服务端原样回显该值</summary>
    public ushort TransactionId { get; }

    /// <summary>单元标识长度 + PDU 长度，不包括前面的事务标识和协议标识</summary>
    public ushort Length { get; }

    /// <summary>用于标识串行子网上的远程从站，在以太网中忽略该值</summary>
    public byte UnitId { get; }

    /// <summary>
    /// Modbus TCP 的 ADU 头，7 字节定长
    /// </summary>
    /// <param name="transactionId">用于客户端匹配相应的请求与响应，在服务端原样回显该值</param>
    /// <param name="length">单元标识长度 + PDU 长度，不包括前面的事务标识和协议标识</param>
    /// <param name="unitId">用于标识串行子网上的远程从站，在以太网中忽略该值</param>
    public Mbap(ushort transactionId, ushort length, byte unitId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 2);

        TransactionId = transactionId;
        Length = length;
        UnitId = unitId;
    }

    /// <summary>
    /// 从缓冲区里读取并解析相应的 MBAP 报文头结构
    /// </summary>
    /// <param name="data">一个至少 7 字节长度的缓冲区</param>
    /// <returns>成功解析的 MBAP 报文头；如果协议标识不为 0 返回 null</returns>
    /// <exception cref="ArgumentException">缓冲区长度小于 7 字节，或者长度字段小于 2</exception>
    public static Mbap? Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < 7)
            throw new ArgumentException("给定的缓冲区长度小于 7", nameof(data));

        var transactionId = BinaryPrimitives.ReadUInt16BigEndian(data);
        var protocolId = BinaryPrimitives.ReadUInt16BigEndian(data[2..]);
        if (protocolId is not 0)
            return null;

        var length = BinaryPrimitives.ReadUInt16BigEndian(data[4..]);
        if (length is < 2)
            throw new ArgumentException("长度字段必须大于等于 2");

        var unitId = data[6];

        return new(transactionId, length, unitId);
    }

    /// <summary>
    /// 把 MBAP 编码到给定的缓冲区写入器里，并通知其写入了 7 个字节
    /// </summary>
    /// <param name="writer">要被写入的缓冲区</param>
    public void Write(IBufferWriter<byte> writer)
    {
        var buffer = writer.GetSpan(7);

        BinaryPrimitives.WriteUInt16BigEndian(buffer, TransactionId);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], 0);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[4..], Length);

        buffer[6] = UnitId;

        writer.Advance(7);
    }
}
