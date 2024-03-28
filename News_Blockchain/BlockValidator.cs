using System.Numerics;
//https://github.com/starkbank/ecdsa-dotnet?tab=readme-ov-file
using EllipticCurve;
using NBitcoin;

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
        private static BlockDB blockDb;
        private static UTXODB utxoDb;

        /// <summary>
        /// Function checks given block for any potential rule violations and marks it as
        /// valid in case of no violations.
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true or false</returns>
        public static bool ValidateBlock(Block block)
        {
            //TODO: check if this validator works
            //make sure this returns previous block
            Block? prevBlock = blockDb.GetRecordByIndex(block.Index - 1);

            if (prevBlock == null)
                return false;

            if (MerkleRootHash(block.Transactions) != block.MerkleRootHash)
                return false;

            if (!EvaluateCorrectnessOfBlockDifficulty(prevBlock, block))
                return false;

            if (!CheckForBlockTime(block))
                return false;

            if (!CheckIndex(prevBlock, block))
                return false;

            if (!CheckBlockSerializedSize(block))
                return false;

            if (!CheckForMatchingBlockHeader(prevBlock, block))
                return false;


            if (!CheckCoinbaseTransaction(block, block.Index))
                return false;

            for (int i = 1; i < block.Transactions.Count; i++)
            {
                if (ValidateTransaction(block.Transactions[i]) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Function check if transaction is valid
        /// </summary>
        /// <param name="trx"></param>
        /// <returns></returns>
        public static bool ValidateTransaction(Transaction trx)
        {
            //checks signatures
            string trxHash = Helpers.GetTransactionHash(trx);
            foreach (Transacation_Input ti in trx.Inputs)
            {
                if (!CheckTransactionInputSignature(ti.scriptSignature, trxHash, GetPreviousTrxScript(
                        ti.OutpointHash, ti.OutpointIndex, blockDb)[2]))
                    return false;
            }

            //bug: check if correct value is transferred (inputs - outputs) >= 0

            return true;
        }

        /// <summary>
        /// Function that sets database instance of BlockDB.
        /// MUST BE FIRST ONE TO BE CALLED
        /// </summary>
        /// <param name="db"></param>
        public static void SetBlockDB(BlockDB db, UTXODB utxodb)
        {
            blockDb = db;
            utxoDb = utxodb;
        }

        /// <summary>
        /// Function returns script from transaction output
        /// </summary>
        /// <param name="outpointHash"></param>
        /// <param name="outpointIndex"></param>
        /// <returns></returns>
        public static List<string> GetPreviousTrxScript(string outpointHash, int outpointIndex, BlockDB db)
        {
            UTXOTrans trx = utxoDb.GetRecord(outpointHash + "-" + outpointIndex);

            Block originBlock = db.GetRecord(trx.HashBlock);

            foreach (Transaction blockTrx in originBlock.Transactions)
            {
                if (Helpers.GetTransactionHash(blockTrx) == trx.HashTrans)
                {
                    return blockTrx.Outputs.ElementAt(outpointIndex).Script;
                }
            }

            return new System.Collections.Generic.List<string>();
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

            //target zmnožimo s tem številom, s čimer si zmanjšamo št. kombinacij privzete težavnosti za 16x
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
        public static bool EvaluateCorrectnessOfBlockDifficulty(Block previousBlock, Block newBlock)
        {
            if (newBlock.Index % 2016 == 0)
            {
                Block? oldBlock = blockDb.GetRecordByIndex(newBlock.Index - 2016);
                if (oldBlock == null)
                    return false;

                uint timeDiff = newBlock.Time - oldBlock.Time;

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
            return Serializator.SerializeToString(block).Length <= 1048576;
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
        public static double CalculateBaseReward(int height)
        {
            double output = 50 * Math.Pow(0.5, height / 210000);
            return 50 * Math.Pow(0.5, height / 210000);
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
                    if (ti.OutpointHash == "")
                        break;

                    UTXOTrans trx = utxoDb.GetRecord(ti.OutpointHash + "-" + ti.OutpointIndex);

                    Block originBlock = blockDb.GetRecord(trx.HashBlock);

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
            double maxCoinbaseValue = 2 * CalculateBaseReward(height) + CalculateBlockFees(block);

            return coinbaseTransaction.Outputs.ElementAt(0).Value <= maxCoinbaseValue;
        }

        /// <summary>
        /// Function checks validity of signature
        /// </summary>
        /// <param name="stringSignature"></param>
        /// <param name="trxHash"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool CheckTransactionInputSignature(string stringSignature, string trxHash, string address)
        {
            int endIndex = stringSignature.IndexOf("-----END PUBLIC KEY-----");
            PublicKey publicKey = PublicKey.fromPem(stringSignature.Substring(0, endIndex + 24));
            string signature = stringSignature.Substring(endIndex + 24);

            if (address != Helpers.ComputeRIPEMD160Hash(Helpers.ComputeSHA256Hash(publicKey.toPem(), 1)))
                return false;

            return Ecdsa.verify(trxHash, Signature.fromBase64(signature), publicKey);
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
        public static bool CheckForBlockTime(Block newBlock)
        {
            int index = newBlock.Index - 11;
            if (index < 0)
                index = 0;

            List<Block> list = blockDb.GetLastSpecifiedBlocks(index);
            uint sumTime = 0;
            foreach (Block block in list)
                sumTime += block.Time;

            uint average = sumTime / (uint)list.Count;
            uint unixTimestamp = (uint)DateTime.UtcNow.AddHours(2).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if ((average > newBlock.Time) && (newBlock.Time > unixTimestamp))
                return false;

            return true;
        }

        /// <summary>
        /// function checks if there is a UTXO stored in the database
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
        /// Function generates and appends signature to transaction inputs
        /// </summary>
        /// <param name="trx">transaction without a signature</param>
        /// <returns>transaction with signature</returns>
        public static Transaction GenerateSignature(Transaction trx)
        {
            PrivateKey privateKey = PrivateKey.fromPem(GetKeyPairs()["privateKey"]);

            Signature signature = Ecdsa.sign(Helpers.GetTransactionHash(trx), privateKey);

            foreach (Transacation_Input ti in trx.Inputs)
            {
                ti.scriptSignature = GetKeyPairs()["publicKey"] + signature.toBase64();
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
            int index = BlockDB.LastKnownBlockIndex + 1;
            //add implementation of db

            Block block = new Block(blockDb.GetRecordByIndex(index - 1).PreviousBlocKHeaderHash, "", Convert.ToUInt32(
                DateTimeOffset.UtcNow
                    .ToUnixTimeSeconds()), 486604799, 0, index, transactions); // spremen to TODO: spremen to 
            //set nbits to correct value
            //uint nbits = block.NBits % 2016 != 0  ? block.NBits : BlockValidator.NewDifficulty(block.NBits,  block.Time  - blockDb
            // .GetLastSpecifiedBlocks(2016).Last().Time );
            Transaction coinbaseTrx = GenerateCoinbaseTransaction(block);

            block.Transactions.Insert(0, coinbaseTrx);
            block.MerkleRootHash = MerkleRootHash(block.Transactions);

            return block;
        }

        /// <summary>
        /// Function creates valid transaction
        /// </summary>
        /// <param name="myUTXOs">List of my UTXOs in form trxHash-index</param>
        /// <param name="address">Recipient's address</param>
        /// <param name="value">Value of transfer</param>
        /// <param name="fee">Fee in satoshy/byte</param>
        /// <param name="text">Optional argument for news content</param>
        /// <returns></returns>
        public static Transaction CrateTransaction(string[] myUTXOs, string address, double value, int fee,
            string text = "")
        {
            double sumInputs = 0;
            List<Transacation_Input> inputs = new List<Transacation_Input>();

            for (int i = 0; i < myUTXOs.Length; i++)
            {
                UTXOTrans trx = utxoDb.GetRecord(myUTXOs[i]);
                Block originBlock = blockDb.GetRecord(trx.HashBlock);

                foreach (Transaction blockTrx in originBlock.Transactions)
                {
                    if (Helpers.GetTransactionHash(blockTrx) == trx.HashTrans)
                    {
                        sumInputs += blockTrx.Outputs[trx.Index].Value;
                        inputs.Add(new Transacation_Input(trx.HashBlock, trx.Index,
                            blockTrx.Outputs[trx.Index].ScriptLenght, ""));
                        break;
                    }
                }

                //bug: fee value is not taken into account
                if (sumInputs >= value)
                    break;
            }

            double change = sumInputs - value - fee;
            if (change < 0)
                change = 0;
            
            List<Transacation_Output> outputs = new List<Transacation_Output>
            {
                new Transacation_Output(value, 5, new List<string>
                {
                    "OP_DUP", "OP_HASH160", address, "OP_EQUALVERIFY",
                    "OP_CHECKSIG"
                }, text),
                new Transacation_Output(change, 5, new List<string>
                {
                    "OP_DUP", "OP_HASH160", address, "OP_EQUALVERIFY",
                    "OP_CHECKSIG"
                }),
            };


            return new Transaction(inputs, outputs);
        }
    }
}