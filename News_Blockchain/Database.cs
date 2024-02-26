using Tenray.ZoneTree;
using Tenray.ZoneTree.Core;

namespace News_Blockchain;


public abstract class Database
{
    public enum DB
    {
        Blockchain,
        UTXO
    }

    /// <summary>
    /// Returns count of all records
    /// </summary>
    public int Count
    {
        get { return GetAllRecordsCount(); }
    }
    
    protected IZoneTree<string, string> _zoneTree;
    
    /// <summary>
    /// Creates or Opens a new connection to the LocalDB with the default path name = "Blockchain"
    /// </summary>
    public Database()
    {
          _zoneTree = new ZoneTreeFactory<string, string>()
            .SetDataDirectory("Blockchain")
            .OpenOrCreate();
    }
    
    /// <summary>
    /// Creates or Opens a new connection to the LocalDB with custom path name
    /// </summary>
    /// <param name="db">custom path name</param>
    public Database(DB db)
    {
        string path = db == DB.Blockchain ? "Blockchain" : "UTXO";
        _zoneTree = new ZoneTreeFactory<string, string>()
            .SetDataDirectory(path)
            .OpenOrCreate();
    }
    
    /// <summary>
    /// Deletes all records (Blocks from a specified key)
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if successful, false if not</returns>
    public bool DeleteRecords(string key)
    {
        try
        {
            var iterator = _zoneTree.CreateIterator();
            iterator.Seek(key);
            while (iterator.Next())
            {
                _zoneTree.ForceDelete(iterator.CurrentKey);
            }
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Returns count of all records
    /// </summary>
    /// <returns>Count</returns>
    public int GetAllRecordsCount()
    {
        return (int)_zoneTree.Count();
    }
}

public class BlockDB : Database
{
    public BlockDB() : base(DB.Blockchain)
    {
        
    }
    
    /// <summary>
    /// Insert a new key-value (hash, block)
    /// </summary>
    /// <param name="key">Key (Hash of the Block)</param>
    /// <param name="value">Value (Serialized Block)</param>
    public void InsertNewRecord(Block block)
    {
        _zoneTree.AtomicUpsert(Helpers.GetBlockHash(block), Serializator.SerializeToString(block));
    }

    /// <summary>
    /// Get value (Deserialized value of a Block)
    /// </summary>
    /// <param name="key">Key (Hash of the Block)</param>
    /// <returns>Deserialized value of a Block</returns>
    public Block GetRecord(string key)
    {
        string answer = "";
        _zoneTree.TryGet(key, out answer);
        return Serializator.DeserializeToBlock(answer);
    }

    public List<Block> GetLastSpecifiedBlocks(int index)
    {
        List<Block> list = new List<Block>();
        var iterator = _zoneTree.CreateIterator();
        for (int i = 0; i < index; i++)
        {
            list.Add(Serializator.DeserializeToBlock(iterator.CurrentValue));
            iterator.Next();
        }

        return list;
    }
    
    /// <summary>
    /// Get all records store in the database
    /// </summary>
    /// <returns>All records</returns>
    public Dictionary<string, Block> GetAllRecords()
    {
        Dictionary<string, Block> DB = new Dictionary<string, Block>();
        using var iteration = _zoneTree.CreateIterator();
        while (iteration.Next())
        {
            DB.Add(iteration.CurrentKey, Serializator.DeserializeToBlock(iteration.CurrentValue));
        }

        return DB;
    }
}

public class UTXODB : Database
{
    public UTXODB() : base(DB.UTXO)
    {
        
    }
    
    /// <summary>
    /// Insert a new key-value
    /// </summary>
    /// <param name="trans">Transaction to store </param>
    public void InsertNewRecord(UTXOTrans trans)
    {
        _zoneTree.AtomicUpsert(trans.GetKey(), trans.GetValue());
    }
    
    /// <summary>
    /// Get value (value of a Transaction)
    /// </summary>
    /// <param name="key">Key (trxHash-index)</param>
    /// <returns> value of a Transaction</returns>
    public UTXOTrans GetRecord(string key)
    {
        string answer = "";
        _zoneTree.TryGet(key, out answer);
        string[] keys = key.Split("-");
        return new UTXOTrans(keys[0], int.Parse(keys[1]), answer);
    }
    
    /// <summary>
    /// Get all records store in the database
    /// </summary>
    /// <returns>All records</returns>
    public Dictionary<string, string> GetAllRecords()
    {
        Dictionary<string, string> db = new Dictionary<string, string>();
        using var iteration = _zoneTree.CreateIterator();
        while (iteration.Next())
        {
            db.Add(iteration.CurrentKey, iteration.CurrentValue);
        }

        return db;
    }
}



public class UTXOTrans
{
    private string _hashTrans;
    private int _index;
    private string _hashBlock;
    
    public string HashTrans
    {
        get { return _hashTrans; }
        set { _hashTrans = value; }
    }

    public int Index
    {
        get { return _index; }
        set { _index = value; }
    }
    
    public string HashBlock
    {
        get { return _hashBlock; }
        set { _hashBlock = value; }
    }

    public UTXOTrans(string hashTrans, int index, string hashBlock)
    {
        _hashTrans = hashTrans;
        _index = index;
        _hashBlock = hashBlock;
    }

    public string GetKey()
    {
        return _hashTrans + "-" + _index;
    }

    public string GetValue()
    {
        return _hashBlock;
    }
}