using System.Net;
using System.Net.Sockets;
using System.Text;

namespace News_Blockchain;

public class Networking
{
    public Networking()
    {
        
    }
    
    public async Task<string> Connect(byte[] ip)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(ip), 1201);
        
        using Socket client = new(
            ipEndPoint.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);

        await client.ConnectAsync(ipEndPoint);
        while (true)
        {
            // Send message.
            var message = "Hi";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            //serializacija celotnega objekta, ne bo stringov
            
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
            Console.WriteLine($"Socket client sent message: \"{message}\"");

            // Receive ack.
            var buffer = new byte[1_024];
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "<|ACK|>")
            {
                //Console.WriteLine(
                  //  $"Socket client received acknowledgment: \"{response}\"");
                break;
            }
        }
        client.Shutdown(SocketShutdown.Both);
        return "";
    }

    public string Listen()
    {
        
        
        return "";
    }
    
}