using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            var result = default(TValue);
            if (key != null)
            {
                dictionary.TryGetValue(key, out result);
            }
            return result;
        }
    }
}
