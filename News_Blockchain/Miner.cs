using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

namespace News_Blockchain;

public class Miner
{
    //Dobim block, previos block hash - potegnes iz baze...
    //zracunam merkle root hash - dobim merkle root hash
    // Time - ta cas v unix (current time)
    // nbits - ce je trenutni height ni deljiv z 2016(uporabis prejsni nbits), ce je klices NewDifficulty, time difrences time - 2016 block nazaj
    //nonce - poklicem ComputeSHA256Hash input serializirana vrednost blocka, ena ponovitev 
    private bool _mining = true;
    private static List<Transaction> _openTransactions = new List<Transaction>();
    
    public static List<Transaction> OpenTransaction
    {
        get { return _openTransactions; }
        set { _openTransactions = value; }
    }
    
    public Miner(BlockDB db, Web web)
    {
        int listSize = _openTransactions.Count;
        Block block = BlockValidator.CreateBlock(_openTransactions);
        while (_mining)
        {
            if (listSize != _openTransactions.Count)
            {
                block = BlockValidator.CreateBlock(_openTransactions);
                listSize = _openTransactions.Count;
            }

            if (block.Nonce > 10000000)
            {
                Console.WriteLine(Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - block.Time));
                block.Time = Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                block.Nonce = 0;
            }
            if (BlockValidator.CheckHashDifficultyTarget(Helpers.GetBlockHash(block), block.NBits))
            {
                Console.WriteLine(Helpers.GetBlockHash(block));
                Console.WriteLine(block.Nonce);
                db.InsertNewRecord(block);
                web.Spread(new Message(new List<Block>() { block }));
                
                break;
            }
            block.Nonce++;
        }
    }

    public void Stop()
    {
        _mining = false;
    }
}