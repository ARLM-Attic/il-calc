using System;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc.Custom
{
  /// <summary>
  /// This class for internal usage only.<br/>
  /// It becames at public surface only because
  /// of Silverlight reflections restrictions.
  /// </summary>
  public static class OwnerSupport
  {
    #region Fields

    internal static readonly Type OwnerNType = typeof(Closure);
    internal static readonly Type Owner2Type = typeof(Closure<,>);
    internal static readonly Type Owner3Type = typeof(Closure<,,>);

    internal static readonly BindingFlags FieldFlags =
      BindingFlags.Instance | BindingFlags.Public;

    internal static readonly FieldInfo OwnerNArray =
      OwnerNType.GetField("closure", FieldFlags);

    #endregion
    #region Closures

    /// <summary>For internal usage only.</summary>
    public sealed class Closure
    {
      /// <summary/>
      public object[] closure;

      /// <summary/>
      public Closure(object[] closure)
      {
        Debug.Assert(closure != null);
        this.closure = closure;
      }
    }

    /// <summary>For internal usage only.</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public sealed class Closure<T1, T2>
    {
      /// <summary/>
      public readonly T1 obj0;
      /// <summary/>
      public readonly T2 obj1;

      /// <summary/>
      public Closure(T1 o0, T2 o1)
      {
        this.obj0 = o0;
        this.obj1 = o1;
      }
    }

    /// <summary>For internal usage only.</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public sealed class Closure<T1, T2, T3>
    {
      /// <summary/>
      public readonly T1 obj0;
      /// <summary/>
      public readonly T2 obj1;
      /// <summary/>
      public readonly T3 obj2;

      /// <summary/>
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
