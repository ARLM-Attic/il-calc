using System;
using System.Diagnostics;
using System.Globalization;
using ILCalc.Custom;

namespace ILCalc
{
  // TODO: Trigger to use|not use output

  sealed partial class Parser<T>
  {
    #region Fields

    readonly CalcContext<T> context;
    IExpressionOutput<T> output;

    string expr;
    int xlen;

    char dotSymbol;
    char sepSymbol;

    NumberFormatInfo numFormat;

    static readonly
      ILiteralParser<T> Literal = LiteralParser.Resolve<T>();

    #endregion
    #region Methods

    public Parser(CalcContext<T> context)
    {
      Debug.Assert(context != null);

      this.context = context;
      InitCulture();
    }

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
      this.xlen = expression.Length;
      this.output = exprOutput;
      this.exprDepth = 0;
      this.prePos = 0;
      this.value = default(T);

      int i = 0;
      Parse(ref i, false);
    }

    public void InitCulture()
    {
      CultureInfo culture = Context.Culture;
      if (culture == null)
      {
        this.dotSymbol = '.';
        this.sepSymbol = ',';
        this.numFormat = new NumberFormatInfo();
      }
      else
      {
        try
        {
          this.dotSymbol =
            culture.NumberFormat.NumberDecimalSeparator[0];
          this.sepSymbol =
            culture.TextInfo.ListSeparator[0];
        }
        catch (IndexOutOfRangeException)
        {
          throw new ArgumentException(
            Resource.errCultureExtract);
        }

        this.numFormat = culture.NumberFormat;
      }
    }

    #endregion
    #region StaticData

    /////////////////////////////////////////
    // WARNING: do not modify items order! //
    /////////////////////////////////////////
    enum Item
    {
      Operator = 0,
      Separator = 1,
      Begin = 2,
      Number = 3,
      End = 4,
      Identifier = 5
    }

    const string Operators = "-+*/%^";

    static readonly int[] Priority = { 0, 0, 1, 1, 1, 3, 2 };

    #endregion
  }
}