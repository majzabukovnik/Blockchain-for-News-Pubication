using System.Net;
using EllipticCurve;

namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Write location of frontend application");
        string frontendLocation = Console.ReadLine();
        if (frontendLocation.IndexOfAny(Path.GetInvalidPathChars()) == -1)
        {
            Directory.CreateDirectory(frontendLocation + "/News");
            Directory.CreateDirectory(frontendLocation + "/Sent_Transactions");
            Directory.CreateDirectory(frontendLocation + "/Received_Transactions");
            Directory.CreateDirectory(frontendLocation + "/Sent_Form");
            
        } 
            
        BlockDB db = new BlockDB();
        UTXODB utxodb = new UTXODB();
        Web web = new Web(db, utxodb);
        
        BlockValidator.SetBlockDB(db, utxodb);
        Miner miner = new Miner(db, web);

        while (true)
        {
            string input = Console.ReadLine();
            if (input == "exit")
            {
                break;
            }
        }

        Console.WriteLine("Bye bye");

        //Networking net = new Networking(db, utxodb, web );
        //Console.WriteLine(db.GetRecordByIndex(0).MerkleRootHash);



        #region test

        //
        //
        //
        //
        // if (Console.ReadLine() == "l")
        // {
        //
        //
        //
        //     _ = net.Listen(IPAddress.Parse("192.168.0.143").GetAddressBytes());
        //
        //     Console.WriteLine("dgdf");
        //
        //     Console.ReadKey();
        //     Console.WriteLine(db.GetRecordByIndex(0));
        //     Console.ReadKey();
        // }
        // else
        // {
        //     Transacation_Input trxI = new Transacation_Input("", 0, 0, "");
        //     Transacation_Output trxO = new Transacation_Output(50, 5, new List<string>{"OP_DUP","OP_HASH160","a395d3bbd1f141d34dcde4f6832b4390d3667df4","OP_EQUALVERIFY","OP_CHECKSIG"}, 
        //         "The Wall Street Journal - 21/Feb/2024 - Lessons From a Three-Decade-Long Stock Market Disaster" );
        //     Transaction trx = new Transaction(new List<Transacation_Input>{trxI}, new List<Transacation_Output>{trxO});
        //
        //     Block b = new Block("", "5dcee163e9a795af9191586b11f7eede9a889f303440cc1c1e56e6d092047ed6", 1708546790,
        //         486604799, 5592495, 1, new List<Transaction> { trx });
        //     
        //     var msg = net.Connect(IPAddress.Parse("192.168.0.143").GetAddressBytes(), new Message(){Request = new Request(){SpecifiedBlocks = 0}}).Result;
        //     //var msg = net.Connect(IPAddress.Parse("192.168.0.143").GetAddressBytes(), new Message(){Block = new List<Block>(){b}}).Result;
        //     Console.WriteLine("yo");
        //     Console.ReadLine();
        //     Console.WriteLine(msg.GetBlock()[0].MerkleRootHash);
        //     Console.ReadLine();
        // }
        //
        //

        #endregion

    }
}

