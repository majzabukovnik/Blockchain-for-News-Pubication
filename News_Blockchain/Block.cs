using System;
using System.Text.Json.Serialization;
using System.Transactions;

namespace News_Blockchain
{
    public class Block
    {
        private string _pbhh;
        private string _merkleRootHash;
        private uint _time;
        private uint _nBits;
        private uint _nonce;
        private int _index;
        private List<Transaction> _transactions;

        //[JsonPropertyName("pbhh")]
        public string PreviousBlocKHeaderHash { get { return _pbhh; } set { _pbhh = value; } }
        
        //[JsonPropertyName("merkleRootHeaderHash")]
        public string MerkleRootHash { get { return _merkleRootHash; } set { _merkleRootHash = value; } }
        
        public uint Time { get { return _time; } set { _time = value; } }
        
        public uint NBits { get { return _nBits; } set { _nBits = value; } }
        
        public uint Nonce { get { return _nonce; } set { _nonce = value; } }
        
        public int Index { get { return _index; } set { _index = value; } }
        
        public List<Transaction> Transactions { get { return _transactions; } set { _transactions = value; } }
        
        [JsonConstructor]
        public Block(string previousBlocKHeaderHash, string merkleRootHash, uint time, uint nBits, uint nonce, int index, List<Transaction> transactions)
        {
            _pbhh = previousBlocKHeaderHash;
            _merkleRootHash = merkleRootHash;
            _time = time;
            _nBits = nBits;
            _nonce = nonce;
            _index = index;
            _transactions = transactions;
        }
    }
}

