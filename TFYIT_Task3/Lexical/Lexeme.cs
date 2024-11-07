using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFYIT_All_Tasks.Lexical
{
    internal class Lexeme
    {
        string value, type;
        int index;
        public Lexeme(string value, string type, int index)
        {
            this.value = value;
            this.type = type;
            this.index = index;
        }
        public string Value { get { return value; } }
        public string Type { get { return type; } }
        public int Index { get { return index; } }
        public Lexeme FindLexByValue(string value)
        {
            if (this.value == value) return this;
            else return null;
        }
    }
}
