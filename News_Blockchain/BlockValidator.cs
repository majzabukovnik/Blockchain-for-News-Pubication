using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace News_Blockchain
{
    class BlockValidator
    {
        /// <summary>
        /// Function checks given block for any potential rule violations and marks it as
        /// valid in case of no violations.
        /// </summary>
        /// <param name="block"></param>
        /// <returns>true or false</returns>
        public bool ValidateBlock(Block block)
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
        private string MerkleRootHash(List<Transaction> transactions)
        {
            List<List<string>> InternalNodes = new List<List<string>>();
            int height = 0;

            InternalNodes.Add(new List<string>());
            //TODO: Add validation that first transaction in the list is coinbase transaction
            foreach (Transaction transaction in transactions)
            {
                InternalNodes[0].Add(Serializator.SerializeToString(transaction));
            }

            while (true)
            {
                InternalNodes.Add(new List<string>());
                height++;

                for(int index = 0; true; index += 2)
                {
                    if (InternalNodes[height - 1].Count == 1)
                        return InternalNodes[height - 1][0];

                    if (index + 1 == InternalNodes[height - 1].Count)
                    {
                        InternalNodes[height].Add(Helpers.ComputeSHA256Hash(
                            InternalNodes[height - 1][index] + InternalNodes[height - 1][index]));
                        break;
                    }
                    if (index == InternalNodes[height - 1].Count)
                        break;

                    InternalNodes[height].Add(Helpers.ComputeSHA256Hash(
                        InternalNodes[height - 1][index] + InternalNodes[height - 1][index + 1]));
                }
            }
        }

        /// <summary>
        /// Functions checks if provided hash satisfies nBits requirement.
        /// It is not intended to be used for mining, because target is calculated every time.
        /// </summary>
        /// <param name="headerHash"></param>
        /// <param name="nBits"></param>
        /// <returns>true or false</returns>
        private bool CheckHashDifficultyTarget(string headerHash, int nBits)
        {
            int significand = int.Parse(nBits.ToString("X").Substring(2), System.Globalization.NumberStyles.HexNumber);
            int exponent = int.Parse(nBits.ToString("X").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            BigInteger target = significand * BigInteger.Pow(256, exponent - 3);

            BigInteger HexHashValue = BigInteger.Parse(headerHash, System.Globalization.NumberStyles.HexNumber);

            if (HexHashValue > target)
                return false;

            return true;
        }
    }
}