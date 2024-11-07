using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFYIT_All_Tasks.Elements;

namespace TFYIT_All_Tasks.Semantic
{
    internal class PostfixEntry
    {
        public EEntryType Type { get; set; }
        public int Index { get; set; }

        public PostfixEntry(EEntryType type, int index)
        {
            Type = type;
            Index = index;
        }
    }
}
