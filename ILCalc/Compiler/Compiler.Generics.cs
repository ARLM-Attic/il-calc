using System;
using System.Reflection;
using System.Reflection.Emit;
using ILCalc.Custom;

namespace ILCalc
{
  static class CompilerSupport
  {
    #region Generics

    static readonly SupportCollection<object> Support;

    static CompilerSupport()
    {
      Support = new SupportCollection<object>();

      Support.Add<Int32>(new Int32ExprCompiler());
      Support.Add<Int64>(new Int64ExprCompiler());
      Support.Add<Single>(new SingleExprCompiler());
      Support.Add<Double>(new DoubleExprCompiler());
      //Support.Add<Decimal>(new DecimalExprCompiler());
    }

    public static ICompiler<T> Resolve<T>()
    {
      var compiler = (ICompiler<T>) Support.Find<T>();
      if (compiler == null)
        return new UnknownExprCompiler<T>();

      return compiler;
    }

    #endregion
    #region Compilers

    sealed class Int32ExprCompiler : ICompiler<Int32>
    {
      public void LoadConst(ILGenerator il, int value)
      {
        il.Emit(OpCodes.Ldc_I4, value);
      }

      public void Operation(ILGenerator il, int op)
      {
        if (op != Code.Pow)
          il.Emit(OpOperators[op]);
        else
          //TODO: fix it
          throw new NotSupportedException();
      }

      public void CheckedOp(ILGenerator il, int op)
      {
        Operation(il, op);
        il.Emit(OpCodes.Ckfinite);
      }

      public void LoadElem(ILGenerator il) { il.Emit(OpCodes.Ldelem_I4); }
      public void SaveElem(ILGenerator il) { il.Emit(OpCodes.Stelem_I4); }
    }

    sealed class Int64ExprCompiler : ICompiler<Int64>
    {
      public void LoadConst(ILGenerator il, long value)
      {
        //TODO: optimize
        il.Emit(OpCodes.Ldc_I8, value);
      }

      public void Operation(ILGenerator il, int op)
      {
        if (op != Code.Pow)
          il.Emit(OpOperators[op]);
        else
          //TODO: fix it
          throw new NotSupportedException();
      }

      public void CheckedOp(ILGenerator il, int op)
      {
        Operation(il, op);
        il.Emit(OpCodes.Ckfinite);
      }

      public void LoadElem(ILGenerator il) { il.Emit(OpCodes.Ldelem_I8); }
      public void SaveElem(ILGenerator il) { il.Emit(OpCodes.Stelem_I8); }
    }

    sealed class SingleExprCompiler : ICompiler<Single>
    {
      public void LoadConst(ILGenerator il, float value)
      {
        il.Emit(OpCodes.Ldc_R4, value);
      }

      public void Operation(ILGenerator il, int op)
      {
        if (op != Code.Pow)
             il.Emit(OpOperators[op]);
        else il.Emit(OpCodes.Call, PowMethodR4);
      }

      public void CheckedOp(ILGenerator il, int op)
      {
        Operation(il, op);
        il.Emit(OpCodes.Ckfinite);
      }

      public void LoadElem(ILGenerator il) { il.Emit(OpCodes.Ldelem_R4); }
      public void SaveElem(ILGenerator il) { il.Emit(OpCodes.Stelem_R4); }

      //NOTE: should not work at silverlight - visibility :(((
      public static float Pow(float x, float y)
      {
        return (float) Math.Pow(x, y);
      }

      static readonly MethodInfo PowMethodR4 =
        typeof(SingleExprCompiler).GetMethod("Pow", PublicStatic);
    }

    sealed class DoubleExprCompiler : ICompiler<Double>
    {
      public void LoadConst(ILGenerator il, double value)
      {
        il.Emit(OpCodes.Ldc_R8, value);
      }

      public void Operation(ILGenerator il, int op)
      {
        if (op != Code.Pow)
             il.Emit(OpOperators[op]);
        else il.Emit(OpCodes.Call, PowMethodR8);
      }

      public void CheckedOp(ILGenerator il, int op)
      {
        Operation(il, op);
        il.Emit(OpCodes.Ckfinite);
      }

      public void LoadElem(ILGenerator il) { il.Emit(OpCodes.Ldelem_R8); }
      public void SaveElem(ILGenerator il) { il.Emit(OpCodes.Stelem_R8); }

      static readonly MethodInfo PowMethodR8 =
        typeof(Math).GetMethod("Pow");
    }

    sealed class DecimalExprCompiler : ICompiler<Decimal>
    {
      const decimal MinI4 = Int32.MinValue;
      const decimal MaxI4 = Int32.MaxValue;
      const decimal MaxI8 = Int64.MaxValue;
      const decimal MinI8 = Int64.MinValue;

      static readonly Type DecimalType = typeof(Decimal);

      static readonly MethodInfo[] Operators = new[]
      {
        DecimalType.GetMethod("Subtract", PublicStatic),
        DecimalType.GetMethod("Add", PublicStatic),
        DecimalType.GetMethod("Multiply", PublicStatic),
        DecimalType.GetMethod("Divide", PublicStatic),
        DecimalType.GetMethod("Remainder", PublicStatic),
        typeof(DecimalExprCompiler).GetMethod("Pow", PublicStatic),
        DecimalType.GetMethod("Negate", PublicStatic),
      };

      public void LoadConst(ILGenerator il, decimal value)
      {
        throw new NotImplementedException();
      }

      public void Operation(ILGenerator il, int op)
      {
        il.Emit(OpCodes.Call, Operators[op]);
      }

      public void CheckedOp(ILGenerator il, int op) { }

      public void LoadElem(ILGenerator il) { }
      public void SaveElem(ILGenerator il) { }

      //TODO: replace with better impl
      public static decimal Pow(decimal x, decimal y)
      {
        return (decimal) Math.Pow((double) x, (double) y);
      }
    }

    sealed class UnknownExprCompiler<T> : ICompiler<T>
    {
      static Exception MakeException(string name)
      {
        return new NotSupportedException(string.Format(
          Resource.errNotSupported, name, typeof(T)));
      }

      const string Operators = "-+*/%^";

      public void LoadConst(ILGenerator il, T value)
      {
        throw MakeException("load const");
      }

      public void Operation(ILGenerator il, int op)
      {
        throw MakeException(Operators[op].ToString());
      }

      public void CheckedOp(ILGenerator il, int op)
      {
        throw MakeException(Operators[op].ToString());
      }

      public void LoadElem(ILGenerator il) { throw MakeException("load elem"); }
      public void SaveElem(ILGenerator il) { throw MakeException("store elem"); }
    }

    #endregion
    #region CommonData

    const BindingFlags PublicStatic =
      BindingFlags.Public | BindingFlags.Static;

    static readonly OpCode[] OpOperators =
    {
      OpCodes.Sub, OpCodes.Add,
      OpCodes.Mul, OpCodes.Div,
      OpCodes.Rem, OpCodes.Nop,
      OpCodes.Neg
    };

    #endregion
  }
}