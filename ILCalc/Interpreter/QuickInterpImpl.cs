using System.Diagnostics;
using ILCalc.Custom;

namespace ILCalc
{
  sealed class QuickInterpretImpl<T, TSupport> : QuickInterpret<T>
    where TSupport : IArithmetic<T>, new()
  {
    #region Fields

    static readonly TSupport Generic = new TSupport();

    #endregion
    #region Constructor

    public QuickInterpretImpl(T[] arguments)
      : base(arguments) { }

    #endregion
    #region IExpressionOutput

    public override void PutOperator(Code oper)
    {
      Debug.Assert(CodeHelper.IsOp(oper));
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
        else temp = Generic.Pow(temp, value);

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
}