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
    private BlockDB _blockDb;
    public Miner(List<Transaction> transactions,BlockDB blockDb)
    {
        Block block = CreateABlock(transactions);
        
        while (_mining)
        {
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
                break;
            }
            block.Nonce++;
        }
        
        
    }

    public Block CreateABlock(List<Transaction> transactions)
    {
        //add implementation of db
        Block block = new Block("", BlockValidator.MerkleRootHash(transactions), Convert.ToUInt32(DateTimeOffset.UtcNow
            .ToUnixTimeSeconds() ), 486604799 ,0, transactions);
        //set nbits to correct value
        //uint nbits = block.NBits % 2016 != 0  ? block.NBits : BlockValidator.NewDifficulty(block.NBits,  block.Time  - blockDb
        // .GetLastSpecifiedBlocks(2016).Last().Time );
        return block;
    }

    public void Stop()
    {
        _mining = false;
    }
}