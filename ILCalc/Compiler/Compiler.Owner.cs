using System;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
{
  static class OwnerSupport
  {
    #region Fields

    public static readonly Type OwnerNType = typeof(Closure);
    public static readonly Type Owner2Type = typeof(Closure<,>);
    public static readonly Type Owner3Type = typeof(Closure<,,>);

    public static readonly BindingFlags FieldFlags =
      BindingFlags.Instance | BindingFlags.NonPublic;

    public static readonly FieldInfo OwnerNArray =
      OwnerNType.GetField("closure", FieldFlags);

    #endregion
    #region Closures

    [Serializable]
    public sealed class Closure
    {
      object[] closure;

      public Closure(object[] closure)
      {
        Debug.Assert(closure != null);
        this.closure = closure;
      }
    }

    [Serializable]
    public sealed class Closure<T1, T2>
    {
      readonly T1 obj0;
      readonly T2 obj1;

      public Closure(T1 o0, T2 o1)
      {
        this.obj0 = o0;
        this.obj1 = o1;
      }
    }

    [Serializable]
    public sealed class Closure<T1, T2, T3>
    {
      readonly T1 obj0;
      readonly T2 obj1;
      readonly T3 obj2;

      public Closure(T1 o0, T2 o1, T3 o2)
      {
        this.obj0 = o0;
        this.obj1 = o1;
        this.obj2 = o2;
      }
    }

    #endregion
  }
}