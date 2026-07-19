using BPTerm.DBus;

namespace BPTerm.Tests;

public class MessageReaderTests
{
    [Fact]
    public void ReadByte()
    {
        var reader = new MessageReader(new byte[] { 0x05});
        DBusValue result = reader.Read(new DBusPrimitiveType('y'));

        Assert.Equal(new DBusByte(5), result);
    }

    [Fact]
    public void ReadUInt32()
    {
        var reader = new MessageReader(new byte[] { 0x05, 0x00, 0x00, 0x00 });
        DBusValue result = reader.Read(new DBusPrimitiveType('u'));

        Assert.Equal(new DBusUInt32(5), result);
    }

    [Fact]
    public void ReadsArrayOfByte()
    {
        var reader = new MessageReader(new byte[] { 0x03, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03 });

        DBusValue result = reader.Read(new DBusArrayType(new DBusPrimitiveType('y')));

        var array = Assert.IsType<DBusArray>(result);
        Assert.Equal(new List<DBusValue> { new DBusByte(1), new DBusByte(2), new DBusByte(3) }, array.Elements);
        Assert.Equal("y", array.ElementSignature);
    }
}