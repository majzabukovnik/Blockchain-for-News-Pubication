using EllipticCurve;

namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        BlockDB blockDB = new BlockDB();
        //blockDB.DeleteRecords("0000000a05d43f7513b1483914adafad6f25d5795480cd23b3bd782bbcb8e6a5");
        Console.WriteLine(blockDB.GetAllRecordsCount());
        Console.WriteLine(blockDB.GetLastSpecifiedBlocks(1)[0].Transactions.Last().Outputs.Last().Text);
        //Miner miner = new Miner(BlockValidator.CreateBlock(new List<Transaction>()), blockDB);

    }
}

