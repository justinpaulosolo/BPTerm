using System.Text;

namespace BPTerm.DBus
{
    public class MessageWriter
    {
        List<byte> _buffer = new List<byte>();

        public void Align(int boundary)
        {
            int currentOffset = _buffer.Count;
            int padding = (boundary - (currentOffset % boundary)) % boundary;
            _buffer.AddRange(new byte[padding]);
        }

        public void WriteByte(byte busByte)
        {
            Align(1);
            _buffer.Add(busByte);
        }

        public int ReserveUInt32()
        {
            Align(4);
            int offset = _buffer.Count;
            _buffer.AddRange(new byte[4]);
            return offset;
        }

        // WriteUInt16 WriteUInt32 WriteString WriteObjectPath

        public void WriteUInt16(ushort value)
        {
            Align(2);
            _buffer.Add((byte)(value & 0xFF));
            _buffer.Add((byte)((value >> 8) & 0xFF));
        }

        public void WriteUInt32(uint value)
        {
            Align(4);
            _buffer.Add((byte)(value & 0xFF));
            _buffer.Add((byte)((value >> 8) & 0xFF));
            _buffer.Add((byte)((value >> 16) & 0xFF));
            _buffer.Add((byte)((value >> 24) & 0xFF));
        }

        public void WriteString(string value)
        {
            Align(4);
            _buffer.AddRange(BitConverter.GetBytes((uint)value.Length));
            _buffer.AddRange(Encoding.ASCII.GetBytes(value));
            _buffer.Add(0); // Null terminator
        }

        public void WriteSignature(string value)
        {
            Align(1);
            _buffer.Add((byte)(value.Length));
            _buffer.AddRange(Encoding.ASCII.GetBytes(value));
            _buffer.Add(0); // Null terminator
        }

        public void WriteObjectPath(string value)
        {
            WriteString(value);
        }

        public void PatchUInt32(int offset, uint value)
        {
            _buffer[offset] = (byte)(value & 0xFF);
            _buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            _buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            _buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        // WriteHeaderField Helper
        public void WriteHeaderField(byte field, string value)
        {
            Align(8);
            WriteByte(field);
            WriteSignature(value);
        }

        public uint Position
        {
            get { return (uint)_buffer.Count; }
        }

        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }
    }
}