using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFYIT_All_Tasks.Elements;
using TFYIT_All_Tasks.Interpreter;
using TFYIT_All_Tasks.Lexical;

namespace TFYIT_All_Tasks.Semantic
{
    internal class PostfixProcessor
    {
        private List<PostfixEntry> postfixEntries;
        private ExecutionStack stack;

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
        public int WriteConst(int ind) // константа
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etConst, ind));
            return postfixEntries.Count - 1;
        }
        public int WriteCmdPtr(int ptr) // указатель адреса
        {
            postfixEntries.Add(new PostfixEntry(EEntryType.etCmdPtr, ptr));
            return postfixEntries.Count - 1;
        }
        public void SetCmdPtr(int ind, int ptr) // установить указатель
        {
            postfixEntries[ind] = new PostfixEntry(EEntryType.etCmdPtr, ptr);
        }

        public Dictionary<int, string> GetPostfixString(List<Lexeme> lexemes, int elsePtr)
        {
            int i = 0;
            Dictionary<int, string> orderedPostfixes = new Dictionary<int, string>();
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
                        entryDescription = $"{lexemes[entry.Index - 1].Value}";
                        break;
                    case EEntryType.etCmdPtr:
                        if (entry.Index == -1)
                            entry.Index = elsePtr;
                        if (entry.Index == -2)
                            entry.Index = postfixEntries.Count + 1;
                        entryDescription = $"{entry.Index}";
                        break;
                    default:
                        entryDescription = "Неизвестный тип";
                        break;
                }
                i++;
                orderedPostfixes.Add(i, entryDescription);
            }
            return orderedPostfixes;
        }
        public Dictionary<int, PostfixEntry> GetPostfixes(List<Lexeme> lexemes, int elsePtr)
        {
            Dictionary<int, PostfixEntry> pairs = new Dictionary<int, PostfixEntry>();
            int i = 1;
            foreach (var entry in postfixEntries)
            {
                pairs.Add(i, entry);
                i++;
            }
            return pairs;
        }
    }
}
