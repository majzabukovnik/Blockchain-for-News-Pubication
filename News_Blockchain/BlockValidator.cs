using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Blockchain
{
    class BlockValidator
    {
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
    }
}