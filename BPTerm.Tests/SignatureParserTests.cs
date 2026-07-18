using BPTerm.DBus;

namespace BPTerm.Tests;

public class SignatureParserTests
{
    [Fact]
    public void ParseStringPrimitive()
    {
        var (result, size) = SignatureParser.Parse("s");
        Assert.Equal(new DBusPrimitiveType('s'), result);
        Assert.Equal(1, size);
    }

    [Fact]
    public void ParseBytePrimitive()
    {
        var  (result, size) = SignatureParser.Parse("y");

        Assert.Equal(new DBusPrimitiveType('y'), result);
        Assert.Equal(1, size);
    }

    [Fact]
    public void ParseArrayPrimitive()
    {
        var (result, size) = SignatureParser.Parse("as");

        Assert.Equal(new DBusArrayType(new DBusPrimitiveType('s')), result);
        Assert.Equal(2, size);
    }

    [Fact]
    public void ParseVariant()
    {
        var (result, size) = SignatureParser.Parse("v");

        Assert.Equal(new DBusVariantType(), result);
        Assert.Equal(1, size);
    }

    [Fact]
    public void ParseDictEntry()
    {
        var (result, size) = SignatureParser.Parse("a{sv}");

        Assert.Equal(new DBusArrayType
                    (new DBusDictEntryType
                    (new DBusPrimitiveType('s'),
                     new DBusVariantType())), result);
        Assert.Equal(5, size);
    }

    [Fact]
    public void ParsesFullManagedObjectsSignature()
    {
        var (result, size) = SignatureParser.Parse("a{oa{sa{sv}}}");
        Console.WriteLine($"Result: {result}");

        var expected = new DBusArrayType(
            new DBusDictEntryType(
                new DBusPrimitiveType('o'),
                new DBusArrayType(
                    new DBusDictEntryType(
                        new DBusPrimitiveType('s'),
                        new DBusArrayType(
                            new DBusDictEntryType(
                                new DBusPrimitiveType('s'),
                                new DBusVariantType()))))));

        Assert.Equal(expected, result);
        Assert.Equal(13, size);
    }
}
