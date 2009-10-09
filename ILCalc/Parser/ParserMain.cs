using System.Diagnostics;
using ILCalc.Custom;

namespace ILCalc
{
  sealed partial class Parser<T>
  {
    #region Fields

    readonly CalcContext<T> context;
    readonly IListEnumerable[] literals;

    IExpressionOutput<T> output;
    string expr;

    static readonly ILiteralParser<T>
      Literal = LiteralParser.Resolve<T>();

    #endregion
    #region Constructor

    public Parser(CalcContext<T> context)
    {
      Debug.Assert(context != null);

      this.context = context;
      this.literals = new IListEnumerable[]
      {
        context.Arguments,
        context.Constants,
        context.Functions
      };

      InitCulture();
    }

    #endregion
    #region Properties

    CalcContext<T> Context
    {
      get { return this.context; }
    }

    IExpressionOutput<T> Output
    {
      get { return this.output; }
    }

    public void Parse(
      string expression, IExpressionOutput<T> exprOutput)
    {
      Debug.Assert(expression != null);
      Debug.Assert(exprOutput != null);

      this.expr = expression;
      //this.xlen = expression.Length;
      this.output = exprOutput;
      this.exprDepth = 0;
      this.prePos = 0;
      this.value = default(T);

      int i = 0;
      Parse(ref i, false);
    }

    #endregion
    #region StaticData

    /////////////////////////////////////////
    // WARNING: do not modify items order! //
    /////////////////////////////////////////
    enum Item
    {
      Operator   = 0,
      Separator  = 1,
      Begin      = 2,
      Number     = 3,
      End        = 4,
      Identifier = 5,
    }

    const string Operators = "-+*/%^";

    static readonly int[] Priority = { 0, 0, 1, 1, 1, 3, 2 };

    #endregion
  }
}