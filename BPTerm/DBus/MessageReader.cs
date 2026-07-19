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
                _ => throw new ArgumentException("Unsupported type")
            };
        }

        private DBusValue ReadPrimitive(DBusPrimitiveType type)
        {
            return type.Code switch
            {
                'y' => ReadByte(),
                'u' => ReadUInt32(),
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

        private DBusArray ReadArray(DBusArrayType type)
        {
            Align(4);
            int length = (int)ReadUInt32().Value;
            var elements = new List<DBusValue>();
            for (int i = 0; i < length; i++)
            {
                elements.Add(Read(type.ElementType));
            }

            string elementSignature = type.ElementType switch
            {
                DBusPrimitiveType p => p.Code.ToString(),
                _ => throw new NotSupportedException($"Element type not supported: {type.ElementType}")
            };

            return new DBusArray(elements, elementSignature);
        }

        private void Align(int boundary)
        {
            _position = (_position + boundary - 1) & ~(boundary - 1);
        }
    }
}