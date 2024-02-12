using System.Text;

namespace News_Blockchain;

public class Message
{
    //vrsta sporocila se manjka // 
    
    private byte[] _block;
    private byte[] _transaction;

    public byte[] Block => _block;
    public byte[] Transaction => _transaction;

    public Message(Block block)
    {
        _block = Encoding.UTF8.GetBytes(Serializator.SerializeToString(block));
    }

    public Message(Transaction transaction)
    {
        _transaction = Encoding.UTF8.GetBytes(Serializator.SerializeToString(transaction));
    }

    public Type GetMessageType()
    {
        if (_block != null) return _block.GetType();
        else return _transaction.GetType();
    }

    public Block GetBlock()
    {
        return Serializator.DeserializeToBlock(Encoding.UTF8.GetString(_block));
    }

    public Transaction GetTransaction()
    {
        return Serializator.DeserializeToTransaction(Encoding.UTF8.GetString(_transaction));
    }
    

}