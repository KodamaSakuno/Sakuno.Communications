namespace Sakuno.Communications.Modbus.Pdus;

/// <summary>
/// Modbus 响应 PDU 基类
/// </summary>
public abstract class ModbusResponsePdu
{
    /// <summary>
    /// 从缓冲区里读取并解析相应的 PDU
    /// </summary>
    /// <param name="data">要读取的缓冲区</param>
    /// <returns>成功解析时与 FunctionCode 字段对应的 PDU 报文类型</returns>
    /// <exception cref="ArgumentException">缓冲区长度不足</exception>
    /// <exception cref="NotImplementedException">该功能码未实现</exception>
    public static ModbusResponsePdu Read(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            throw new ArgumentException("Data does not contain enough bytes");

        var functionCode = (FunctionCode)data[0];

        switch (functionCode)
        {
            case FunctionCode.ReadHoldingRegisters:
                return ReadHoldingRegistersResponse.Parse(data[1..]);

            case > FunctionCode.MinReserved and <= FunctionCode.MaxReserved:
                if (data.Length < 2)
                    throw new ArgumentException("Data does not contain enough bytes");

                return new ModbusExceptionPdu(functionCode, (ExceptionCode)data[1]);

            default: throw new NotImplementedException();
        }
    }
}
