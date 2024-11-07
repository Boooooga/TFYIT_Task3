using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFYIT_All_Tasks.Lexical;
using TFYIT_All_Tasks.Semantic;

namespace TFYIT_All_Tasks.Syntax
{
    internal class Parser
    {
        List<Lexeme> lexemes = new List<Lexeme>();
        string[] reservedWords = { "if", "then", "else", "end" }; // ключевые слова
        string[] relSigns = { ">", "<", "==", "<>", ">=", "<=", "=" }; // знаки сравнения
        string[] mathSigns = { "+", "-", "*", "/" }; // знаки арифметических операций
        string[] logicSigns = { "or", "and" }; // логические выражения
        private string[] delimiters = { ";", "," }; // разделители
        private int comparisonIndex = 0;
        private int elseBlockStart = 0;

        private PostfixProcessor postfixProcessor;


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
            postfixProcessor = new PostfixProcessor();
        }
        // отобразить лексемы
        public void ShowLexemes()
        {
            int i = 0;
            foreach (Lexeme lexeme in lexemes)
            {
                Console.WriteLine($"{i}. Лексема: {lexeme.Value}, тип: {lexeme.Type}");
                i++;
            }
        }
        // отобразить ПОЛИЗ
        public void ShowPostfix()
        {
            postfixProcessor.PrintPostfix(lexemes, elseBlockStart);
        }
        // остались ли ещё лексемы для считывания
        private bool AnyMoreLexemes()
        {
            return currentIndex < lexemes.Count;
        }
        // совпал ли тип лексемы
        private bool Match(string type)
        {
            if (currentLexeme.Type == type)
            {
                currentIndex++;
                return true;
            }
            return false;
        }
        // проверка порядка следования
        private void Expect(string expected)
        {
            if (!Match(expected))
            {
                throw new Exception($"Ожидался тип {expected}");
            }
        }
        // вспомогательный метод для занесения операций сравнения в постфиксную запись
        private void ProcessComparisonIndex()
        {
            if (lexemes[comparisonIndex].Value == ">")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPM);
            if (lexemes[comparisonIndex].Value == "<")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPL);
            if (lexemes[comparisonIndex].Value == ">=")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPME);
            if (lexemes[comparisonIndex].Value == "<=")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPLE);
            if (lexemes[comparisonIndex].Value == "==")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPE);
            if (lexemes[comparisonIndex].Value == "<>")
                postfixProcessor.WriteCmd(Elements.ECmd.CMPNE);
        }
        // поиск индекса лексемы по значению
        private int FindLexIndexByValue(string value)
        {
            foreach (Lexeme lexeme in lexemes)
            {
                if (lexeme.Value == value)
                    return lexeme.Index;
            }
            return -1;
        }
        private int FindNextLexIndex(int index)
        {
            int i = 0;
            Lexeme lexeme = lexemes[i];
            while (lexeme.Index != index)
            {
                i++;
                lexeme = lexemes[i];
            }
            return lexemes[i + 1].Index;
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
            if (Match("IF"))
            {
                Console.WriteLine("Анализ блока if");
                ParseCondition();
                Expect("THEN");

                postfixProcessor.WriteCmdPtr(-1);
                postfixProcessor.WriteCmd(Elements.ECmd.JZ);
                Console.WriteLine("Анализ блока then");
                ParseStatementList();
                Expect("ELSE");

                elseBlockStart = currentIndex + 1;
                postfixProcessor.WriteCmdPtr(elseBlockStart);
                postfixProcessor.WriteCmd(Elements.ECmd.JMP);
                Console.WriteLine("Анализ блока else");
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
            ProcessComparisonIndex();

            while (Match("OR"))
            {
                Console.WriteLine("Обнаружен 'or', анализ логических выражений продолжается");
                ParseExpression();
                ParseComparisonOp();
                ParseExpression();

                ProcessComparisonIndex();
                postfixProcessor.WriteCmd(Elements.ECmd.OR);
            }
            while (Match("AND"))
            {
                Console.WriteLine("Обнаружен 'and', анализ логических выражений продолжается");
                ParseExpression();
                ParseComparisonOp();
                ParseExpression();

                ProcessComparisonIndex();
                postfixProcessor.WriteCmd(Elements.ECmd.AND);
            }
        }
        // парсинг выражений (с поддержкой арифметических операций)
        private void ParseExpression()
        {
            // Начинаем анализ с первого операнда
            ParseTerm();

            // Анализируем возможные операции сложения/вычитания
            while (currentLexeme.Type == "MATH" && (currentLexeme.Value == "+" || currentLexeme.Value == "-"))
            {
                string op = currentLexeme.Value;  // Оператор + или -
                Console.WriteLine($"Обнаружен оператор {op}, продолжаем анализ выражения");
                currentIndex++;  // Потребляем текущую лексему
                ParseTerm();  // После оператора анализируем следующий операнд
                if (op == "+")
                    postfixProcessor.WriteCmd(Elements.ECmd.ADD);
                if (op == "-")
                    postfixProcessor.WriteCmd(Elements.ECmd.SUB);
            }
        }

        // парсинг термов (с поддержкой умножения/деления)
        private void ParseTerm()
        {
            // Начинаем анализ с первого множителя
            ParseFactor();

            // Анализируем возможные операции умножения/деления
            while (currentLexeme.Type == "MATH" && (currentLexeme.Value == "*" || currentLexeme.Value == "/"))
            {
                string op = currentLexeme.Value;  // Оператор * или /
                Console.WriteLine($"Обнаружен оператор {op}, продолжаем анализ");
                currentIndex++;  // Потребляем текущую лексему
                ParseFactor();  // После оператора анализируем следующий множитель
                if (op == "*")
                    postfixProcessor.WriteCmd(Elements.ECmd.MUL);
                if (op == "/")
                    postfixProcessor.WriteCmd(Elements.ECmd.DIV);
            }
        }

        // парсинг факторов (число или идентификатор)
        private void ParseFactor()
        {
            if (lexemes[currentIndex].Type == "ID")
            {
                Console.Write("- Обнаружен идентификатор ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(currentLexeme.Value);
                Console.ResetColor();
                currentIndex++;
                postfixProcessor.WriteVar(currentIndex);
            }
            else if (lexemes[currentIndex].Type == "NUM")
            {
                Console.Write("- Обнаружена константа ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(currentLexeme.Value);
                Console.ResetColor();
                currentIndex++;
                postfixProcessor.WriteConst(currentIndex);
            }
            else if (Match("LPAREN"))  // Обработка скобок
            {
                Console.WriteLine("Обнаружены скобки, начинаем новое подвыражение");
                ParseExpression();  // Рекурсивно анализируем выражение внутри скобок
                Expect("RPAREN");
            }
            else
            {
                throw new Exception("Ожидался идентификатор, число или скобки");
            }
        }
        // парсинг операции сравнения
        private void ParseComparisonOp()
        {
            Console.Write($"Анализ операции сравнения. Текущая лексема: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(currentLexeme.Value);
            Console.ResetColor();

            comparisonIndex = currentIndex;

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
            postfixProcessor.WriteCmd(Elements.ECmd.SET);
        }

        public List<Lexeme> GetLexemes
        {
            get { return lexemes; }
        }
    }
}
