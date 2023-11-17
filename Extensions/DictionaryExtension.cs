using System.Collections.Generic;

namespace Illumine.LPR
{
    public static class DictionaryExtension
    {
        public static void Put<TKey, TValue>(
          this Dictionary<TKey, TValue> Dict,
          TKey Key,
          TValue Value)
        {
            if (Dict.ContainsKey(Key))
                Dict[Key] = Value;
            else
                Dict.Add(Key, Value);
        }

        public static TValue Get<TKey, TValue>(
          this Dictionary<TKey, TValue> Dict,
          TKey Key,
          TValue DefaultValue = default(TValue))
        {
            return Dict.ContainsKey(Key) ? Dict[Key] : DefaultValue;
        }
    }
}
