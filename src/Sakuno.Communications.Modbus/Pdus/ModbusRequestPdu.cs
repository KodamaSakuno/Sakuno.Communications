using System.Buffers;

namespace Sakuno.Communications.Modbus.Pdus;

/// <summary>
/// Modbus 请求 PDU 基类
/// </summary>
public abstract class ModbusRequestPdu
{
    /// <summary>
    /// 把 PDU 编码到给定的缓冲区写入器里
    /// </summary>
    /// <param name="writer">要被写入的缓冲区</param>
    public abstract void Write(IBufferWriter<byte> writer);
}
