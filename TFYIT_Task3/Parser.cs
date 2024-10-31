using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFYIT_Task3
{
    internal class Parser
    {
        List<Lexeme> lexemes = new List<Lexeme>();
        string[] reservedWords = { "if", "then", "else", "end"}; // ключевые слова
        string[] relSigns = { ">", "<", "==", "<>", ">=", "<=", "=" }; // знаки сравнения
        string[] mathSigns = { "+", "-", "*", "/" }; // знаки арифметических операций
        string[] logicSigns = { "or", "and" }; // логические выражения
        private string[] delimiters = { ";", ","}; // разделители


        private int currentIndex = 0;
        private Lexeme currentLexeme
        {
            get
            {
                if (AnyMoreLexemes())
                {
                    return lexemes[currentIndex];
                }
                else
                {
                    throw new Exception("Ошибка");
                }
            }
        }

        // конструктор
        public Parser(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line = sr.ReadLine();
                string lexeme;
                string lexType;
                int index;
                while (line != null)
                {
                    index = int.Parse(line.Split()[0]);
                    lexeme = line.Split()[1];
                    lexType = line.Split()[2];
                    lexemes.Add(new Lexeme(lexeme, lexType, index));
                    line = sr.ReadLine();
                }
            }
        }
        public void ShowLexemes()
        {
            int i = 0;
            foreach (Lexeme lexeme in lexemes)
            {
                Console.WriteLine($"{i}. Лексема: {lexeme.Value}, тип: {lexeme.Type}");
                i++;
            }
        }
        // остались ли ещё лексемы для считывания
        private bool AnyMoreLexemes()
        {
            return currentIndex < lexemes.Count;
        }
        private bool Match(string type)
        {
            //if (currentIndex > lexemes.Count)
            //{
            //    throw new Exception($"Ожидался тип {type}, но входная строка закончилась.");
            //}
            if (currentLexeme.Type == type)
            {
                currentIndex++;
                return true;
            }
            return false;
        }
        private void Expect(string expected)
        {
            if (!Match(expected))
            {
                throw new Exception($"Ожидался тип {expected}");
            }
        }

        // запуск парсинга
        public void Parse()
        {
            Console.WriteLine("Начинается синтаксический анализ...\n");
            ParseStatement();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nАнализ завершён. Ошибки не найдены.");
            Console.ResetColor();
        }
        // парсинг оператора if-then-else
        private void ParseStatement()
        {
            Console.Write($"Анализ условной конструкции if-then-else: Текущая лексема: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(currentLexeme.Value);
            Console.ResetColor();

            if (Match("IF"))
            {
                Console.WriteLine("Анализ оператора if");
                ParseCondition();
                Expect("THEN");
                Console.WriteLine("Анализ оператора then");
                ParseStatementList();
                Expect("ELSE");
                Console.WriteLine("Анализ оператора else");
                ParseStatementList();
                Expect("END");
            }
            else
            {
                Console.WriteLine("Анализ операции присваивания");
                ParseAssignment();
            }
        }
        // парсинг списка операторов
        private void ParseStatementList()
        {
            Console.WriteLine("Анализ списка операторов");
            ParseStatement();
            while (Match("DLM"))
            {
                Console.Write("Обнаружен разделитель '");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(";");
                Console.ResetColor();
                Console.WriteLine("', продолжается анализ списка операторов");
                ParseStatement();
            }
        }
        // парсинг условий
        private void ParseCondition()
        {
            Console.WriteLine("Анализ логических выражений");
            ParseExpression();
            ParseComparisonOp();
            ParseExpression();

            while (Match("OR"))
            {
                Console.WriteLine("Обнаружен 'or', анализ логических выражений продолжается");
                ParseExpression();
                ParseComparisonOp();
                ParseExpression();
            }
            while (Match("AND"))
            {
                Console.WriteLine("Обнаружен 'and', анализ логических выражений продолжается");
                ParseExpression();
                ParseComparisonOp();
                ParseExpression();

            }
        }
        // парсинг идентификатора или константы
        private void ParseExpression()
        {
            if (lexemes[currentIndex].Type == "ID")
            {
                Console.Write("- Обнаружен идентификатор ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(currentLexeme.Value);
                Console.ResetColor();
                currentIndex++;
            }
            else if (lexemes[currentIndex].Type == "NUM")
            {
                Console.Write("- Обнаружена константа ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(currentLexeme.Value);
                Console.ResetColor();
                currentIndex++;
            }
            else
            {
                throw new Exception("Ожидался идентификатор или константа");
            }
        }
        // парсинг операции сравнения
        private void ParseComparisonOp()
        {
            Console.Write($"Анализ операции сравнения. Текущая лексема: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(currentLexeme.Value);
            Console.ResetColor();

            if (!Match("REL"))
            {
                throw new Exception("Ожидался оператор сравнения");
            }
        }

        // парсинг оператора присваивания
        private void ParseAssignment()
        {
            ParseExpression();
            Expect("ASGN");
            Console.WriteLine("Анализ операции присваивания");
            ParseExpression();
        }
    }
}
