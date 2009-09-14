using System.Collections.Generic;
using System.Diagnostics;

// TODO: feature x ^  0		=> 1
// TODO: feature x ^ -2		=> 1 x x * /
// TODO: feature x *  0		=> 0
// TODO: feature 2 + x + 2	=> 4 x +

namespace ILCalc
{
  sealed class OptimizeOutput<T>
    : BufferOutput<T>,
      IExpressionOutput<T>
  {
    #region Fields

    readonly IExpressionOutput<T> output;
    readonly QuickInterpret<T> interp;
    readonly OptimizeModes mode;

    #endregion
    #region Constructor

    //TODO: checks!
    public OptimizeOutput(
      IExpressionOutput<T> output, OptimizeModes mode)
    {
      Debug.Assert(output != null);

      this.output = output;
      this.mode = mode;

      this.interp = QuickInterpret<T>.CreateInstance(null);
    }

    #endregion
    #region Properties

    bool ConstantFolding
    {
      get { return (this.mode & OptimizeModes.ConstantFolding) != 0; }
    }

    bool FuncionFolding
    {
      get { return (this.mode & OptimizeModes.FunctionFolding) != 0; }
    }

    bool PowOptimize
    {
      get { return (this.mode & OptimizeModes.PowOptimize) != 0; }
    }

    T LastNumber
    {
      get { return this.numbers[this.numbers.Count - 1]; }
      set { this.numbers[this.numbers.Count - 1] = value; }
    }

    #endregion
    #region IExpressionOutput

    public new void PutOperator(int oper)
    {
      Debug.Assert(Code.IsOperator(oper));

      if (ConstantFolding)
      {
        // Unary operator optimize ======================
        if (oper == Code.Neg && IsLastKnown())
        {
          PerformNegate();
          return;
        }

        // Binary operator optimize =====================
        if (IsLastTwoKnown())
        {
          PerformBinaryOp(oper);
          return;
        }

        // Power operator optimize ======================
        Debug.Assert(this.code.Count >= 2);

        if (oper == Code.Pow && PowOptimize &&
          LastValue(code, 1) == Code.Number &&
          LastValue(code, 2) == Code.Argument)
        {
          int? val = this.interp.IsIntegral(LastNumber);
          if (val.HasValue)
          {
            int value = val.Value;
            //int value = -1; //GetIntegerValue(LastNumber);
            if (value > 0 && value < 16)
            {
              OptimizePow(value);
              return;
            }
          }
        }
      }

      code.Add(oper);
    }

    public new void PutCall(FunctionInfo<T> func, int argsCount)
    {
      if (FuncionFolding)
      {
        int pos = code.Count - 1;
        bool allArgsKnown = true;

        while (!IsCallBegin(pos))
        {
          if (code[pos--] == Code.Number)
          {
            if (code[pos] == Code.Separator) pos--;
          }
          else
          {
            allArgsKnown = false;
            break;
          }
        }

        if (allArgsKnown)
        {
          FoldFunction(pos, func, argsCount);
          return;
        }
      }

      base.PutCall(func, argsCount);
    }

    public new void PutExprEnd()
    {
      base.PutExprEnd();
      WriteTo(this.output);
      this.output.PutExprEnd();
    }

    #endregion
    #region Helpers

//    static int GetIntegerValue(double value)
//    {
//      var intVal = (int) value;
//      return (intVal == value) ? intVal : -1;
//    }

    // ReSharper disable SuggestBaseTypeForParameter

    static U LastValue<U>(List<U> list, int id)
    {
      Debug.Assert(id <= list.Count);
      return list[list.Count - id];
    }

    static void RemoveLast<U>(List<U> list)
    {
      Debug.Assert(list.Count >= 1);
      list.RemoveAt(list.Count - 1);
    }

    // ReSharper restore SuggestBaseTypeForParameter

    bool IsLastKnown()
    {
      Debug.Assert(code.Count >= 1);
      return code[code.Count - 1] == Code.Number;
    }

    bool IsLastTwoKnown()
    {
      Debug.Assert(code.Count >= 2);
      int index = code.Count;
      return code[index - 1] == Code.Number
          && code[index - 2] == Code.Number;
    }

    bool IsCallBegin(int pos)
    {
      Debug.Assert(code.Count >= 1);

      int op = code[pos];
      return op == Code.ParamCall
          || op == Code.BeginCall;
    }

    #endregion
    #region Optimizations

    void PerformNegate()
    {
      Debug.Assert(this.numbers.Count >= 1);

      this.interp.PutConstant(LastNumber);
      this.interp.PutOperator(Code.Neg);

      LastNumber = this.interp.Result;

      this.interp.Reset();
    }

    void PerformBinaryOp(int oper)
    {
      Debug.Assert(this.numbers.Count >= 2);
      Debug.Assert(this.code.Count >= 1);

      this.interp.PutConstant(LastValue(this.numbers, 2));
      this.interp.PutConstant(LastValue(this.numbers, 1));
      this.interp.PutOperator(oper);

      RemoveLast(this.numbers);
      RemoveLast(code);

      LastNumber = this.interp.Result;

      this.interp.Reset();
    }

    void FoldFunction(int start, FunctionInfo<T> func, int argsCount)
    {
      Debug.Assert(start >= 0);
      Debug.Assert(func != null);
      Debug.Assert(argsCount >= 0);
      Debug.Assert(argsCount <= this.numbers.Count);

      int numIndex = this.numbers.Count - argsCount;
      var stack = new T[argsCount];
      this.numbers.CopyTo(numIndex, stack, 0, argsCount);

      if (code[start] == Code.ParamCall)
      {
        Debug.Assert(data.Count >= 2);
        data.RemoveRange(data.Count - 2, 2);
      }

      Debug.Assert(this.code.Count > start);
      this.code.RemoveRange(start, code.Count - start);

      if (argsCount > 0)
      {
        Debug.Assert(this.numbers.Count > numIndex);
        this.numbers.RemoveRange(numIndex, argsCount);
      }

      PutConstant(func.Invoke(stack, argsCount - 1, argsCount));
    }

    void OptimizePow(int value)
    {
      Debug.Assert(value > 0);
      Debug.Assert(this.numbers.Count >= 1);
      Debug.Assert(this.code.Count >= 1);

      RemoveLast(this.numbers);
      RemoveLast(code);
      int argumentId = LastValue(data, 1);

      for (int i = 1; i < value; i++)
      {
        PutArgument(argumentId);
        base.PutOperator(Code.Mul);
      }
    }

    #endregion
  }
}