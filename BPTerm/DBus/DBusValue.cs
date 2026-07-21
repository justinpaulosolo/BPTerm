namespace BPTerm.DBus
{
    public abstract record DBusValue();

    public sealed record DBusByte(byte Value) : DBusValue; // 'y'
    public sealed record DBusBool(bool Value) : DBusValue; // 'b'
    public sealed record DBusInt16(short Value) : DBusValue; // 'n'
    public sealed record DBusUInt16(ushort Value) : DBusValue; // 'q'
    public sealed record DBusUInt32(uint Value) : DBusValue; // 'u'
    public sealed record DBusString(string Value) : DBusValue; // 's'
    public sealed record DBusObjectPath(string Value) : DBusValue; // 'o'
    public sealed record DBusSignature(string Value) : DBusValue; // 'g'

    public sealed record DBusArray(List<DBusValue> Elements, string ElementSignature) : DBusValue; // 'a...'
    public sealed record DBusDictEntry(DBusValue Key, DBusValue Value) : DBusValue; // '{..}'
    public sealed record DBusVariant(string Signature, DBusValue Value) : DBusValue; // 'v'
}