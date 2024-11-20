using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TFYIT_All_Tasks.Elements;
using TFYIT_All_Tasks.Interpreter;
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
        ExecutionStack stack = new ExecutionStack();
        Dictionary<string, int> variables = new Dictionary<string, int>(); // переменные и их значения


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
        // отобразить ПОЛИЗ
        public void ShowPostfix()
        {
            Dictionary<int, string> postfixes = postfixProcessor.GetPostfixString(lexemes, elseBlockStart);

            Console.Write("ПОЛИЗ:");
            foreach (var postfix in postfixes)
            {
                Console.Write($"{postfix.Value} ");
            }

            Console.WriteLine();
            foreach (var postfix in postfixes)
            {
                Console.WriteLine($"{postfix.Key}. {postfix.Value}");
            }

        }

        #region Интерпретатор
        // интерпретация
        public void ProcessLine()
        {
            Dictionary<int, PostfixEntry> postfixes = postfixProcessor.GetPostfixes(lexemes, elseBlockStart);
            Dictionary<int, string> postfixValues = postfixProcessor.GetPostfixString(lexemes, elseBlockStart);

            int pos = 1;
            while (pos < postfixes.Count + 1)
            {
                var entry = postfixes[pos];
                switch (entry.Type)
                {
                    case EEntryType.etCmd:
                        Console.Write($"{pos}. Обработка команды '{postfixValues[pos]}'\t");
                        Console.Write("Состояние стека: { ");
                        ShowStack();
                        Console.WriteLine("}");

                        var cmd = (ECmd)entry.Index;
                        pos = ExecuteCmd(cmd, pos, postfixValues);
                        break;
                    case EEntryType.etVar:
                         Console.Write($"{pos}. Обработка переменной '{postfixValues[pos]}'\t");
                        Console.Write("Состояние стека: { ");
                        ShowStack();
                        Console.WriteLine("}");

                        stack.PushVal(GetVarValue(postfixValues[pos]));
                        pos++; 
                        break;
                    case EEntryType.etConst:
                        Console.Write($"{pos}. Обработка константы '{postfixValues[pos]}'\t");
                        Console.Write("Состояние стека: { ");
                        ShowStack();
                        Console.WriteLine("}");

                        stack.PushVal(int.Parse(postfixValues[pos]));
                        pos++;
                        break;
                    case EEntryType.etCmdPtr:
                        Console.Write($"{pos}. Обработка адреса '{postfixValues[pos]}'\t");
                        Console.Write("Состояние стека: { ");
                        ShowStack();
                        Console.WriteLine("}");

                        stack.PushVal(int.Parse(postfixValues[pos]));
                        pos++;
                        break;
                }
            }
            ShowVariables();

        }
        // исполнение команд интерпретатором
        public int ExecuteCmd(ECmd cmd, int pos, Dictionary<int, string> posfixValues)
        {
            switch (cmd)
            {
                case ECmd.SET:
                    int value = stack.PopVal();
                    int targetValue = stack.PopVal();

                    string variable = variables.FirstOrDefault(pair => pair.Value == targetValue).Key;
                    SetVarValue(variable, value);
                    return pos + 1;
                case ECmd.ADD:
                    stack.PushVal(stack.PopVal() + stack.PopVal());
                    return pos + 1;
                case ECmd.SUB:
                    int subBy = stack.PopVal();
                    stack.PushVal(stack.PopVal() - subBy);
                    return pos + 1;
                case ECmd.MUL:
                    stack.PushVal(stack.PopVal() * stack.PopVal());
                    return pos + 1;
                case ECmd.DIV:
                    int divideBy = stack.PopVal();
                    if (divideBy == 0) throw new Exception("Деление на ноль!");
                    else
                        stack.PushVal(stack.PopVal() / divideBy);
                    return pos + 1;
                case ECmd.CMPL:
                    stack.PushVal(stack.PopVal() > stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.CMPM:
                    stack.PushVal(stack.PopVal() < stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.CMPLE:
                    stack.PushVal(stack.PopVal() >= stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.CMPME:
                    stack.PushVal(stack.PopVal() <= stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.CMPNE:
                    stack.PushVal(stack.PopVal() != stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.CMPE:
                    stack.PushVal(stack.PopVal() == stack.PopVal() ? 1 : 0);
                    return pos + 1;
                case ECmd.AND:
                    int cond1, cond2;
                    cond1 = stack.PopVal();
                    cond2 = stack.PopVal();
                    stack.PushVal((cond1 != 0 && cond2 != 0) ? 1 : 0);
                    return pos + 1;
                case ECmd.OR:
                    cond1 = stack.PopVal();
                    cond2 = stack.PopVal();
                    stack.PushVal((cond1 != 0 || cond2 != 0) ? 1 : 0);
                    return pos + 1;
                case ECmd.JMP:
                    return stack.PopVal();
                case ECmd.JZ:
                    int addr = stack.PopVal();
                    int cond = stack.PopVal();
                    return cond == 0 ? addr : pos + 1;
                default:
                    throw new Exception("Неизвестная команда");

            }
        }
        // получение значения переменной по её названию (если значение не задано - предложение задать)
        private int GetVarValue(string identifier)
        {
            if (variables.ContainsKey(identifier))
                return variables[identifier];
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" - Значение переменной {identifier} не определено. Задайте значение {identifier} = ");
                int newVariableValue = int.Parse(Console.ReadLine());
                Console.ResetColor();
                variables.Add(identifier, newVariableValue);
                return variables[identifier];
            }
        }
        // установка значения переменной по её названию
        private void SetVarValue(string identifier, int value)
        {
            if (variables.ContainsKey(identifier))
                variables[identifier] = value;
            else
                throw new Exception($"Попытка установить значение неизвестной переменной {identifier}");
        }
        // отображение стека в процессе интерпретации
        private void ShowStack()
        {
            Stack<int> toReturn = stack.stack;
            foreach (int item in toReturn)
            {
                Console.Write($"{item} ");
            }
        }
        // отображение текущих значений переменных
        private void ShowVariables()
        {
            Console.WriteLine("\nЗначения переменных на момент окончания работы:");
            foreach (var item in variables)
            {
                Console.WriteLine($"Переменная {item.Key} - значение {item.Value}");
            }
        }
        #endregion

        #region Вспомогательные методы для синтаксического анализа
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
        #endregion

        #region Синтаксический анализ.
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
                postfixProcessor.WriteCmdPtr(-2);
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
            Console.WriteLine("Анализ оператора");
            ParseStatement();
            if (Match("DLM"))
            {
                Console.Write("Обнаружен разделитель '");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(";");
                Console.ResetColor();
                Console.WriteLine("', продолжается анализ операторов");
                ParseStatementList();
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

            if (Match("OR"))
            {
                Console.WriteLine("Обнаружен 'or', анализ логических выражений продолжается");
                ParseCondition();

                ProcessComparisonIndex();
                postfixProcessor.WriteCmd(Elements.ECmd.OR);
            }
            if (Match("AND"))
            {
                Console.WriteLine("Обнаружен 'and', анализ логических выражений продолжается");
                ParseCondition();

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
        #endregion
        public List<Lexeme> GetLexemes
        {
            get { return lexemes; }
        }
    }
}
