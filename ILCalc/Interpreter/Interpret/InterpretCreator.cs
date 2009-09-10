using System;
using System.Diagnostics;

namespace ILCalc
{
  sealed class InterpretCreator<T>
    : IExpressionOutput<T>
  {
    #region Fields

    int npos, cpos, fpos;

    T[] nums;
    int[] codes;
    object[] funcs;

    static readonly object[]
      EmptyFuncs = new object[0];

    int stackMax, stackSize;

    #endregion
    #region Constructor

    public InterpretCreator()
    {
      this.codes = new int[8];
      this.funcs = EmptyFuncs;
      this.nums = new T[4];
    }

    #endregion
    #region Properties

    public int[] Codes
    {
      get { return this.codes; }
    }

    public T[] Numbers
    {
      get { return this.nums; }
    }

    public object[] Functions
    {
      get { return this.funcs; }
    }

    public int StackMax
    {
      get { return this.stackMax; }
    }

    #endregion
    #region IExpressionOutput

    public void PutConstant(T value)
    {
      AddCode(Code.Number);

      if (this.npos == this.nums.Length)
      {
        ExpandArray(ref this.nums);
      }

      this.nums[this.npos++] = value;

      if (++this.stackSize > this.stackMax)
      {
        this.stackMax = this.stackSize;
      }
    }

    public void PutArgument(int id)
    {
      Debug.Assert(id >= 0);
      AddCode(Code.Argument);
      AddCode(id);

      if (++this.stackSize > this.stackMax)
      {
        this.stackMax = this.stackSize;
      }
    }

    public void PutOperator(int oper)
    {
      Debug.Assert(Code.IsOperator(oper));
      AddCode(oper);

      if (oper != Code.Neg)
      {
        this.stackSize--;
      }
    }

    public void PutSeparator() { }
    public void PutBeginCall() { }

    public void PutCall(
      FunctionItem<T> func, int argsCount)
    {
      Debug.Assert(func != null);
      Debug.Assert(argsCount >= 0);

#if CF2
			AddFunc(func, argsCount);
			AddCode(Code.Function);
			RecalcStackSize(argsCount);
#else
      if (func.HasParamArray || func.ArgsCount > 2)
      {
        AddFunc(func, argsCount);
        AddCode(Code.Function);
        RecalcStackSize(argsCount);
        return;
      }

      switch (func.ArgsCount)
      {
        case 0:
          AddDelegate(func, EvalType0);
          AddCode(Code.Delegate0);

          if (++this.stackSize > this.stackMax)
          {
            this.stackMax = this.stackSize;
          }

          break;

        case 1:
          AddDelegate(func, EvalType1);
          AddCode(Code.Delegate1);
          break;

        case 2:
          AddDelegate(func, EvalType2);
          AddCode(Code.Delegate2);
          this.stackSize--;
          break;
      }
#endif
    }

    public void PutExprEnd()
    {
      AddCode(Code.Return);
    }

    #endregion
    #region Helpers

    void AddCode(int code)
    {
      if (this.cpos == this.codes.Length)
      {
        ExpandArray(ref this.codes);
      }

      this.codes[this.cpos++] = code;
    }

    static void ExpandArray<U>(ref U[] src)
    {
      int size = (src.Length == 0) ? 4 : src.Length * 2;
      var dest = new U[size];

      if (src.Length > 0)
      {
        Array.Copy(src, 0, dest, 0, src.Length);
      }

      src = dest;
    }

    void RecalcStackSize(int argsCount)
    {
      Debug.Assert(argsCount >= 0);

      if (argsCount == 0)
      {
        if (++this.stackSize > this.stackMax)
        {
          this.stackMax = this.stackSize;
        }
      }
      else this.stackSize -= argsCount - 1;
    }

#if !CF2

    void AddDelegate(FunctionItem<T> func, Type delegType)
    {
      Debug.Assert(func != null);
      Debug.Assert(delegType != null);

      if (this.fpos == this.funcs.Length)
      {
        ExpandArray(ref this.funcs);
      }

      this.funcs[this.fpos++] =
        Delegate.CreateDelegate(
          delegType, func.Target, func.Method);
    }

#endif

    void AddFunc(FunctionItem<T> func, int argsCount)
    {
      Debug.Assert(func != null);
      Debug.Assert(argsCount >= 0);

      if (this.fpos == this.funcs.Length)
      {
        ExpandArray(ref this.funcs);
      }

      this.funcs[this.fpos++] =
        new FuncCall<T>(func, argsCount);
    }

    #endregion
    #region Static Data

    static readonly Type EvalType0 = typeof(EvalFunc0<T>);
    static readonly Type EvalType1 = typeof(EvalFunc1<T>);
    static readonly Type EvalType2 = typeof(EvalFunc2<T>);

    #endregion
  }
}