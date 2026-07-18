namespace BPTerm.DBus
{
    public class Message
    {
        private readonly MessageWriter _writer = new MessageWriter();
        private readonly uint _bodyLengthOffset;

        public Message(byte messageType, byte flags, uint serial, string path, string interfaceString, string memberString, string destinationString)
        {
            /*
            Bytes   Field
            0       Endianness
            1       Message type
            2       Flags
            3       Version
            4-7     Body length
            8-11    Serial
            */
            _writer.WriteByte((byte)'l');                // Endianness
            _writer.WriteByte(messageType);              // Message Type
            _writer.WriteByte(flags);                    // Flags
            _writer.WriteByte((byte)1);                  // Version
            _bodyLengthOffset = (uint)_writer.ReserveUInt32(); // Body length
            _writer.WriteUInt32(serial);                 // Serial

            int headerFieldsLengthOffset = _writer.ReserveUInt32();

            WriteObjectPathField(1, path);
            WriteStringField(2, interfaceString);
            WriteStringField(3, memberString);
            WriteStringField(6, destinationString);

            uint headerFieldsLength = _writer.Position - (uint)headerFieldsLengthOffset - 4;
            //patch
            _writer.PatchUInt32(headerFieldsLengthOffset, headerFieldsLength);
            _writer.Align(8);
            _writer.PatchUInt32((int)_bodyLengthOffset, 0);
        }

        public void WriteObjectPathField(byte code, string path)
        {
            _writer.Align(8);
            _writer.WriteByte(code);
            _writer.WriteSignature("o");
            _writer.WriteString(path);
        }

        public void WriteStringField(byte code, string value)
        {
            _writer.Align(8);
            _writer.WriteByte(code);
            _writer.WriteSignature("s");
            _writer.WriteString(value);
        }

        public byte[] ToArray()
        {
            return _writer.ToArray();
        }
    }
}