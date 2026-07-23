using System.Security.Permissions;

namespace BPTerm.DBus
{
    public static class HeaderFields
    {
        public const byte Path = 1;
        public const byte Interface = 2;
        public const byte Member = 3;
        public const byte ErrorName = 4;
        public const byte ReplySerial = 5;
        public const byte Destination = 6;
        public const byte Sender = 7;
        public const byte Signature = 8;
        public const byte UnixFds = 9;

        private static readonly DBusArrayType ArrayType =
        new DBusArrayType(new DBusStructType(new List<DBusType>
        {
            new DBusPrimitiveType('y'),
            new DBusVariantType()
        }));

        public static Dictionary<byte, DBusValue> Parse(byte[] arrayBytes)
        {
            var reader = new MessageReader(arrayBytes);
            var array = (DBusArray)reader.Read(ArrayType);

            var result = new Dictionary<byte, DBusValue>();
            foreach (var element in array.Elements)
            {
                var entry = (DBusStruct)element;
                var code = ((DBusByte)entry.Members[0]).Value;
                var variant = ((DBusVariant)entry.Members[1]);
                result[code] = variant.Value;
            }
            return result;
        }
    }
}