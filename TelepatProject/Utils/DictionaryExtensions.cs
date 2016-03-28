using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelepatProject.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
                                        TKey key)
        {
            if (dictionary != null && dictionary.ContainsKey(key)) return dictionary[key];
            return default(TValue);
        }

        public static void Put<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
                                        TKey key, TValue value)
        {
            if (dictionary == null) return;
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }
    }
}
