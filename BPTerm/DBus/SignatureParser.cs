namespace BPTerm.DBus
{
    public static class SignatureParser
    {
        public static (DBusType, int) Parse(string signature)
        {
            if (signature[0] == 'a')
            {
                var (childType, childSize) = Parse(signature.Substring(1));
                return (new DBusArrayType(childType), childSize + 1);
            }

            if (signature[0] == 'v')
            {
                return (new DBusVariantType(), 1);
            }

            if (signature[0] == '{')
            {
                var (keyType, keySize) = Parse(signature.Substring(1));
                var (valueType, valueSize) = Parse(signature.Substring(keySize + 1));
                return (new DBusDictEntryType(keyType, valueType), 1 + keySize + valueSize + 1);
            }

            return (new DBusPrimitiveType(signature[0]), 1);
        }
    }
}