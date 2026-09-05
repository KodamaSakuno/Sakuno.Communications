using System.Buffers;
using FluentAssertions;
using Sakuno.Communications.Modbus.Pdus;

namespace Sakuno.Communications.Modbus.Tests;

public class PduTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(125)]
    public void ReadHoldingRegistersRequest_Normal(ushort quantity)
    {
        var action = () => new ReadHoldingRegistersRequest(0, quantity);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(126)]
    [InlineData(ushort.MaxValue)]
    public void ReadHoldingRegistersRequest_QuantityNotBetween1And125(ushort quantity)
    {
        var action = () => new ReadHoldingRegistersRequest(0, quantity);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReadHoldingRegistersRequest_Write()
    {
        var writer = new ArrayBufferWriter<byte>(1024);

        new ReadHoldingRegistersRequest(1, 2).Write(writer);

        writer.WrittenCount.Should().Be(5);
        writer.WrittenSpan.Should().BeEqualTo([0x03, 0x00, 0x01, 0x00, 0x02]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(125)]
    public void ReadHoldingRegistersResponse_Normal(int registersDataLength)
    {
        var action = () => new ReadHoldingRegistersResponse(new ushort[registersDataLength]);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(126)]
    public void ReadHoldingRegistersResponse_RegistersDataLengthNotBetween1And125(int registersDataLength)
    {
        var action = () => new ReadHoldingRegistersResponse(new ushort[registersDataLength]);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReadHoldingRegistersResponse_Read()
    {
        var response = ModbusResponsePdu.Read([0x03, 0x04, 0x01, 0x02, 0x03, 0x04, 0x05]);

        var rhr = Assert.IsType<ReadHoldingRegistersResponse>(response);
        rhr.RegistersData.Should().BeEqualTo([258, 772]);
    }

    [Fact]
    public void ReadHoldingRegistersResponse_ReadWithWrongLength()
    {
        var action = () => ModbusResponsePdu.Read([0x03, 0x04, 0x01]);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FunctionCode128NotImplemented()
    {
        var action = () => ModbusResponsePdu.Read([0x80, 0x01]);

        action.Should().Throw<NotImplementedException>();
    }

    [Theory]
    [InlineData(129)]
    [InlineData(255)]
    public void ModbusExceptionPdu(byte functionCode)
    {
        var response = ModbusResponsePdu.Read([functionCode, 0x01]);

        var exception = Assert.IsType<ModbusExceptionPdu>(response);
        exception.ExceptionCode.Should().Be(ExceptionCode.IllegalFunction);
    }
}
