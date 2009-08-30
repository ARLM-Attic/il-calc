using System;
using System.Collections.Generic;

namespace ILCalc
{
  sealed class SupportCollection<TItem>
    where TItem : class
  {
    readonly List<Type> types;
    readonly List<TItem> items;

    public SupportCollection() : this(0) { }

    public SupportCollection(int capacity)
    {
      this.types = new List<Type>(capacity);
      this.items = new List<TItem>(capacity);
    }

    public void Add<T>(TItem item)
    {
      lock (this.types)
      {
        this.types.Add(typeof(T));
        this.items.Add(item);
      }
    }

    public TItem Find<T>()
    {
      Type type = typeof(T);

      lock (this.types)
      for (int i = 0; i < this.types.Count; i++)
      {
        if (this.types[i] == type)
        {
          return this.items[i];
        }
      }

      return null;
    }
  }
}