using System.Text;

namespace News_Blockchain;

public enum RequestType
{
    Transaction,
    Block
}

 public struct Request
 {
     private RequestType type;
     private bool last;
     private int? specified;

     public Request(RequestType type, bool last, int? specified = null)
     {
         this.type = type;
         this.last = last;
         this.specified = specified  == null ? -2 : specified;
     }
     
     public bool LastBlock
     {
         get { return last; }
         set { last = value; }
     }
     
     public int? SpecifiedBlock
     {
         get { return specified; }
         set
         {
             if (value >= 0)
             {
                 specified = value;
             }
         }
     }
    /// <summary>
    /// Returns -1 if it needs the last block and an int if it needs a specific block
    /// </summary>
    /// <returns></returns>
     public int GetBlockIndex()
     {
         return last == true ? -1 : (int)specified;
     }
 }

public class Message
{
    private Request _request;
    
    private Block? _block;
    
    private Transaction? _transaction;
    
    public Request Request
    {
        get => _request;
        set => _request = value;
    }

    public Block? Block
    {
        get => _block;
        set => _block = value;
    }

    public Transaction? Transaction
    {
        get => _transaction;
        set => _transaction = value;
    }

    public Message(){}
    
    public Message(Block block)
    {
        _block = block;
    }

    public Message(Transaction transaction)
    {
        _transaction = transaction;
    }

    public Type? GetMessageType()
    {
        if (_block != null) return _block.GetType();
        else if (_transaction != null) return _transaction.GetType();
        else if (_request.GetBlockIndex() != -2) return _request.GetType();
        else return null;
        
    }

    public Block GetBlock()
    {
        return _block;
    }

    public Transaction GetTransaction()
    {
        return _transaction;
    }
    

}