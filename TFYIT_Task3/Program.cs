using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using TFYIT_All_Tasks.Lexical;
using TFYIT_All_Tasks.Semantic;
using TFYIT_All_Tasks.Syntax;

namespace TFYIT_Task2
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Лексический анализатор
            // ЛЕКСИЧЕСКИЙ АНАЛИЗАТОР
            string path = @"C:\Users\123\Documents\Универ\Теория формальных языков и трансляций\inputCode.txt";
            LexAnalyzer analyzer = new LexAnalyzer();
            List<string> result = analyzer.Analyze(path);

            string outPath = @"C:\Users\123\Documents\Универ\Теория формальных языков и трансляций\lexemesList.txt";
            using (StreamWriter sw = new StreamWriter(outPath))
            {
                int i = 0;
                foreach (string line in result)
                {
                    sw.WriteLine($"{analyzer.indexes[i]} " + line);
                    i++;
                }
            }

            Console.WriteLine();
            #endregion

            #region Синтаксический анализатор
            // СИНТАКСИЧЕСКИЙ АНАЛИЗАТОР
            path = @"C:\Users\123\Documents\Универ\Теория формальных языков и трансляций\lexemesList.txt";
            Parser parser = new Parser(path);
            parser.Parse();
            #endregion

            Console.WriteLine();

            Semantic semantic = new Semantic(parser);
            Console.WriteLine(semantic.GeneratePOLIZ());
        }
    }
}