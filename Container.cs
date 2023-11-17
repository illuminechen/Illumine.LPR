using System;
using System.Collections.Generic;
using System.Linq;

namespace Illumine.LPR
{
  public class Container
  {
    private static Dictionary<Type, object> Repository = new Dictionary<Type, object>();

    public static TValue Get<TValue>(int Key, TValue DefaultValue = default(TValue)) => Container.Get<int, TValue>(Key, DefaultValue);

    public static TValue Get<TKey, TValue>(TKey Key, TValue DefaultValue = default(TValue)) => Container.GetDictionary<TKey, TValue>().Get<TKey, TValue>(Key, DefaultValue);

    public static TValue Get<TValue>() => Container.Get<TValue>(0);

    public static TValue GetFrom<TValue>(int index) where TValue : IIndexData
    {
      Dictionary<int, List<TValue>> dictionary = Container.GetDictionary<int, List<TValue>>();
      if (!dictionary.ContainsKey(0))
        dictionary.Add(0, new List<TValue>());
      return dictionary[0].Find((Predicate<TValue>) (x => x.Id == index));
    }

    public static int PutInto<TValue>(TValue value, bool append = true) where TValue : IIndexData
    {
      Dictionary<int, List<TValue>> dictionary = Container.GetDictionary<int, List<TValue>>();
      if (!dictionary.ContainsKey(0))
        dictionary.Add(0, new List<TValue>());
      List<TValue> source = dictionary[0];
      int num = source.Count == 0 ? 1 : source.Max<TValue>((Func<TValue, int>) (x => x.Id)) + 1;
      value.Id = num;
      if (append)
        source.Add(value);
      else
        source.Insert(0, value);
      return num;
    }

    public static void Put<TValue>(TValue value) => Container.Put<int, TValue>(0, value);

    public static void Put<TKey, TValue>(TKey Key, TValue Value) => Container.GetDictionary<TKey, TValue>().Put<TKey, TValue>(Key, Value);

    private static void CreateDictIfNotExist<TKey, TValue>()
    {
      if (Container.Repository.ContainsKey(typeof (TValue)))
        return;
      Container.Repository.Add(typeof (TValue), (object) new Dictionary<TKey, TValue>());
    }

    private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
    {
      Container.CreateDictIfNotExist<TKey, TValue>();
      return (Dictionary<TKey, TValue>) Container.Repository[typeof (TValue)];
    }
  }
}
