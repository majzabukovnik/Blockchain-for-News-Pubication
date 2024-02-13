using System.ComponentModel.DataAnnotations;

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
    private BlockValidator _blockValidator;
    public Miner(List<Transaction> transactions,BlockDB blockDb)
    {
        BlockValidator validator = new BlockValidator();

        Block block = CreateABlock(transactions);
        uint nbits = block.NBits % 2016 != 0  ? block.NBits : validator.NewDifficulty(block.NBits,  block.Time  - blockDb.GetLastSpecifiedBlocks(2016).Last().Time );
        while (_mining)
        {
            if (validator.CheckHashDifficultyTarget(Helpers.GetBlockHash(block), block.NBits)) break;
            block.Nonce++;
        }
        
        
    }

    public Block CreateABlock(List<Transaction> transactions)
    {
        Block block = new Block(_blockDb.GetLastSpecifiedBlocks(1)[0].PreviousBlocKHeaderHash, _blockValidator.MerkleRootHash(transactions), Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds() ), 486604799 ,0, transactions);
        return block;
    }

    public void Stop()
    {
        _mining = false;
    }
}