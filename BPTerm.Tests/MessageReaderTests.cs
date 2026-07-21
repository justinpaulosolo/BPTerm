using System.Text;
using BPTerm.DBus;

namespace BPTerm.Tests;

public class MessageReaderTests
{
    [Fact]
    public void ReadByte()
    {
        MessageReader reader = new MessageReader(new byte[] { 0x05});
        DBusValue result = reader.Read(new DBusPrimitiveType('y'));

        Assert.Equal(new DBusByte(5), result);
    }

    [Fact]
    public void ReadBoolean()
    {
        // [1, 0, 0, 0] True
        MessageReader reader = new MessageReader(new byte[] { 0x01, 0x00, 0x00, 0x00 });
        DBusValue result = reader.Read(new DBusPrimitiveType('b'));

        Assert.Equal(new DBusBool(true), result);
    }
    
    [Fact]
    public void ReadInt16()
    {
        // [5, 0] True
        MessageReader reader = new MessageReader(new byte[] { 0x05, 0x00 });
        DBusValue result = reader.Read(new DBusPrimitiveType('n'));

        Assert.Equal(new DBusInt16(5), result);
    }

    [Fact]
    public void ReadUInt16()
    {
        // [5, 0] True
        MessageReader reader = new MessageReader(new byte[] { 0x05, 0x00 });
        DBusValue result = reader.Read(new DBusPrimitiveType('q'));

        Assert.Equal(new DBusUInt16(5), result);
    }

    [Fact]
    public void ReadUInt32()
    {
        // [5, 0, 0, 0]
        UInt32 value = 5;
        byte[] valueBytes = BitConverter.GetBytes(value);

        MessageReader reader = new MessageReader(valueBytes);
        DBusValue result = reader.Read(new DBusPrimitiveType('u'));

        Assert.Equal(new DBusUInt32(value), result);
    }

    [Fact]
    public void ReadString()
    {
        // Length
        // [5, 0, 0, 0] ["Hello"] [NUL]
        string str = "Hello";
        int strLength = Encoding.UTF8.GetByteCount(str);
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        byte[] lengthBytes = BitConverter.GetBytes(strLength);
        
        byte[] messageBytes = lengthBytes
        .Concat(strBytes)
        .Concat(new byte[] { 0x00 })
        .ToArray();

        MessageReader reader = new MessageReader(messageBytes);
        DBusValue result = reader.Read(new DBusPrimitiveType('s'));
        Assert.Equal(new DBusString("Hello"), result);
    }

    [Fact]
    public void ReadObjectPath()
    {
        // Length
        // [5, 0, 0, 0] ["/org/freedesktop/DBus"] [NUL]
        string objectPath = "/org/freedesktop/DBus";
        int stringLength = Encoding.UTF8.GetByteCount(objectPath);
        byte[] objectPathBytes = Encoding.UTF8.GetBytes(objectPath);
        byte[] lengthBytes = BitConverter.GetBytes(stringLength);
        byte[] messageBytes = lengthBytes
            .Concat(objectPathBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();

        MessageReader reader = new MessageReader(messageBytes);
        DBusValue result = reader.Read(new DBusPrimitiveType('o'));
        Assert.Equal(new DBusObjectPath(objectPath), result);
    }

    [Fact]
    public void ReadSignature()
    {
        // Length
        // [1] ["s"] [NULL]
        string signature = "s";
        byte signatureLength = (byte)Encoding.UTF8.GetByteCount(signature);
        byte[] signatureBytes = Encoding.UTF8.GetBytes(signature);
        byte[] messageBytes = new byte[] { signatureLength }
            .Concat(signatureBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();

        MessageReader reader = new MessageReader(messageBytes);
        DBusValue result = reader.Read(new DBusPrimitiveType('g'));
        Assert.Equal(new DBusSignature("s"), result);
    }

    [Fact]
    public void ReadsArrayOfByte()
    {
        // Length
        // [3, 0, 0, 0] [1, 2, 3]
        int arrayLength = 3;
        byte[] arrayValues = { 1, 2, 3 };
        byte[] lengthBytes = BitConverter.GetBytes(arrayLength);

        byte[] messageBytes = lengthBytes
            .Concat(arrayValues)
            .ToArray();

        MessageReader reader = new MessageReader(messageBytes);


        DBusValue result = reader.Read(new DBusArrayType(new DBusPrimitiveType('y')));

        DBusArray array = Assert.IsType<DBusArray>(result);
        Assert.Equal(new List<DBusValue> { new DBusByte(1), new DBusByte(2), new DBusByte(3) }, array.Elements);
        Assert.Equal("y", array.ElementSignature);
    }

    [Fact]
    public void ReadsArrayOfUInt32()
    {
        // Length
        // [12, 0, 0, 0] [1, 0, 0, 0] [2, 0, 0, 0] [3, 0, 0, 0]
        uint[] arrayValues = { 1, 2, 3 };
        int arrayLength = arrayValues.Length * sizeof(uint);

        byte[] lengthBytes = BitConverter.GetBytes(arrayLength);
        byte[] elementBytes = arrayValues.SelectMany(BitConverter.GetBytes).ToArray();

        byte[] messageBytes = lengthBytes.Concat(elementBytes).ToArray();

        MessageReader reader = new MessageReader(messageBytes);

        DBusValue result = reader.Read(new DBusArrayType(new DBusPrimitiveType('u')));

        var array = Assert.IsType<DBusArray>(result);
        Assert.Equal(new List<DBusValue> { new DBusUInt32(1), new DBusUInt32(2), new DBusUInt32(3) }, array.Elements);
        Assert.Equal("u", array.ElementSignature);
    }

    [Fact]
    public void ReadVariantUInt32()
    {
        // Length
        // [1] ["u"] [NULL][0 (Padding)] [5, 0, 0, 0]
        string variantSignature = "u";
        uint value = 5;
        byte signatureLength = (byte)Encoding.UTF8.GetByteCount(variantSignature);
        byte[] signatureBytes = Encoding.UTF8.GetBytes(variantSignature);
        byte[] signatureMessageBytes = new byte[] { signatureLength }
            .Concat(signatureBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();

        // 4byte alignment before UINT32 Values
        int padding = (4 - (signatureMessageBytes.Length % 4)) % 4;
        byte[] paddingBytes = new byte[padding];

        byte[] valueBytes = BitConverter.GetBytes(value);
        byte[] messageBytes = signatureMessageBytes
            .Concat(paddingBytes)
            .Concat(valueBytes)
            .ToArray();

        MessageReader reader = new MessageReader(messageBytes);

        DBusValue result = reader.Read(new DBusVariantType());

        Assert.Equal(new DBusVariant("u", new DBusUInt32(5)), result);
    }

    [Fact]
    public void ReadDictEntry()
    {
        // Length
        // [3, 0, 0, 0] ["s", NULL, PAD, PAD] [5, 0, 0, 0]
        string key = "s";
        uint value = 5;

        byte[] keyLengthBytes = BitConverter.GetBytes(Encoding.UTF8.GetByteCount(key));
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyMessageBytes = keyLengthBytes
            .Concat(keyBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();
            
        int padding = (4 - (keyMessageBytes.Length % 4)) % 4;
        byte[] paddingBytes = new byte[padding];
        byte[] valueBytes = BitConverter.GetBytes(value);

        byte[] messageBytes = keyMessageBytes
        .Concat(paddingBytes)
        .Concat(valueBytes)
        .ToArray();

        MessageReader reader = new MessageReader(messageBytes);
        DBusValue result = reader.Read(new DBusDictEntryType(new DBusPrimitiveType('s'), new DBusPrimitiveType('u')));

        Assert.Equal(new DBusDictEntry(new DBusString("s"), new DBusUInt32(5)), result);
    }

    [Fact]
    public void ReadArrayOfDictEntryStringVariant()
    {
        // Length
        // [16, 0, 0, 0] [PAD, PAD, PAD, PAD] [1, 0, 0, 0] ["a", NULL, 1, "u"] [NULL, PAD, PAD, PAD] [7, 0, 0, 0]
        string key = "a";
        string variantSignature = "u";
        uint value = 7;

        // Dict entry (key + variant)
        byte[] keyLengthBytes = BitConverter.GetBytes(Encoding.UTF8.GetByteCount(key));
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyMessageBytes = keyLengthBytes
            .Concat(keyBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();
        
        byte signatureLength = (byte)Encoding.UTF8.GetByteCount(variantSignature);
        byte[] signatureBytes = Encoding.UTF8.GetBytes(variantSignature);
        byte[] signatureMessageBytes = new byte[] { signatureLength }
            .Concat(signatureBytes)
            .Concat(new byte[] { 0x00 })
            .ToArray();

        byte[] valueBytes = BitConverter.GetBytes(value);

        // pading before uint32 value, from the start of the dict entry
        byte[] keyAndSignature = keyMessageBytes.Concat(signatureMessageBytes).ToArray();
        int valuePadding = (4 - (keyAndSignature.Length % 4)) % 4;
        byte[] valuePaddingBytes = new byte[valuePadding];

        byte[] dictEntryBytes = keyAndSignature
            .Concat(valuePaddingBytes)
            .Concat(valueBytes)
            .ToArray();

        // Array framing
        byte[] arrayLengthBytes = BitConverter.GetBytes(dictEntryBytes.Length);

        // Padding aftrer the length prefix to align the first dict entry to 8
        int entryAlignPadding = (8 - (arrayLengthBytes.Length % 8)) % 8;
        byte[] entryAlignPaddingBytes = new byte[entryAlignPadding];

        byte[] messageBytes = arrayLengthBytes
            .Concat(entryAlignPaddingBytes)
            .Concat(dictEntryBytes)
            .ToArray();

        MessageReader reader = new MessageReader(messageBytes);

        DBusValue result = reader.Read(new DBusArrayType(
            new DBusDictEntryType(new DBusPrimitiveType('s'), new DBusVariantType())));

        var array = Assert.IsType<DBusArray>(result);
        Assert.Equal(
            new List<DBusValue> { new DBusDictEntry(new DBusString("a"), new DBusVariant("u", new DBusUInt32(7))) },
            array.Elements);
        Assert.Equal("{sv}", array.ElementSignature);
    }

    [Fact]
    public void ReadArrayOfDictEntryStringArrayOfDictEntryStringVariant()
    {
        // a{sa{sv}} one outer entry: key "x", value = a{sv} with one entry: key "n", value = variant<u> = 42
        var reader = new MessageReader(new byte[]
        {
            0x20, 0x00, 0x00, 0x00,             // outer array length = 32 bytes
            0x00, 0x00, 0x00, 0x00,             // padding to align outer dict-entry to 8
            0x01, 0x00, 0x00, 0x00, 0x78, 0x00, // outer key: string "x" (length=1, 'x', NUL)
            0x00, 0x00,                         // padding to align inner array's length field to 4
            0x10, 0x00, 0x00, 0x00,             // inner array length = 16 bytes
            0x00, 0x00, 0x00, 0x00,             // padding to align inner dict-entry to 8
            0x01, 0x00, 0x00, 0x00, 0x6E, 0x00, // inner key: string "n" (length=1, 'n', NUL)
            0x01, 0x75, 0x00,                   // inner variant signature: length=1, 'u', NUL
            0x00, 0x00, 0x00,                   // padding to align uint32 value to 4
            0x2A, 0x00, 0x00, 0x00              // value: 42
        });

        DBusValue result = reader.Read(new DBusArrayType(
            new DBusDictEntryType(new DBusPrimitiveType('s'),
                new DBusArrayType(new DBusDictEntryType(new DBusPrimitiveType('s'), new DBusVariantType())))));

        DBusArray array = Assert.IsType<DBusArray>(result);
        DBusValue entry = Assert.Single(array.Elements);
        DBusDictEntry dictEntry = Assert.IsType<DBusDictEntry>(entry);

        Assert.Equal(new DBusString("x"), dictEntry.Key);
        Assert.Equal("{sa{sv}}", array.ElementSignature);

        DBusArray innerArray = Assert.IsType<DBusArray>(dictEntry.Value);
        Assert.Equal(new List<DBusValue> { new DBusDictEntry(new DBusString("n"), new DBusVariant("u", new DBusUInt32(42))) }, innerArray.Elements);
        Assert.Equal("{sv}", innerArray.ElementSignature);
    }
}