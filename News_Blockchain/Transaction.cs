using System;
namespace News_Blockchain
{
    public class Transaction
    {
        private List<Transacation_Input> _inputs;
        private int _inCounter; //po potrebi

        private List<Transacation_Output> _outputs;
        private int _outCounter; //po potrebi

        public List<Transacation_Input> Inputs { get { return _inputs; } set { _inputs = value; } }
        public int InCounter { get { return _inputs.Count; } }
        public List<Transacation_Output> Outputs { get { return _outputs; } set { _outputs = value; } }
        public int OutCounter { get { return _outputs.Count; }  }

        public Transaction(List<Transacation_Input> inputs, List<Transacation_Output> outputs)
        {
            _inputs = inputs;
            _outputs = outputs;
        }

    }

    public class Transacation_Input
    {
        private string _outpointHash;
        private int _outpointIndex;
        private int _scriptLenght;
        private string _stringSignature;

        public string OutpointHash { get { return _outpointHash; } set { _outpointHash = value; } }
        public int OutpointIndex { get { return _outpointIndex; } set { _outpointIndex = value; } }
        public int ScriptLenght { get { return _scriptLenght; } set { _scriptLenght = value; } }
        public string stringSignature { get { return _stringSignature; } set { _stringSignature = value; } }

        public Transacation_Input(string outpointHash, int outpointIndex, int scriptLenght, string scriptSignature)
        {
            _outpointHash = outpointHash;
            _outpointIndex = outpointIndex;
            _scriptLenght = scriptLenght;
            _stringSignature = scriptSignature;
        }
    }

    public class Transacation_Output
    {
        private double _value;
        private int _scriptLenght;
        private List<string> _script;
        private string _text;

        public double Value { get { return _value; } set { _value = value; } }
        public int ScriptLenght { get { return _scriptLenght; } set { _scriptLenght = value; } }
        public string Text { get { return _text; } set { _text = value; } }
        public List<string> Script { get { return _script; } set { _script = value; } }


        public Transacation_Output(double value, int scriptLenght, List<string> script, string text = "")
        {
            _value = value;
            _scriptLenght = scriptLenght;
            _text = text;
            _script = script;
        }
    }

}