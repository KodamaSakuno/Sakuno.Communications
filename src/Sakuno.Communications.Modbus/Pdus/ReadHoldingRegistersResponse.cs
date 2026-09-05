namespace Sakuno.Communications.Modbus.Pdus;

/// <summary>
/// 读保持寄存器响应 PDU
/// </summary>
public class ReadHoldingRegistersResponse : ModbusResponsePdu
{
    /// <summary>
    /// 寄存器数据
    /// </summary>
    public IReadOnlyList<ushort> RegistersData { get; }

    /// <summary>
    /// 读保持寄存器响应 PDU
    /// </summary>
    /// <param name="registersData">原始寄存器数据</param>
    /// <exception cref="ArgumentOutOfRangeException">寄存器数据长度不是 1~125</exception>
    public ReadHoldingRegistersResponse(ushort[] registersData)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(registersData.Length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(registersData.Length, 125);

        RegistersData = registersData.AsReadOnly();
    }

    internal static ReadHoldingRegistersResponse Parse(ReadOnlySpan<byte> data)
    {
        var byteCount = data[0];

        if (data.Length < byteCount + 1)
            throw new ArgumentException("Data does not contain enough bytes");

        return new(data.Slice(1, byteCount).ToRegistersData());
    }
}
