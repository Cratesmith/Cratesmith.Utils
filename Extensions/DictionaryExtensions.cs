using System.Collections.Generic;

namespace Cratesmith.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue notFoundValue=default(TValue))
        {
            TValue output = default;
            if (@this.TryGetValue(key, out output))
            {
                return output;
            }

            return notFoundValue;
        }
    }
}