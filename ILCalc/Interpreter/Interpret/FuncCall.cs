using System;
using System.Diagnostics;
using System.Threading;

namespace ILCalc
{
  // TODO: rewrite to use range checks elimination

  [Serializable]
  sealed partial class FuncCall<T>
  {
    #region Fields

    readonly FunctionItem<T> func;
    readonly int lastIndex; // TODO: without it?

    readonly object[] fixArgs;
    readonly T[] varArgs;

    readonly object syncRoot;
    readonly int argsCount;

    #endregion
    #region Constructor

    public FuncCall(FunctionItem<T> f, int argsCount)
    {
      Debug.Assert(f != null);
      Debug.Assert(argsCount >= 0);
      Debug.Assert(
        ( f.HasParamArray && f.ArgsCount <= argsCount) ||
        (!f.HasParamArray && f.ArgsCount == argsCount));

      int fixCount = f.ArgsCount;

      if (f.HasParamArray)
      {
        this.varArgs = new T[argsCount - fixCount];
        this.fixArgs = new object[fixCount + 1];
        this.fixArgs[fixCount] = this.varArgs;
      }
      else this.fixArgs = new object[fixCount];

      this.func = f;
      this.lastIndex = fixCount - 1;
      this.argsCount = argsCount;
      this.syncRoot = new object();
    }

    #endregion
    #region Methods

    //TODO: new IsReusable?

    public void Invoke(T[] stack, ref int pos)
    {
      Debug.Assert(stack != null);
      Debug.Assert(stack.Length > pos);

      if (Monitor.TryEnter(this.syncRoot))
      {
        try
        {
          // fill parameters array:
          if (this.varArgs != null)
          {
            for (int i = this.varArgs.Length - 1; i >= 0; i--)
            {
              this.varArgs[i] = stack[pos--];
            }
          }

          // fill arguments:
          object[] fixTemp = this.fixArgs;
          for (int i = this.lastIndex; i >= 0; i--)
          {
            fixTemp[i] = stack[pos--];
          }

          // invoke via reflection:
          stack[++pos] = this.func.Invoke(fixTemp);
        }
        finally
        {
          Monitor.Exit(this.syncRoot);
        }
      }
      else
      {
        T result = this.func.Invoke(
          stack, pos, this.argsCount);

        pos -= this.argsCount - 1;
        stack[pos] = result; // TODO: is all right here?
      }
    }

    #endregion
  }
}