using System;
using System.Diagnostics;
using System.Reflection;
using ILCalc.Custom;
#if SILVERLIGHT
using System.Linq.Expressions;
#endif

namespace ILCalc
{
  sealed class QuickInterpretImpl<T, TSupport>
    : QuickInterpret<T>
    where TSupport : IArithmetic<T>, new()
  {
    #region Fields

    static readonly TSupport Generic = new TSupport();

    #endregion
    #region Constructor

    public QuickInterpretImpl(T[] arguments)
      : base(arguments) { }

    public static QInterpFactory<T> GetDelegate()
    {
      return args =>
        new QuickInterpretImpl<T, TSupport>(args);
    }

    #endregion
    #region IExpressionOutput

    public override void PutOperator(int oper)
    {
      Debug.Assert(Code.IsOperator(oper));
      Debug.Assert(this.pos >= 0);

      T value = this.stack[this.pos];
      if (oper != Code.Neg)
      {
        Debug.Assert(this.pos >= 0);
        Debug.Assert(this.pos < this.stack.Length);

        T temp = this.stack[--this.pos];

        if      (oper == Code.Add) temp = Generic.Add(temp, value);
        else if (oper == Code.Mul) temp = Generic.Mul(temp, value);
        else if (oper == Code.Sub) temp = Generic.Sub(temp, value);
        else if (oper == Code.Div) temp = Generic.Div(temp, value);
        else if (oper == Code.Mod) temp = Generic.Mod(temp, value);
        else
          temp = Generic.Pow(temp, value);

        this.stack[this.pos] = temp;
      }
      else
      {
        this.stack[this.pos] = Generic.Neg(value);
      }
    }

    public override int? IsIntegral(T value)
    {
      return Generic.IsIntergal(value);
    }

    #endregion
  }

  delegate QuickInterpret<T> QInterpFactory<T>(T[] arguments);

  static class QuickInterpretHelper
  {
    static readonly Type
      InterpType = typeof(QuickInterpretImpl<,>);

#if SILVERLIGHT

    public static QInterpFactory<T> GetFactory<T>(Type ar)
    {
      var argsType = typeof(T[]);

      var ctor = InterpType
        .MakeGenericType(typeof(T), ar)
        .GetConstructor(new[] { argsType });

      var argsParam = Expression.Parameter(argsType, "arguments");

      var func = Expression.Lambda<QInterpFactory<T>>(
        Expression.New(ctor, argsParam), argsParam);

      return func.Compile();
    }

#else

    const BindingFlags PublicStatic =
      BindingFlags.Static | BindingFlags.Public;

    public static QInterpFactory<T> GetFactory<T>(Type ar)
    {
      return (QInterpFactory<T>) InterpType
        .MakeGenericType(typeof(T), ar)
        .GetMethod("GetDelegate", PublicStatic)
        .Invoke(null, null);
    }

#endif
  }
}