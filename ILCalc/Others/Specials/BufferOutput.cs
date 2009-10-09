using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
{
  [Serializable]
  abstract class BufferOutput<T> : IExpressionOutput<T>
  {
    #region Fields

    protected readonly List<FunctionInfo<T>> functions;
    protected readonly List<T> numbers;
    protected readonly List<Code> code;
    protected readonly List<int> data;

    #endregion
    #region Constructor

    protected BufferOutput()
    {
      this.functions = new List<FunctionInfo<T>>(2);
      this.numbers = new List<T>(4);
      this.code = new List<Code>(8);
      this.data = new List<int>(2);
    }

    #endregion
    #region IExpressionOutput

    public void PutConstant(T value)
    {
      this.code.Add(Code.Number);
      this.numbers.Add(value);
    }

    public void PutOperator(Code oper)
    {
      Debug.Assert(CodeHelper.IsOp(oper));

      this.code.Add(oper);
    }

    public void PutArgument(int id)
    {
      Debug.Assert(id >= 0);

      this.code.Add(Code.Argument);
      this.data.Add(id);
    }

    public void PutSeparator()
    {
      this.code.Add(Code.Separator);
    }

    public void PutBeginCall()
    {
      this.code.Add(Code.BeginCall);
    }

    public void PutCall(FunctionInfo<T> func, int args)
    {
      Debug.Assert(func != null);
      Debug.Assert(args >= 0);

      this.code.Add(Code.Function);
      this.data.Add(args);
      this.functions.Add(func);
    }

    public void PutExprEnd()
    {
      this.code.Add(Code.Return);
    }

    #endregion
    #region Methods

    public void WriteTo(IExpressionOutput<T> output)
    {
      int i = 0, n = 0,
          f = 0, d = 0;

      while (true)
      {
        Code op = this.code[i++];

        if (CodeHelper.IsOp(op))      output.PutOperator(op);
        else if (op == Code.Number)   output.PutConstant(this.numbers[n++]);
        else if (op == Code.Argument) output.PutArgument(this.data[d++]);
        else if (op == Code.Function) output.PutCall(
          this.functions[f++], this.data[d++]);
        else if (op == Code.Separator) output.PutSeparator();
        else if (op == Code.BeginCall) output.PutBeginCall();
        else { output.PutExprEnd(); break; }
      }
    }

    #endregion
  }
}