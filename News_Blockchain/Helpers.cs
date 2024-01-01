using System;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace News_Blockchain
{
	public static class Serializator
	{
		///<summary>
		///Serializes a block to a string
		///</summary>
		///<param name="block"> The block to convert </param>
		///<returns> The converted string </returns>
		public static string SerializeToString(Block block)
		{
			return JsonSerializer.Serialize(block); ;
		}

        ///<summary>
        ///Serializes a transaction to a string
        ///</summary>
        ///<param name="transaction"> The transaction to convert </param>
        ///<returns> The converted string </returns>
        public static string SerializeToString(Transaction transaction)
        {
            return JsonSerializer.Serialize(transaction);
        }

        ///<summary>
        ///Deserializes a Block to a string
        ///</summary>
        ///<param name="block"> The string to convert to a block </param>
        ///<returns> The converted block </returns>
        public static Block DeserializeToBlock(string block)
		{
			return JsonSerializer.Deserialize<Block>(block);
		}

        ///<summary>
        ///Serializes a string to a transaction
        ///</summary>
        ///<param name="transaction"> The string to convert to a transaction </param>
        ///<returns> The converted transaction </returns>
        public static Transaction DeserializeToTransaction(string transaction)
		{
            return JsonSerializer.Deserialize<Transaction>(transaction);

        }
    }

    public static class Helpers
    {
	    /// <summary>
	    /// Function returns twice hashed SHA256 value of given block
	    /// </summary>
	    /// <param name="block"></param>
	    /// <returns>SHA256 hash</returns>
	    public static string GetBlockHash(Block block)
	    {
		    string dataToHash = block.PreviousBlocKHeaderHash + block.MerkleRootHash + block.Time + block.NBits +
		                        block.Nonce;
		    return Helpers.ComputeSHA256Hash(dataToHash);
	    }

	    /// <summary>
	    /// Function returns twice hashed SHA256 value of given transaction.
	    /// Function's output can be used for signing transaction's inputs.
	    /// </summary>
	    /// <param name="trx"></param>
	    /// <returns>SHA256 hash</returns>
	    public static string GetTransactionHash(Transaction trx)
	    {
		    string dataToHash = Convert.ToString(trx.InCounter);
		    foreach (Transacation_Input input in trx.Inputs)
			    dataToHash += input.OutpointHash + input.OutpointIndex + input.ScriptLenght;
		    dataToHash += Convert.ToString(trx.OutCounter);
		    foreach (Transacation_Output output in trx.Outputs)
		    {
			    dataToHash += output.Value + output.ScriptLenght + output.Text;
			    foreach (string script in output.Script)
				    dataToHash += script;
		    }
		    
		    return Helpers.ComputeSHA256Hash(dataToHash);
	    } 
	    
        /// <summary>
        /// Every input of the function is hashed twice via SHA256. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="timesHashed"></param>
        /// <returns>SHA256 hash</returns>
        public static string ComputeSHA256Hash(string input, int timesHashed = 1)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }

                if (timesHashed == 2)
                    return stringBuilder.ToString();
                return ComputeSHA256Hash(stringBuilder.ToString(), 2);
            }
        }

        /// <summary>
        /// Function computes ripemd160 hash from string
        /// </summary>
        /// <param name="input"></param>
        /// <returns>hash</returns>
        public static string ComputeRIPEMD160Hash(string input)
        {
            using (SshNet.Security.Cryptography.RIPEMD160 ripemd160 = new SshNet.Security.Cryptography.RIPEMD160())
            {
                byte[] data = Encoding.UTF8.GetBytes(input);
                return BitConverter.ToString(ripemd160.ComputeHash(data)).Replace("-", "").ToLower();
            }
        }   
    }
}

