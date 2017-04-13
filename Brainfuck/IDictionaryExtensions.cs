using System.Collections.Generic;

namespace Brainfuck
{
    public static class IDictionaryExtensions
    {
        public static TValue Get<TKeyK, TValue>(this IDictionary<TKeyK, TValue> dict, TKeyK key, TValue defaultValue)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
                value = defaultValue;
            return value;
        }
    }
}
