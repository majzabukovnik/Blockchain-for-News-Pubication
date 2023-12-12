using Tenray.ZoneTree;
using Tenray.ZoneTree.Core;

namespace News_Blockchain;

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
    /// <param name="path">custom path name</param>
    public Database(string path)
    {
        _zoneTree = new ZoneTreeFactory<string, string>()
            .SetDataDirectory(path)
            .OpenOrCreate();
    }
    
    /// <summary>
    /// Insert a new key-value pair (hash, block)
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
        //Nevem ce to rabmo... kommentiri na to
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