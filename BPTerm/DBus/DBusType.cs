namespace BPTerm.DBus
{
    public abstract record DBusType;

    public sealed record DBusPrimitiveType(char Code) : DBusType;
    public sealed record DBusArrayType(DBusType ElementType) : DBusType;
    public sealed record DBusStructType(List<DBusType> Members) : DBusType;
    public sealed record DBusDictEntryType(DBusType KeyType, DBusType ValueType) : DBusType;
    public sealed record DBusVariantType() : DBusType;
}