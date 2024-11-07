using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFYIT_All_Tasks.Elements;
using TFYIT_All_Tasks.Lexical;

namespace TFYIT_All_Tasks.Semantic
{
    internal class PostfixProcessor
    {
        private List<PostfixEntry> postfixEntries;

        public PostfixProcessor()
        {
            postfixEntries = new List<PostfixEntry>();
        }
        public int WriteCmd(ECmd cmd) // команда
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etCmd, (int)cmd));
            return postfixEntries.Count - 1;
        }
        public int WriteVar(int ind) // переменная
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etVar, ind));
            return postfixEntries.Count - 1;
        }
        public int WriteConst(int ind)
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etConst, ind));
            return postfixEntries.Count - 1;
        }
        public int WriteCmdPtr(int ptr)
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etCmdPtr, ptr));
            return postfixEntries.Count - 1;
        }
        public void SetCmdPtr(int ind, int ptr)
        {
            postfixEntries[ind] = new PostfixEntry(EEntryType.etCmdPtr, ptr);
        }

        public void PrintPostfix(List<Lexeme> lexemes, int elsePtr)
        {
            int i = 0;
            List<string> orderedPostfixes = new List<string>();
            foreach (var entry in postfixEntries)
            {
                string entryDescription = "";

                switch (entry.Type)
                {
                    case EEntryType.etCmd:
                        entryDescription = $"{((ECmd)entry.Index)}";
                        break;
                    case EEntryType.etVar:
                        entryDescription = $"{lexemes[entry.Index - 1].Value}";
                        break;
                    case EEntryType.etConst:
                        i++;
                        entryDescription = $"{lexemes[entry.Index - 1].Value}";
                        break;
                    case EEntryType.etCmdPtr:
                        if (entry.Index == -1)
                            entry.Index = elsePtr;
                        entryDescription = $"{entry.Index + 2}";
                        break;
                    default:
                        entryDescription = "Неизвестный тип";
                        break;
                }
                i++;
                orderedPostfixes.Add($"{i}. {entryDescription}");
                Console.Write($"{entryDescription} ");
            }
            Console.WriteLine();
            foreach (string item in orderedPostfixes)
            {
                Console.WriteLine(item);
            }
        }
    }
}
