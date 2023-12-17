using Tenray.ZoneTree;
using Tenray.ZoneTree.Core;

namespace News_Blockchain;

public enum DB
{
    Blockchain,
    UTEx
}
public class Database
{
    /// <summary>
    /// Returns count of all records
    /// </summary>
    public int Count
    {
        get { return GetAllRecordsCount(); }
    }
    
    private IZoneTree<string, string> _zoneTree;
    
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
        string path = db == DB.Blockchain ? "Blockchain" : "UTEx";
        _zoneTree = new ZoneTreeFactory<string, string>()
            .SetDataDirectory(path)
            .OpenOrCreate();
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
    /// 
    /// </summary>
    /// <param name="trans"></param>
    public void InsertNewRecord(UTExTrans trans)
    {
        _zoneTree.AtomicUpsert(trans.GetKey(), trans.GetValue());
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
    
    /// <summary>
    /// Returns count of all records
    /// </summary>
    /// <returns>Count</returns>
    public int GetAllRecordsCount()
    {
        return (int)_zoneTree.Count();
    }
}

public class UTExTrans
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

    public UTExTrans(string hashTrans, int index, string hashBlock)
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