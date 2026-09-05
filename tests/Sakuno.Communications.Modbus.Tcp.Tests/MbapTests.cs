using System.Buffers;
using FluentAssertions;

namespace Sakuno.Communications.Modbus.Tcp.Tests;

public class MbapTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Read_BufferInsufficientSize(int bufferSize)
    {
        var action = () =>
        {
            var buffer = (stackalloc byte[bufferSize]);

            return Mbap.Read(buffer);
        };

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(0x7F)]
    public void Read_NormalHeader(ushort length)
    {
        var mbap = Mbap.Read([0x12, 0x34, 0x00, 0x00, 0x00, (byte)length, 0x00]);

        mbap.Should().NotBeNull();
        mbap.Should().Be(new Mbap(0x1234, length, 0));
    }

    [Fact]
    public void Read_ReturnNullIfNotModbusProtocol()
    {
        var mbap = Mbap.Read([0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00]);

        mbap.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Read_ThrowIfLengthIsLessThanTwo(byte length)
    {
        var action = () => Mbap.Read([0x00, 0x00, 0x00, 0x00, 0x00, length, 0x00]);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Write()
    {
        var writer = new ArrayBufferWriter<byte>(1024);

        new Mbap(0x1234, 0x7F, 0).Write(writer);

        writer.WrittenCount.Should().Be(7);
        writer.WrittenSpan.Should().BeEqualTo([0x12, 0x34, 0x00, 0x00, 0x00, 0x7F, 0x00]);
    }

    [Fact]
    public void Write_WithUnitId()
    {
        var writer = new ArrayBufferWriter<byte>(1024);

        new Mbap(0x1234, 0x7F, 1).Write(writer);

        writer.WrittenCount.Should().Be(7);
        writer.WrittenSpan.Should().BeEqualTo([0x12, 0x34, 0x00, 0x00, 0x00, 0x7F, 0x01]);
    }
}
