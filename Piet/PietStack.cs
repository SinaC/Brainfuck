using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piet
{
    // Don't use framework Stack because Roll operation is 'hard' to implement properly.
    public class PietStack
    {
        private const int StackSize = 65536;
        private readonly int[] _stack = new int[StackSize];
        private int _topIndex = -1;

        public int Count => _topIndex + 1;

        public int Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Stack is empty");
            return _stack[_topIndex];
        }

        public int Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("Stack is empty");
            return _stack[_topIndex--];
        }

        public void Push(int value)
        {
            if (Count >= StackSize)
                throw new InvalidOperationException("Stack overflow");
            _stack[++_topIndex] = value;
        }

        // A single roll to depth n is defined as burying the top value on the stack n deep and bringing all values above it up by 1 place. A negative number of rolls rolls in the opposite direction.
        public void Roll(int count, int depth)
        {
            if (Count == 0)
                throw new InvalidOperationException("Stack is empty");
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int value = _stack[_topIndex];
                    for (int j = 0; j < depth - 1; j++)
                        _stack[_topIndex - j] = _stack[_topIndex - j - 1];
                    _stack[_topIndex - depth+1] = value;
                }
            }
            else
            {
                count = -count;
                for (int i = 0; i < count; i++)
                {
                    int value = _stack[_topIndex - depth+1];
                    for (int j = 0; j < depth - 1; j++)
                        _stack[_topIndex - depth + j+1] = _stack[_topIndex-depth+j+2];
                    _stack[_topIndex] = value;
                }
            }
        }
    }
}
