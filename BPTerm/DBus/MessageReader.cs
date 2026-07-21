using System.Text;

namespace BPTerm.DBus
{
    public class MessageReader
    {
        byte[] _buffer;
        int _position;

        public MessageReader(byte[] buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public DBusValue Read(DBusType type)
        {
            return type switch
            {
                DBusPrimitiveType p => ReadPrimitive(p),
                DBusArrayType a => ReadArray(a),
                DBusVariantType => ReadVariant(),
                DBusDictEntryType d => ReadDictEntry(d),
                _ => throw new ArgumentException("Unsupported type")
            };
        }

        private DBusValue ReadPrimitive(DBusPrimitiveType type)
        {
            return type.Code switch
            {
                'y' => ReadByte(),
                'u' => ReadUInt32(),
                's' => ReadString(),
                'o' => ReadObjectPath(),
                'g' => ReadSignature(),
                _ => throw new ArgumentException("Unsupported primitive type")
            };
        }

        private DBusByte ReadByte()
        {
            Align(1);
            return new DBusByte(_buffer[_position++]);
        }

        private DBusUInt32 ReadUInt32()
        {
            Align(4);
            uint value = BitConverter.ToUInt32(_buffer, _position);
            _position += 4;
            return new DBusUInt32(value);
        }

        private DBusString ReadString()
        {
            Align(4);
            int length = (int)ReadUInt32().Value;
            var bytes = new byte[length];
            Array.Copy(_buffer, _position, bytes, 0, length);
            _position += length;
            _position += 1; // Skip the null terminator
            return new DBusString(Encoding.UTF8.GetString(bytes));
        }

        private DBusObjectPath ReadObjectPath()
        {
            Align(4);
            int length = (int)ReadUInt32().Value;
            var bytes = new byte[length];
            Array.Copy(_buffer, _position, bytes, 0, length);
            _position += length;
            _position += 1; // Skip the null terminator
            return new DBusObjectPath(Encoding.UTF8.GetString(bytes));
        }

        private DBusSignature ReadSignature()
        {
            int length = ReadByte().Value;
            var bytes = new byte[length];
            Array.Copy(_buffer, _position, bytes, 0, length);
            _position += length;
            _position += 1; // Skip the null terminator
            return new DBusSignature(Encoding.UTF8.GetString(bytes));
        }

        private DBusArray ReadArray(DBusArrayType type)
        {
            Align(4);
            int length = (int)ReadUInt32().Value;
            var elements = new List<DBusValue>();

            int startPosition = _position;
            while (_position - startPosition < length)
            {
                elements.Add(Read(type.ElementType));
            }

            string elementSignature = DictSigValueHelper(type.ElementType);

            return new DBusArray(elements, elementSignature);
        }

        // Recursive helper to build dictionary signature strings
        private string DictSigValueHelper(DBusType type)
        {
            return type switch
            {
                DBusArrayType a => "a" + DictSigValueHelper(a.ElementType),
                DBusPrimitiveType p => p.Code.ToString(),
                DBusDictEntryType d => "{" + DictSigValueHelper(d.KeyType) + DictSigValueHelper(d.ValueType) + "}",
                DBusVariantType => "v",
                _ => throw new NotSupportedException($"Type not supported: {type}")
            };
        }

        private DBusVariant ReadVariant()
        {
            /*
            Example: {0x01, 0x75, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00}, starting at _position = 0.
            Read the signature
            ReadByte(); read _buffer[0] = 0x01 (Length = 1)
            _poistion advance to 1
            Coppies 1 byte starting at _position = 1: _buffer[1] = 0x75 ('u)
            _position += 1 // Skip the null terminator
            Returns DBusSignature("u")
            Position is now 3
            Pass 'u' to the signature parser
            Since it doesnt match 'a', 'v' or '{'
            Signature Parser returns (new DBusPrimitiveType(signature[0]), 1);
            
            */
            var signature = ReadSignature();
            var (elementType, _) = SignatureParser.Parse(signature.Value);
            return new DBusVariant(signature.Value, Read(elementType));
        }

        private DBusDictEntry ReadDictEntry(DBusDictEntryType d)
        {
            Align(8);
            var key = Read(d.KeyType);
            var value = Read(d.ValueType);
            return new DBusDictEntry(key, value);
        }

        private void Align(int boundary)
        {
            _position = (_position + boundary - 1) & ~(boundary - 1);
        }

    }
}