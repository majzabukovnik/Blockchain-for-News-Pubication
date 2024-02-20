using System.Text;

namespace News_Blockchain;

 public struct Request
 {
     private bool lastBlock;
     private int specifiedBlock;
     
     public bool LastBlock
     {
         get { return lastBlock; }
         set { lastBlock = value; }
     }
     
     public int SpecifiedBlock
     {
         get { return specifiedBlock; }
         set
         {
             if (value >= 0)
             {
                 specifiedBlock = value;
             }
         }
     }
    /// <summary>
    /// Returns -1 if it needs the last block and an int if it needs a specific block
    /// </summary>
    /// <returns></returns>
     public int GetBlockIndex()
     {
         return lastBlock == true ? -1 : specifiedBlock;
     }
 }

public class Message
{
    public Request _request;
   // private byte[] _block;
   private Block _block;
    //private byte[] _transaction;
    private Transaction _transaction;

    //public byte[] Block => _block;
    
    //public byte[] Transaction => _transaction;

    public Message(Block block)
    {
        //_block = Encoding.UTF8.GetBytes(Serializator.SerializeToString(block));
        _block = block;
    }

    public Message(Transaction transaction)
    {
        //_transaction = Encoding.UTF8.GetBytes(Serializator.SerializeToString(transaction));
        _transaction = transaction;
    }

    public Type GetMessageType()
    {
        if (_block != null) return _block.GetType();
        else if (_transaction != null) return _transaction.GetType();
        else return _request.GetType();
        
    }

    public Block GetBlock()
    {
        //return Serializator.DeserializeToBlock(Encoding.UTF8.GetString(_block));
        return _block;
    }

    public Transaction GetTransaction()
    {
        //return Serializator.DeserializeToTransaction(Encoding.UTF8.GetString(_transaction));
        return _transaction;
    }
    

}