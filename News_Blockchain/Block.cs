using System;
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
        private List<Transaction> _transactions;


        public string PreviousBlocKHeaderHash { get { return _pbhh; } set { _pbhh = value; } }
        public string MerkleRootHash { get { return _merkleRootHash; } set { _merkleRootHash = value; } }
        public uint Time { get { return _time; } set { _time = value; } }
        public uint NBits { get { return _nBits; } set { _nBits = value; } }
        public uint Nonce { get { return _nonce; } set { _nonce = value; } }
        public List<Transaction> Transactions { get { return _transactions; } set { _transactions = value; } }


        public Block(string pbhh, string merkleRootHeaderHash, uint time, uint nBits, uint nonce, List<Transaction> transactions)
        {
            _pbhh = pbhh;
            _merkleRootHash = merkleRootHeaderHash;
            _time = time;
            _nBits = nBits;
            _nonce = nonce;
            _transactions = transactions;
        }
    }
}

