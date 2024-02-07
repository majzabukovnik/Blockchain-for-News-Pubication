using System.ComponentModel.DataAnnotations;

namespace News_Blockchain;

public class Miner
{
    //Dobim block, previos block hash - potegnes iz baze...
    //zracunam merkle root hash - dobim merkle root hash
    // Time - ta cas v unix (current time)
    // nbits - ce je trenutni height ni deljiv z 2016(uporabis prejsni nbits), ce je klices NewDifficulty, time difrences time - 2016 block nazaj
    //nonce - poklicem ComputeSHA256Hash input serializirana vrednost blocka, ena ponovitev 

    public Miner(Block block ,BlockDB blockDb)
    {
        BlockValidator validator = new BlockValidator();
        
        //Time
        uint nbits = block.NBits % 2016 != 0  ? block.NBits : validator.NewDifficulty(block.NBits,  block.Time  - blockDb.GetLastSpecifiedBlocks(2016).Last().Time );
        while (true)
        {
            if (validator.CheckHashDifficultyTarget(Helpers.GetBlockHash(block), block.NBits)) break;
            block.Nonce++;
        }
        // nonce++;
    }
}