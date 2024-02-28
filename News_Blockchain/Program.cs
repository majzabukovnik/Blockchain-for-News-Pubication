using System.Net;

namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Networking net = new Networking();

        if (Console.ReadLine() == "l")
        {



            _ = net.Listen(IPAddress.Parse("192.168.0.143").GetAddressBytes());

            Console.WriteLine("dgdf");

            Console.ReadKey();
        }
        else
        {
            net.Connect(IPAddress.Parse("192.168.0.143").GetAddressBytes(), new Message());
            Console.WriteLine("yo");
            Console.ReadLine();
        }
    }
}

