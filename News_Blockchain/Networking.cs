using System.Net;
using System.Net.Sockets;
using System.Text;

namespace News_Blockchain;


public class Networking
{
    public static List<string> UTXOKey = new List<string>();
    public static List<string> OwnUTXOKey = new List<string>();
    private BlockDB _blockDb;
    private UTXODB _utxodb;
    private Web _web;
    private Block lastSentBlock;
    private Transaction lastSentTransaction;
    
    public Networking(BlockDB blockDb, UTXODB utxodb, Web web)
    {
        _blockDb = blockDb;
        _utxodb = utxodb;
        _web = web;
    }
    
    public async Task<Message> Connect(byte[] ip, Message msg)
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

                if (msg.GetMessageType() == typeof(Request))
                {
                    var buffer = new byte[1_024];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    var deserialized = Serializator.DeserializeMessage(response);
                    return deserialized;
                }
                else
                {
                    // Receive ack.
                    var buffer = new byte[1_024];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    if (response == "<|ACK|>")
                    {
                        return new Message();
                    }
                }
            }
            client.Shutdown(SocketShutdown.Both);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Message();
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

        while (true)
        {
            
            var handler = await listener.AcceptAsync();
            while (true)
            {
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                Message msg = Serializator.DeserializeMessage(response);


                if (msg.GetMessageType() == typeof(Request))
                {
                    List<Block>? block = new List<Block>();
                    if (msg.Request.GetBlockIndex() == -1)
                        block.Add(_blockDb.GetLastBlock());
                    else
                        block = _blockDb.GetLastSpecifiedBlocks(msg.Request.GetBlockIndex());

                    msg.Block = block;
                    var ackMessage = Serializator.SerializeToString(msg);
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    break;
                }
                else
                {
                    if (msg.GetMessageType() == typeof(Block))
                    {
                        if (BlockValidator.ValidateBlock(msg.Block[0]))
                        {
                            if (!_blockDb.RecordExists(msg.Block[0]))
                            {
                                _blockDb.InsertNewRecord(msg.Block[0]);
                                _web.Spread(msg);

                            }
                            else if (lastSentBlock != msg.Block[0])
                            {
                                _web.Spread(msg);
                                lastSentBlock = msg.Block[0];
                            }

                        }
                    }
                    else if (msg.GetMessageType() == typeof(Transaction))
                    {
                        //TODO: validiri transakcijo ter vstavi v db in posli naprej
                        if (BlockValidator.ValidateTransaction(msg.Transaction))
                        {
                            
                            //     1.preveri list utxodb
                            //      2. preveri open trnsaction
                            //      3. ce je ni nikjer, shrani
                            //      4. preveri ce je trans namenjena tebi
                            //      v transaction output, script index 2 je address prejemnika
                            int i = 0;
                            foreach (var utxo in UTXOKey)
                            {
                                if (utxo != Helpers.GetTransactionHash(msg.Transaction) + "-" + i)
                                {
                                    UTXOKey.Add(Helpers.GetTransactionHash(msg.Transaction) + "-" + i);
                                    
                                }
                            }
                            
                            foreach (var trnx in Miner.OpenTransaction)
                            {
                                if (trnx != msg.Transaction)
                                {
                                    Miner.OpenTransaction.Add(msg.Transaction);
                                }
                            }
                            //---
                            foreach (var output in msg.Transaction.Outputs)
                            {
                                
                                
                                //output.Script[2] 
                            }
                            //---
                            if (lastSentTransaction != msg.Transaction)
                            {
                                _web.Spread(new Message(msg.Transaction));
                                lastSentTransaction = msg.Transaction;

                            }
                        }
                    }

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);

                    break;
                }
            }
        }

        return ""; 
    }
    
}

public class Web
{
    public static List<string>? Peers;
    
    private Networking _networking;
    private BlockDB _blockDb;
    
    public Web(BlockDB blockDb, UTXODB utxodb)
    {
        _networking = new Networking(blockDb, utxodb, this);
        _blockDb = blockDb;
        GetPeers("192.168.0.143");
    }

    private async Task GetPeers(string ip)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(IPAddress.Parse("86.61.79.100").GetAddressBytes()), 1804);
        
        using Socket client = new(
            ipEndPoint.AddressFamily, 
            SocketType.Stream, 
            ProtocolType.Tcp);

        await client.ConnectAsync(ipEndPoint);
        while (true)
        {
            // Send message.
            var message = ip;
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
            //Console.WriteLine($"Socket client sent message: \"{message}\"");

            // Receive ack.
            var buffer = new byte[1_024];
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            Peers = Serializator.DeserializeToListString(response);
            break;
            
        }

        client.Shutdown(SocketShutdown.Both);
    }

    public static List<string> GetRandomPeers()
    {
        List<string> peers = new List<string>();

        if (peers.Count <= 5) return Peers;

        Random rand = new Random();
        for (int i = 0; i < 5; i++)
        {
            int randomIndx = rand.Next(0, Peers.Count);
            if (!peers.Contains(Peers[randomIndx])) peers.Add(Peers[randomIndx]);
            else i--;
        }

        return peers;
    }

    public async Task Spread(Message msg)
    {
        
        foreach (var VARIABLE in GetRandomPeers())
        {
            _ = await _networking.Connect(IPAddress.Parse(VARIABLE).GetAddressBytes(), msg);
        }
    }

    public async Task<Message> ReqFromPeers(Request req)
    {
        foreach (var VARIABLE in GetRandomPeers())
        {
            var res = await _networking.Connect(IPAddress.Parse(VARIABLE).GetAddressBytes(), new Message() { Request = req });
            if (res != null) return res;
        }

        return new Message();
    }
    
}