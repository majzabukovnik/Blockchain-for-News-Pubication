using System.Numerics;
//https://github.com/starkbank/ecdsa-dotnet?tab=readme-ov-file
using EllipticCurve;
using System.IO;
using System.Threading.Channels;

namespace News_Blockchain
{
    static class BlockValidator
    {
        private const uint DEFAULT_NBITS = 0x1d00ffff;
        private const int TARGET_BLOCK_TIME = 10;
        private const int BLOCKS_PER_DIFFICULTY_READJUSTMENT = 2016;
        private const string FILE_WITH_KEYS = "key.txt";
        private static BigInteger currentTarget;
        private static uint currentNbits;

        /// <summary>
        /// Function checks given block for any potential rule violations and marks it as
        /// valid in case of no violations.
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true or false</returns>
        public static bool ValidateBlock(Block block)
        {
            return true;
        }

        /// <summary>
        /// Given list of transactios merkle root hash is returned.
        /// Every value is hashed twice with SHA256.
        /// In case of odd number of leaves value is hased with itself.
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns>Merkle root hash value</returns>
        public static string MerkleRootHash(List<Transaction> transactions)
        {
            List<string> hashes = new List<string>();
            foreach (Transaction t in transactions)
                hashes.Add(Helpers.GetTransactionHash(t));

            int end = hashes.Count - 1;
            while (end != 0)
            {
                for (int i = 0; i <= end; i += 2)
                {
                    if (i == end)
                    {
                        hashes[i / 2] = Helpers.ComputeSHA256Hash(hashes[i] + hashes[i]);
                    }
                    else hashes[i / 2] = Helpers.ComputeSHA256Hash(hashes[i] + hashes[i + 1]);
                }

                end /= 2;
            }

            return hashes[0];
        }

        /// <summary>
        /// Function checks if provided hash satisfies nBits requirement.
        /// Function checks if provided hash satisfies nBits requirement
        /// It is not intended to be used for mining, because target is calculated every time.
        /// </summary>
        /// <param name="headerHash"></param>
        /// <param name="nBits"></param>
        /// <returns>true or false</returns>
        public static bool CheckHashDifficultyTarget(string headerHash, uint nBits)
        {
            BigInteger target = currentTarget;
            
            if (nBits != currentNbits)
            {
                target = DecompressNbits(nBits);
                currentNbits = nBits;
            }

            BigInteger hexHashValue = BigInteger.Parse("0" + headerHash, System.Globalization.NumberStyles.HexNumber);

            //targetu prištejemo to število s čimer si zmanjšamo št kombinacij privzete težavnosti 256x
            if (hexHashValue > target * 0x10)
                return false;

            return true;
        }

        /// <summary>
        /// Function calculates difficulty, given some nBits value
        /// </summary>
        /// <param name="currentNbits"></param>
        /// <returns>difficulty</returns>
        public static uint CalculateDifficulty(uint nBits)
        {
            return (uint)(DecompressNbits(DEFAULT_NBITS) / DecompressNbits(nBits));
        }

        /// <summary>
        /// Function calculates full 256 bit value for nBits from compressed 32 bit int value
        /// </summary>
        /// <param name="nBits"></param>
        /// <returns>decompressed nBits value</returns>
        public static BigInteger DecompressNbits(uint nBits)
        {
            int significand = int.Parse(nBits.ToString("X").Substring(2), System.Globalization.NumberStyles.HexNumber);
            int exponent = int.Parse(nBits.ToString("X").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            currentTarget = significand * BigInteger.Pow(256, exponent - 3);
            return currentTarget;
        }

        /// <summary>
        /// Given nBits value of previous 14 day interval and time difference in minutes between first and
        /// last block in 14 day interval, function outputs new nBits target
        /// </summary>
        /// <param name="oldNbits"></param>
        /// <param name="timeDifference"></param>
        /// <returns>new nBits target</returns>
        public static uint NewDifficulty(uint oldNbits, uint timeDifference)
        {
            uint oldDifficulty = CalculateDifficulty(oldNbits);
            double newDifficulty = oldDifficulty * (double)(BLOCKS_PER_DIFFICULTY_READJUSTMENT * TARGET_BLOCK_TIME) /
                                   timeDifference;
            uint newNbits = (uint)(DEFAULT_NBITS / newDifficulty);

            if (newNbits > DEFAULT_NBITS)
                return DEFAULT_NBITS;
            return newNbits;
        }

        /// <summary>
        /// Function checks if new block's index is in the right order
        /// </summary>
        /// <param name="previousBlock"></param>
        /// <param name="newBlock"></param>
        /// <returns>true or false</returns>
        public static bool CheckIndex(Block previousBlock, Block newBlock)
        {
            return previousBlock.Index + 1 == newBlock.Index;
        }

        /// <summary>
        /// Function checks if provided nBits in a block are correct. It also takes into
        /// account difficulty readjustment.
        /// </summary>
        /// <param name="previousBlock"></param>
        /// <param name="newBlock"></param>
        /// <param name="blockHeight"></param>
        /// <returns>true or false</returns>
        public static bool EvaluateCorrectnessOfBlockDifficulty(Block previousBlock, Block newBlock, int blockHeight,
            BlockDB blockDB)
        {
            if (blockHeight % 2016 == 0)
            {
                uint timeDiff = newBlock.Time - blockDB.GetLastSpecifiedBlocks(newBlock.Index)[0].Time;

                if (newBlock.NBits != NewDifficulty(previousBlock.NBits, timeDiff))
                    return false;
            }

            if (previousBlock.NBits != newBlock.NBits)
                return false;

            return true;
        }

        /// <summary>
        /// Function checks if block size is equal or less than 1MB
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true or false</returns>
        public static bool CheckBlockSerializedSize(Block block)
        {
            if (Serializator.SerializeToString(block).Length >= 1024 * 1024)
                return false;

            return true;
        }

        /// <summary>
        /// Function checks if the current block has the same pbhh as the previous block header hash
        /// </summary>
        /// <param name="previousBlock"></param>
        /// <param name="currentBlock"></param>
        /// <returns>true or false</returns>
        public static bool CheckForMatchingBlockHeader(Block previousBlock, Block currentBlock)
        {
            if (currentBlock.PreviousBlocKHeaderHash != Helpers.GetBlockHash(previousBlock))
                return false;

            return true;
        }

        /// <summary>
        /// Function calculates base reward for given block height
        /// </summary>
        /// <param name="heigt"></param>
        /// <returns>base reward</returns>
        public static double CalculateBaseReward(int heigt)
        {
            return 50 * Math.Pow(0.5, heigt / 210000);
        }

        /// <summary>
        /// Function calculates blocks fees 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>fee sum</returns>
        public static double CalculateBlockFees(Block block)
        {
            double fees = 0;

            foreach (Transaction t in block.Transactions)
            {
                double inputValue = 0;
                double outputValue = 0;

                foreach (Transacation_Input ti in t.Inputs)
                {
                    UTXODB utxoDB = new UTXODB();
                    UTXOTrans trx = utxoDB.GetRecord(ti.OutpointHash + "-" + ti.OutpointIndex);

                    BlockDB blockDB = new BlockDB();
                    Block originBlock = blockDB.GetRecord(trx.HashBlock);

                    foreach (Transaction blockTrx in originBlock.Transactions)
                    {
                        if (Helpers.GetTransactionHash(blockTrx) == trx.HashTrans)
                        {
                            inputValue += blockTrx.Outputs[trx.Index].Value;
                            break;
                        }
                    }
                }

                foreach (Transacation_Output to in t.Outputs)
                {
                    outputValue += to.Value;
                }

                fees += inputValue - outputValue;
            }

            return fees;
        }

        /// <summary>
        /// Function checks if coinbase transaction is correct 
        /// </summary>
        /// <param name="block"></param>
        /// <param name="height"></param>
        /// <returns>true or false</returns>
        public static bool CheckCoinbaseTransaction(Block block, int height)
        {
            Transaction coinbaseTransaction = block.Transactions.ElementAt(0);
            double maxCoinbaseValue = CalculateBaseReward(height) + CalculateBlockFees(block);

            if (coinbaseTransaction.Outputs.ElementAt(0).Value > maxCoinbaseValue)
                return false;

            return true;
        }

        /// <summary>
        /// Function checks validity of signature
        /// </summary>
        /// <param name="stringSignature"></param>
        /// <param name="prevTrxHash"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool CheckTransactionInputSignature(string stringSignature, string prevTrxHash, string address)
        {
            int endIndex = stringSignature.IndexOf("-----END PUBLIC KEY-----");
            PublicKey publicKey = PublicKey.fromPem(stringSignature.Substring(0, endIndex + 24));
            string signature = stringSignature.Substring(endIndex + 24);

            if (address != Helpers.ComputeRIPEMD160Hash(Helpers.ComputeSHA256Hash(publicKey.toPem(), 1)))
                return false;

            return Ecdsa.verify(prevTrxHash, Signature.fromBase64(signature), publicKey);
        }

        /// <summary>
        /// Function checks if block height of a coinbase transaction is greater than 100
        /// </summary>
        /// /// <param name="block">block that contains coinbase trx</param>
        /// /// <param name="trxHeight">blocks height</param>
        /// <param name="currentHeight">current height</param>
        /// <returns>ture of false</returns>
        public static bool CoinbaseTransactionMaturity(Block block, int trxHeight, int currentHeight)
        {
            if (!CheckCoinbaseTransaction(block, trxHeight))
                return false;

            if (currentHeight - trxHeight < 100)
                return false;

            return true;
        }

        /// <summary>
        /// Function checks the average time of previous 11 blocks and if the current block is less than 2 hours
        /// </summary>
        /// <param name="blockDB"></param>
        /// <param name="newBlock"></param>
        /// <returns>true or false</returns>
        public static bool CheckForBlockTime(BlockDB blockDB, Block newBlock)
        {
            List<Block> list = blockDB.GetLastSpecifiedBlocks(11);
            uint sumTime = 0;
            foreach (Block block in list)
            {
                sumTime += block.Time;
            }

            uint average = sumTime / 11;
            uint unixTimestamp = (uint)DateTime.UtcNow.AddHours(2).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if ((average > newBlock.Time) && (newBlock.Time > unixTimestamp))
                return false;

            return true;
        }

        /// <summary>
        /// fuction checks if there is a UTXO stored in the database
        /// </summary>
        /// <returns></returns>
        public static bool CheckForDoubleSpending(UTXOTrans uTXOTrans)
        {
            string? transaction = uTXOTrans.GetKey();

            if (transaction != null)
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// this generates random PrivateKey, and using that key it generates a PublicKey
        /// </summary>
        /// <returns>public key</returns>
        public static IDictionary<string, string> GetKeyPairs()
        {
            PrivateKey privateKey = new PrivateKey();

            if (File.Exists(FILE_WITH_KEYS))
            {
                privateKey = PrivateKey.fromPem(File.ReadAllText(FILE_WITH_KEYS));
            }
            else
            {
                File.Create(FILE_WITH_KEYS).Close();
                File.WriteAllText(FILE_WITH_KEYS, privateKey.toPem());
            }

            PublicKey publicKey = privateKey.publicKey();

            return new Dictionary<string, string>
            {
                { "privateKey", privateKey.toPem() },
                { "publicKey", publicKey.toPem() }
            };
        }

        /// <summary>
        /// Function generates and append signature to transaction inputs
        /// </summary>
        /// <param name="trx">transaction without a signature</param>
        /// <returns>transaction with signature</returns>
        public static Transaction GenerateSignature(Transaction trx)
        {
            PrivateKey privateKey = PrivateKey.fromPem(GetKeyPairs()["privateKey"]);

            Signature signature = Ecdsa.sign(Helpers.GetTransactionHash(trx), privateKey);

            foreach (Transacation_Input ti in trx.Inputs)
            {
                ti.stringSignature = signature.toBase64();
            }

            return trx;
        }

        /// <summary>
        /// Function creates coinbase transaction
        /// </summary>
        /// <param name="newBlock"></param>
        /// <param name="news"></param>
        /// <returns></returns>
        public static Transaction GenerateCoinbaseTransaction(Block newBlock, string news = "")
        {
            List<string> script = new List<string>
                { "OP_DUP", "OP_HASH160", GetAddress(), "OP_EQUALVERIFY", "OP_CHECKSIG" };
            Transacation_Output to = new Transacation_Output(CalculateBaseReward(newBlock.Index) + CalculateBlockFees
                (newBlock), script.Count, script, news);

            Transacation_Input ti = new Transacation_Input("", 0, 0, "");

            return new Transaction(new List<Transacation_Input> { ti }, new List<Transacation_Output> { to });
        }

        /// <summary>
        /// Function returns your address from your public key
        /// </summary>
        /// <returns></returns>
        public static string GetAddress()
        {
            PublicKey publicKey = PublicKey.fromPem(GetKeyPairs()["publicKey"]);
            return Helpers.ComputeRIPEMD160Hash(Helpers.ComputeSHA256Hash(publicKey.toPem(), 1));
        }
        
        /// <summary>
        /// Function for creating a new block
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        public static Block CreateBlock(List<Transaction> transactions)
        {
            //add implementation of db
            Block block = new Block("", "", Convert.ToUInt32(DateTimeOffset.UtcNow
                .ToUnixTimeSeconds() ), 486604799 ,0, transactions);
            //set nbits to correct value
            //uint nbits = block.NBits % 2016 != 0  ? block.NBits : BlockValidator.NewDifficulty(block.NBits,  block.Time  - blockDb
            // .GetLastSpecifiedBlocks(2016).Last().Time );
            Transaction coinbaseTrx = GenerateCoinbaseTransaction(block);
            
            block.Transactions.Insert(0, coinbaseTrx);
            block.MerkleRootHash = MerkleRootHash(block.Transactions);

            return block;
        }
}
}