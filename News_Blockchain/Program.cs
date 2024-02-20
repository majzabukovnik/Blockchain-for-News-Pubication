namespace News_Blockchain;

class Program
{
    static void Main(string[] args)
    {
        List<string> script = new List<string> { "   ", "", "3", "", "" };
        Transacation_Output to = new Transacation_Output(50, 5, script, "To je besedilo" );
        Transacation_Input ti = new Transacation_Input("", 0, 0, "");
        List <Transacation_Output> toL = new List<Transacation_Output> { to };
        List<Transacation_Input> tiL = new List<Transacation_Input> { ti };
        
        Transaction t = new Transaction(tiL, toL);

        BlockDB blockDB = new BlockDB();
        
        Miner miner = new Miner(new List<Transaction> { t }, blockDB);

    }
}

