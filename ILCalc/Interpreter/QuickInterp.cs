using System;
using System.Diagnostics;

namespace ILCalc
{
  abstract class QuickInterpret<T>
    : IExpressionOutput<T>
  {
    #region Fields

    readonly T[] arguments;
    protected T[] stack;
    protected int pos;

    #endregion
    #region Constructor

    protected QuickInterpret(T[] arguments)
    {
      this.stack = new T[4];
      this.arguments = arguments;
      this.pos = -1;
    }

    #endregion
    #region Methods

    public T Result
    {
      get
      {
        Debug.Assert(this.pos == 0);
        return this.stack[0];
      }
    }

    public void Reset()
    {
      this.pos = -1;
    }

    #endregion
    #region IExpressionOutput

    public void PutConstant(T value)
    {
      if (++this.pos == this.stack.Length)
      {
        var realloc = new T[this.pos * 2];
        Array.Copy(this.stack, 0, realloc, 0, this.pos);
        this.stack = realloc;
      }

      this.stack[this.pos] = value;
    }

    public void PutArgument(int id)
    {
      Debug.Assert(id >= 0);

      if (++this.pos == this.stack.Length)
      {
        var realloc = new T[this.pos * 2];
        Array.Copy(this.stack, 0, realloc, 0, this.pos);
        this.stack = realloc;
      }

      this.stack[this.pos] = this.arguments[id];
    }

    public void PutBeginCall() { }
    public void PutSeparator() { }

    public void PutCall(FunctionInfo<T> func, int argsCount)
    {
      Debug.Assert(this.pos + 1 >= argsCount);

      T result = func.Invoke(this.stack, this.pos, argsCount);
      this.pos -= argsCount;

      if (argsCount > 0)
      {
        this.stack[++this.pos] = result;
      }
      else PutConstant(result);
    }

    public void PutExprEnd() { }

    public abstract void PutOperator(int oper);

    public abstract int? IsIntegral(T value);

    #endregion
    #region Creation

    internal static readonly QInterpFactory<T> CreateInstance;
    internal static readonly QInterpFactory<T> CreateChecked;

    static QuickInterpret()
    {
      Type ar = Arithmetics.Resolve<T>(true);
      if (ar != null)
        CreateChecked = QuickInterpretHelper.GetFactory<T>(ar);

      ar = Arithmetics.Resolve<T>(false);
      if (ar == null)
      {
        // TODO: good message
        throw new NotSupportedException(
          "Type " + typeof(T) + " is not supported.");
      }

      CreateInstance = QuickInterpretHelper.GetFactory<T>(ar);

      if (CreateChecked == null)
        CreateChecked = CreateInstance;
    }

    #endregion
  }
}