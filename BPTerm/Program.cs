using BPTerm.DBus;
class Program
{
    static void Main(string[] args)
    {
        DBusConnection connection = new DBusConnection();
        connection.Connect("/run/dbus/system_bus_socket");

        Message helloMessage = new Message(
            1,
            0,
            1,
            "/org/freedesktop/DBus",
            "org.freedesktop.DBus",
            "Hello",
            "org.freedesktop.DBus");

        connection.SendRaw(helloMessage.ToArray());

        byte[] helloMessageBytes = ReadOneFullMessage(connection);

        PrintBytes(helloMessageBytes);

        Console.WriteLine("[----------------]");

        Message getManagedObjectsMessage = new Message(
            1,
            0,
            2,
            "/" ,
            "org.freedesktop.DBus.ObjectManager",
            "GetManagedObjects",
            "org.bluez");

        connection.SendRaw(getManagedObjectsMessage.ToArray());

        byte[] objectMessageBytes = ReadOneFullMessage(connection);
        PrintBytes(objectMessageBytes);

        Console.WriteLine("[----------------]");

        byte[] anotherOne = ReadOneFullMessage(connection);
        PrintBytes(anotherOne);
    }

    public static byte[] ReadOneFullMessage(DBusConnection connection)
    {
        var header = connection.ReceiveExactly(12);

        var headerFieldsLengthBytes = connection.ReceiveExactly(4);

        uint length = BitConverter.ToUInt32(headerFieldsLengthBytes, 0);

        var headerFields = connection.ReceiveExactly((int)length);

        int totalSoFar = 12 + 4 + (int)length;
        int alignedLength = (totalSoFar + 7) & ~7;
        int pad = alignedLength - totalSoFar;

        var paddingLength = connection.ReceiveExactly(pad);

        uint bodyLength = BitConverter.ToUInt32(header, 4);

        var body = connection.ReceiveExactly((int)bodyLength);

        return header.Concat(headerFieldsLengthBytes).Concat(headerFields).Concat(paddingLength).Concat(body).ToArray();
    }

    public static void PrintBytes(byte[] bytes)
    {
        foreach (byte b in bytes)
        {
            Console.Write(b.ToString("X2") + " ");
        }
        Console.WriteLine();
    }
}


/*
List<byte> helloBuffer = new List<byte>();
helloBuffer.Add(((byte)'l'));
helloBuffer.Add(((byte)1));
helloBuffer.Add(((byte)0));
helloBuffer.Add(((byte)1));
helloBuffer.AddRange(BitConverter.GetBytes((uint)0));
helloBuffer.AddRange(BitConverter.GetBytes((uint)1));

List<byte> headerFieldsBuffer = new List<byte>();
//  4-byte length prefix, then N struct entries, each 8-byte aligned:
//    PATH        = "/org/freedesktop/DBus"
//    INTERFACE   = "org.freedesktop.DBus"
//    MEMBER      = "Hello"
//    DESTINATION = "org.freedesktop.DBus"


// PATH
//  4-byte length prefix, then N struct entries, each 8-byte aligned:
while(headerFieldsBuffer.Count % 8 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

headerFieldsBuffer.Add((byte)1); // PATH
headerFieldsBuffer.Add((byte)1); // signature length = 1
headerFieldsBuffer.Add((byte)'o'); // signature = "o"
headerFieldsBuffer.Add((byte)0); // signature nul terminator

while (headerFieldsBuffer.Count % 4 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

string path = "/org/freedesktop/DBus";
headerFieldsBuffer.AddRange(BitConverter.GetBytes((uint)path.Length));
headerFieldsBuffer.AddRange(Encoding.ASCII.GetBytes(path));
headerFieldsBuffer.Add((byte)0); // null terminator

// INTERFACE
while(headerFieldsBuffer.Count % 8 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

headerFieldsBuffer.Add((byte)2); // INTERFACE
headerFieldsBuffer.Add((byte)1); // signature length = 1
headerFieldsBuffer.Add((byte)'s'); // signature = "s"
headerFieldsBuffer.Add((byte)0); // signature nul terminator

while (headerFieldsBuffer.Count % 4 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

string interfaceString = "org.freedesktop.DBus";
headerFieldsBuffer.AddRange(BitConverter.GetBytes((uint)interfaceString.Length));
headerFieldsBuffer.AddRange(Encoding.ASCII.GetBytes(interfaceString));
headerFieldsBuffer.Add((byte)0); // null terminator

// MEMBER
while(headerFieldsBuffer.Count % 8 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

headerFieldsBuffer.Add((byte)3); // MEMBER
headerFieldsBuffer.Add((byte)1); // signature length = 1
headerFieldsBuffer.Add((byte)'s'); // signature = "s"
headerFieldsBuffer.Add((byte)0); // signature nul terminator

while (headerFieldsBuffer.Count % 4 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

string memberString = "Hello";
headerFieldsBuffer.AddRange(BitConverter.GetBytes((uint)memberString.Length));
headerFieldsBuffer.AddRange(Encoding.ASCII.GetBytes(memberString));
headerFieldsBuffer.Add((byte)0); // null terminator

// DESTINATION
while(headerFieldsBuffer.Count % 8 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

headerFieldsBuffer.Add((byte)6); // DESTINATION
headerFieldsBuffer.Add((byte)1); // signature length = 1
headerFieldsBuffer.Add((byte)'s'); // signature = "s"
headerFieldsBuffer.Add((byte)0); // signature nul terminator

while (headerFieldsBuffer.Count % 4 != 0)
{
    headerFieldsBuffer.Add((byte)0);
}

string destinationString = "org.freedesktop.DBus";
headerFieldsBuffer.AddRange(BitConverter.GetBytes((uint)destinationString.Length));
headerFieldsBuffer.AddRange(Encoding.ASCII.GetBytes(destinationString));
headerFieldsBuffer.Add((byte)0); // null terminator


uint headerFieldsLength = (uint)headerFieldsBuffer.Count;
Console.WriteLine("Header Fields Length: " + headerFieldsLength);

// append headerFieldsBuffer to helloBuffer as 4 bytes
helloBuffer.AddRange(BitConverter.GetBytes(headerFieldsLength));

helloBuffer.AddRange(headerFieldsBuffer);

// pad helloBuffer to the next 8-byte boundary
while (helloBuffer.Count % 8 != 0)
{
    helloBuffer.Add((byte)0);
}


socket.Send(helloBuffer.ToArray());

byte[] res = new byte[80];
int totalRead = 0;

while (totalRead < res.Length)
{
    int bytesRead = socket.Receive(res, totalRead, res.Length - totalRead, SocketFlags.None);

    if (bytesRead == 0)
        break;

    totalRead += bytesRead;
}

foreach (byte b in res)
{
    Console.Write(b.ToString("X2") + " ");
}

Console.WriteLine();

byte[] finalBytes = new byte[12];
int num = 0;

while (num < finalBytes.Length)
{
    int bytesRead = socket.Receive(finalBytes, num, finalBytes.Length - num, SocketFlags.None);

    if (bytesRead == 0)
    {
        break;
    }
    num += bytesRead;
}

Console.WriteLine("[[Last 12 bytes]]");
foreach (byte b in finalBytes)
{
    Console.Write(b.ToString("X2") + " ");
}

Console.WriteLine();
*/