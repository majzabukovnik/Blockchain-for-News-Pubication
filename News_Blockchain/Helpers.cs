using System;
using System.Text.Json;

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
}

