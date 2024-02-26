using System.Net;
using System.Net.Sockets;
using System.Text;

namespace News_Blockchain;

public struct Peers
{
    
}

public class Networking
{
    
    
    public Networking()
    {
        
    }
    
    public async Task<string> Connect(byte[] ip, Message msg)
    {

        try
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
                var message = Serializator.SerializeToString(msg);
                var messageBytes = Encoding.UTF8.GetBytes(message);
                //serializacija celotnega objekta, ne bo stringov
            
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                //Console.WriteLine($"Socket client sent message: \"{message}\"");

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
            return "succes";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "error";
        }
    }

    public async Task<string> Listen(byte[] ip)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(ip), 1201);
        
        using Socket listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        listener.Bind(ipEndPoint);
        listener.Listen(100);

        var handler = await listener.AcceptAsync();
        while (true)
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            Message msg = Serializator.DeserializeMessage(response);
            var eom = "<|EOM|>";
            if (response.IndexOf(eom) > -1 /* is end of message */)
            {
                Console.WriteLine(
                    $"Socket server received message: \"{response.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await handler.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                break;
            }
            // Sample output:
            //    Socket server received message: "Hi friends 👋!"
            //    Socket server sent acknowledgment: "<|ACK|>"
        }
        
        return ""; 
    }
    
}