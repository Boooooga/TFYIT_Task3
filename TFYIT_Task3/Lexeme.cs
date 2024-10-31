using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFYIT_Task3
{
    internal class Lexeme
    {
        string value, type;
        public Lexeme(string value, string type)
        {
            this.value = value;
            this.type = type;
        }
        public string Value { get { return value; } }
        public string Type { get { return type; } }
    }
}
