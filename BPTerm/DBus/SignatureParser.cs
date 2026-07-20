namespace BPTerm.DBus
{
    public static class SignatureParser
    {
        /// <summary>
        /// Parses a DBus signature and returns the corresponding type and size.
        /// </summary>
        /// <param name="signature">The DBus signature to parse.</param>
        /// <returns>A tuple containing the parsed type and its size.</returns>
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