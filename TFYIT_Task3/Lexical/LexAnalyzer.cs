using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TFYIT_All_Tasks.Lexical
{
    public class LexAnalyzer
    {
        string path = "";
        string[] reservedWords = { "if", "then", "else", "end", "or", "and", "not" };
        string[] specialSigns = { ">", "<", "==", "<>", ">=", "<=", "=", "+", "-", "*", "/" };
        List<string> messages = new List<string>();
        public List<int> indexes = new List<int>();
        private string[] delimiters = { ".", ";", ",", "(", ")" };
        string originalText = "";

        Dictionary<string, List<string>> lexemes = new Dictionary<string, List<string>>();


        // ПАРАМЕТРЫ АВТОМАТА
        private enum States { S, ID, NUM, OPER, ASGN, CMPM, CMPL, CMPME, CMPLE, CMPNE, CMPE, F, DLM, ERR } // состояния автомата
        // S - начальное, ID - идентификатор, NUM - константа, OPER - арифметическая операция,
        // ASGN - присваивание, CMPM - больше, CMPL - меньше, CMPME - больше или равно, CMPLE - меньше или равно,
        // CMPE - равно, CMPNE - не равно,
        // DLM - разделитель, ERR - ошибка, F - финальное
        private States currentState;
        public string GetText
        {
            get
            {
                return originalText;
            }
        }
        private string GetLexemeType(string input)
        {
            if (input == "if") return "IF";
            else if (input == "then") return "THEN";
            else if (input == "else") return "ELSE";
            else if (input == "end") return "END";
            else if (input == ">" || input == "<" || input == "==" ||
                input == ">=" || input == "<=" || input == "<>") return "REL";
            else if (input == "+" || input == "-" || input == "*" || input == "/") return "MATH";
            else if (input == "and") return "AND";
            else if (input == "or") return "OR";
            else if (input == ";" || input == ",") return "DLM";
            else if (input == "=") return "ASGN";
            else if (char.IsDigit(input[0])) return "NUM";
            else if (char.IsLetter(input[0])) return "ID";
            else return "UNKNOWN";
        }
        public List<string> Analyze(string path)
        {
            int i = 0;
            int start = 0;
            string text;
            bool isOk = true;
            string unknownLex = "";
            List<string> allLexemes = new List<string>();
            List<string> orderedLexemesOut = new List<string>();
            // читаем анализируемый текст из файла
            using (StreamReader file = new StreamReader(path))
            {
                text = file.ReadToEnd();
            }

            Console.Write($"Исходный текст: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(text);
            Console.ResetColor();
            Console.WriteLine("\n");
            originalText = text;

            text = text.ToLower() + " ";
            while (currentState != States.F && currentState != States.ERR)
            {
                States prevState = currentState;
                bool add = true;

                switch (currentState)
                {
                    case States.S:
                        // если считанный символ - буква
                        if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        // если считанный символ - цифра
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        // если считанный символ - знак больше
                        else if (text[i] == '>')
                            currentState = States.CMPM;
                        // если считанный символ - знак меньше
                        else if (text[i] == '<')
                            currentState = States.CMPL;
                        // если считанный символ - знак равенства
                        else if (text[i] == '=')
                            currentState = States.ASGN;
                        // если считанный символ - арифметическая операция
                        else if (text[i] == '+' || text[i] == '-' || text[i] == '*' || text[i] == '/')
                            currentState = States.OPER;
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            isOk = false;
                        }
                        break;

                    case States.ID:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetterOrDigit(text[i]))
                            add = false;
                        else if (text[i] == '>')
                            currentState = States.CMPM;
                        else if (text[i] == '<')
                            currentState = States.CMPL;
                        else if (text[i] == '=')
                            currentState = States.ASGN;
                        else if (text[i] == '+' || text[i] == '-' || text[i] == '*' || text[i] == '/')
                            currentState = States.OPER;
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.NUM:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsDigit(text[i]))
                            add = false;
                        else if (text[i] == '>')
                            currentState = States.CMPM;
                        else if (text[i] == '<')
                            currentState = States.CMPL;
                        else if (text[i] == '=')
                            currentState = States.ASGN;
                        else if (text[i] == '+' || text[i] == '-' || text[i] == '*' || text[i] == '/')
                            currentState = States.OPER;
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.CMPM:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else if (text[i] == '=')
                        {
                            currentState = States.CMPME;
                            add = false; // если получился символ >=
                        }
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.CMPL:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else if (text[i] == '=')
                        {
                            currentState = States.CMPME;
                            add = false; // если получился символ <=
                        }
                        else if (text[i] == '>')
                        {
                            currentState = States.CMPNE;
                            add = false; // если получился символ <>
                        }
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        //else if (text[i - 1] == '<' && text[i] == '>')
                        //    currentState = States.S;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.ASGN:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))    
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else if (text[i] == '=')
                        {
                            currentState = States.CMPE;
                            add = false;
                        }
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.OPER:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.CMPME:
                    case States.CMPLE:
                    case States.CMPNE:
                    case States.CMPE:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else if (text[i] == ';')
                            currentState = States.DLM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;

                    case States.DLM:
                        if (text[i] == ' ')
                            currentState = States.S;
                        else if (char.IsLetter(text[i]))
                            currentState = States.ID;
                        else if (char.IsDigit(text[i]))
                            currentState = States.NUM;
                        else
                        {
                            currentState = States.ERR;
                            isOk = false;
                            unknownLex = text[i].ToString();
                            add = false;
                        }
                        break;
                }

                string message = "";
                if (add)
                {
                    for (int j = start; j < i; j++)
                    {
                        message += text[j];
                    }
                    if (messages.Count > 0 && message.Replace(" ", "").Length != 0)
                    {
                        if (messages.Last() != message.Replace(" ", ""))
                        {
                            messages.Add(message.Replace(" ", ""));
                            indexes.Add(start + 1);
                        }
                    }
                    else if (message.Replace(" ", "").Length != 0)
                    {
                        messages.Add(message.Replace(" ", ""));
                        indexes.Add(start);
                    }
                }

                if (currentState != prevState && (currentState == States.ID ||
                    currentState == States.NUM || currentState == States.CMPL || 
                    currentState == States.CMPM || currentState == States.ASGN || 
                    currentState == States.OPER || currentState == States.DLM))
                {
                    start = i;
                }

                if (currentState != States.F)
                    i++;

                if (message.Contains("end"))
                {
                    break;
                }

            }

            if (isOk) 
            {
                foreach (string item in messages)
                {
                    if (reservedWords.Contains(item))
                    {
                        if (!lexemes.Keys.Contains("Зарезервированные слова"))
                        {
                            lexemes.Add("Зарезервированные слова", new List<string> { item });
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| зарезервированное слово");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }

                        else
                        {
                            lexemes["Зарезервированные слова"].Add(item);
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| зарезервированное слово");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                    }
                    // если добавляем спецсимвол
                    else if (specialSigns.Contains(item))
                    {
                        if (!lexemes.Keys.Contains("Спецсимволы"))
                        {
                            lexemes.Add("Спецсимволы", new List<string> { item });
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| спецсимвол");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                        else
                        {
                            lexemes["Спецсимволы"].Add(item);
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| спецсимвол");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                    }
                    // если добавляем разделитель
                    else if (delimiters.Contains(item))
                    {
                        if (!lexemes.Keys.Contains("Разделители"))
                        {
                            lexemes.Add("Разделители", new List<string> { item });
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| разделитель");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                        else
                        {
                            lexemes["Разделители"].Add(item);
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| разделитель");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                    }
                    // если добавляем константу
                    else if (item.All(char.IsDigit))
                    {
                        if (!lexemes.Keys.Contains("Константы"))
                        {
                            lexemes.Add("Константы", new List<string> { item });
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| константа");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                        else
                        {
                            lexemes["Константы"].Add(item);
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| константа");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                    }
                    else
                    {
                        if (!lexemes.Keys.Contains("Идентификаторы"))
                        {
                            lexemes.Add("Идентификаторы", new List<string> { item });
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| идентификатор");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                        else
                        {
                            lexemes["Идентификаторы"].Add(item);
                            allLexemes.Add(item);
                            Console.WriteLine($"{item} \t| идентификатор");
                            orderedLexemesOut.Add($"{item} " + $"{GetLexemeType(item)}");
                        }
                    }
                }


                Console.WriteLine();
                foreach (string key in lexemes.Keys)
                {
                    Console.Write($"{key}: ");
                    foreach (string value in lexemes[key].Distinct())
                    {
                        Console.Write($"{value}, ");
                    }
                    Console.WriteLine();
                }

                return orderedLexemesOut;
            }
            else
            {
                Console.WriteLine($"Обнаружена неизвестная лексема: {unknownLex}!");
                return new List<string>();
            }
            
        }
    }
}
