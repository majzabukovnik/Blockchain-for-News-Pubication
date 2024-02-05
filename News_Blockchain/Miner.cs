namespace News_Blockchain;

public class Miner
{
    //Dobim block, previos block hash - potegnes iz baze...
    //zracunam merkle root hash - dobim merkle root hash
    // Time - ta cas v unix (current time)
    // nbits - ce je trenutni height ni deljiv z 2016(uporabis prejsni nbits), ce je klices NewDifficulty, time difrences time - 2016 block nazaj
    //nonce - poklicem ComputeSHA256Hash input serializirana vrednost blocka, ena ponovitev 

    public Miner(Block block, string pbh ,BlockDB blockDb)
    {
        BlockValidator validator = new BlockValidator();
        validator.MerkleRootHash(block.Transactions);
        //Time
        uint nbits = block.NBits / 2016 ? block.NBits : validator.NewDifficulty(block.NBits, block.Time /* ni prau */);
        nonce++; 
    }
}