using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
{
  static class TypeHelper<T>
  {
    #region Fields

    public static readonly Type ValueType = typeof(T);
    public static readonly Type ArrayType = typeof(T[]);

    public static readonly Type Func0 = typeof(EvalFunc0<T>);
    public static readonly Type Func1 = typeof(EvalFunc1<T>);
    public static readonly Type Func2 = typeof(EvalFunc2<T>);
    public static readonly Type Func3 = typeof(EvalFuncN<T>);

    static readonly List<Type>
      TypesList = new List<Type> { ArrayType };

    #endregion
    #region Methods

    public static Type GetArrayType(int rank)
    {
      Debug.Assert(rank > 0);

      if (rank >= TypesList.Count)
      {
        lock (((ICollection) TypesList).SyncRoot)
        {
          int count = rank - TypesList.Count;
          Type last = TypesList[TypesList.Count - 1];
#if CF
          var buf = new System.Text.StringBuilder(last.FullName);
          for (int i = 0; i < count; i++)
          {
            buf.Append("[]");
            TypesList.Add(Type.GetType(buf.ToString()));
          }
#else
          for (int i = 0; i < count; i++)
          {
            last = last.MakeArrayType();
            TypesList.Add(last);
          }
#endif
        }
      }

      return TypesList[rank - 1];
    }

    #endregion
  }
}