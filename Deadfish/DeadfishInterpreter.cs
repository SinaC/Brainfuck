using System;
using System.Collections.Generic;
using System.Linq;

namespace Deadfish
{
    //https://esolangs.org/wiki/Deadfish#C.23
    public class DeadfishInterpreter
    {
        private Dictionary<char, Func<int, int>> Commands { get; }

        private string Program { get; set; }

        public DeadfishInterpreter(Action<string> outputAction)
        {
            Commands = new Dictionary<char, Func<int, int>>
            {
                {'i', i => ++i},
                {'d', i => --i},
                {'s', i => i * i},
                {
                    'o', i =>
                    {
                        outputAction(i.ToString());
                        return i;
                    }
                }
            };
        }

        public void Parse(string s)
        {
            Program = s;
        }

        public void Execute()
        {
            int i = 0;
            foreach (char cmd in Program.Where(x => Commands.Keys.Contains(x)))
            {
                i = i == 256 || i < 0 
                    ? 0
                    : i;
                if (Commands.TryGetValue(cmd, out var func))
                    i = func(i);
            }
        }
    }
}
