namespace Sakuno.Communications.Modbus;

/// <summary>
/// Modbus 功能码
/// </summary>
public enum FunctionCode
{
    /// <summary>
    /// 读保持寄存器
    /// </summary>
    ReadHoldingRegisters = 3,

    /// <summary>
    /// 最小保留值，用于异常响应
    /// </summary>
    MinReserved = 128,
    /// <summary>
    /// 最大保留值，用于异常响应
    /// </summary>
    MaxReserved = 255,
}
