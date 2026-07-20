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
    public void ReadString()
    {
        // ASCII: Hello
        // needs a null terminator
        var reader = new MessageReader(new byte[] { 0x05, 0x00, 0x00, 0x00, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 });
        DBusValue result = reader.Read(new DBusPrimitiveType('s'));
        Assert.Equal(new DBusString("Hello"), result);
    }

    [Fact]
    public void ReadSignature()
    {
        // ASCII: s
        var reader = new MessageReader(new byte[] { 0x01, 0x73, 0x00});
        DBusValue result = reader.Read(new DBusPrimitiveType('g'));
        Assert.Equal(new DBusSignature("s"), result);
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

    [Fact]
    public void ReadsArrayOfUInt32()
    {
        // 0x0C is the length of the array (3 elements * 4 bytes each)
        var reader = new MessageReader(new byte[] { 0x0C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00 });

        DBusValue result = reader.Read(new DBusArrayType(new DBusPrimitiveType('u')));

        var array = Assert.IsType<DBusArray>(result);
        Assert.Equal(new List<DBusValue> { new DBusUInt32(1), new DBusUInt32(2), new DBusUInt32(3) }, array.Elements);
        Assert.Equal("u", array.ElementSignature);
    }

    [Fact]
    public void ReadVariantUInt32()
    {
        // 1byte length, char u, nullterminator = 3 bytes
        //  { length-byte, 'u', NUL, padding, value-byte, value-byte, value-byte, value-byte }
        var reader = new MessageReader(new byte[] { 0x01, 0x75, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00 });
        DBusValue result = reader.Read(new DBusVariantType());

        Assert.Equal(new DBusVariant("u", new DBusUInt32(5)), result);
    }

    [Fact]
    public void ReadDictEntry()
    {
        // 4 bytes for the string "s" and 4 bytes for the uint32 value 5
        var reader = new MessageReader(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x73, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00 });
        DBusValue result = reader.Read(new DBusDictEntryType(new DBusPrimitiveType('s'), new DBusPrimitiveType('u')));

        Assert.Equal(new DBusDictEntry(new DBusString("s"), new DBusUInt32(5)), result);
    }

    [Fact]
    public void ReadArrayOfDictEntryStringVariant()
    {
        // a{sv} — one entry: key "a", value variant<u> = 7
        var reader = new MessageReader(new byte[]
        {
            0x10, 0x00, 0x00, 0x00,             // array length = 16 bytes (the spec excludes the align-8 padding that follows from this count)
            0x00, 0x00, 0x00, 0x00,             // padding to align the first dict-entry to 8
            0x01, 0x00, 0x00, 0x00, 0x61, 0x00, // key: string "a" (length=1, 'a', NUL)
            0x01, 0x75, 0x00,                   // variant signature: length=1, 'u', NUL
            0x00, 0x00, 0x00,                   // padding to align the uint32 value to 4
            0x07, 0x00, 0x00, 0x00              // value: 7
        });

        DBusValue result = reader.Read(new DBusArrayType(
            new DBusDictEntryType(new DBusPrimitiveType('s'), new DBusVariantType())));

        var array = Assert.IsType<DBusArray>(result);
        Assert.Equal(
            new List<DBusValue> { new DBusDictEntry(new DBusString("a"), new DBusVariant("u", new DBusUInt32(7))) },
            array.Elements);
        Assert.Equal("{sv}", array.ElementSignature);
    }
}