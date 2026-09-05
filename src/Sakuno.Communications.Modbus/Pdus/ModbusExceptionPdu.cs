namespace Sakuno.Communications.Modbus.Pdus;

/// <summary>
/// Modbus 异常响应 PDU
/// </summary>
/// <param name="functionCode">功能码（原功能码最高位设置 1）</param>
/// <param name="exceptionCode">异常码</param>
public sealed class ModbusExceptionPdu(FunctionCode functionCode, ExceptionCode exceptionCode) : ModbusResponsePdu
{
    /// <summary>
    /// 功能码（原功能码最高位设置 1）
    /// </summary>
    public FunctionCode FunctionCode { get; } = functionCode;
    /// <summary>
    /// 异常码
    /// </summary>
    public ExceptionCode ExceptionCode { get; } = exceptionCode;
}
