using System.Net;

namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Networking net = new Networking();

        net.Listen(IPAddress.Parse("192.168.137.230").GetAddressBytes());

    }
}

