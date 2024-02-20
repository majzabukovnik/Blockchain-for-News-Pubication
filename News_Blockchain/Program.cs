using EllipticCurve;

namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        BlockDB blockDB = new BlockDB();
        
        Miner miner = new Miner(BlockValidator.CreateBlock(new List<Transaction>()), blockDB);

    }
}

