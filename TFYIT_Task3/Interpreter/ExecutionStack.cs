using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFYIT_All_Tasks.Interpreter
{
    internal class ExecutionStack
    {
        public Stack<int> stack;

        public ExecutionStack()
        {
            stack = new Stack<int>();
        }

        public int PopVal()
        {
            if (stack.Count == 0)
                throw new InvalidOperationException("Ошибка: попытка извлечь значение из пустого стека.");
            return stack.Pop();
        }

        public void PushVal(int value)
        {
            stack.Push(value);
        }

        public void ShowStack()
        {
            Console.WriteLine("Текущее состояние стека: ");
            foreach (var item in stack)
            {
                Console.WriteLine(item);
            }
        }
    }
}
