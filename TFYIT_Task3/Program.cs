using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using TFYIT_Task3;

namespace TFYIT_Task2
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\123\Documents\Универ\Теория формальных языков и трансляций\lexemesList.txt";
            Parser parser = new Parser(path);
            //parser.ShowLexemes();
            parser.Parse();
        }
    }
}