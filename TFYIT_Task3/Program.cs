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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("1. Лексический анализ");
            Console.ResetColor();

            string path = @"D:\Media\Универ\Теория формальных языков\inputline.txt";
            LexAnalyzer analyzer = new LexAnalyzer();
            List<string> result = analyzer.Analyze(path);

            string outPath = @"D:\Media\Универ\Теория формальных языков\lexemeslist.txt";
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("2. Синтаксический анализ");
            Console.ResetColor();
            path = @"D:\Media\Универ\Теория формальных языков\lexemeslist.txt";
            Parser parser = new Parser(path);
            parser.Parse();
            #endregion

            #region Семантический анализатор
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n3. Семантический анализ");
            Console.ResetColor();
            Console.WriteLine($"\nИсходная строка: {analyzer.GetText}");
            Console.Write("ПОЛИЗ: ");
            parser.ShowPostfix();
            Console.WriteLine();
            #endregion
        }
    }
}