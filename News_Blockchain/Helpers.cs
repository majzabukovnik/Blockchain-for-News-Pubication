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

