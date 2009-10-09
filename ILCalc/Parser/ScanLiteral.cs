using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ILCalc.Custom;

namespace ILCalc
{
  sealed partial class Parser<T> : IParserSupport<T>
  {
    #region CultureSupport

    char dotSymbol;
    char sepSymbol;

    NumberFormatInfo numFormat;

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
          this.dotSymbol = culture.NumberFormat.NumberDecimalSeparator[0];
          this.sepSymbol = culture.TextInfo.ListSeparator[0];
        }
        catch (IndexOutOfRangeException)
        {
          throw new ArgumentException(Resource.errCultureExtract);
        }

        this.numFormat = culture.NumberFormat;
      }
    }

    #endregion
    #region IParserSupport

    public int BeginPos { get { return this.curPos; } }
    
    public string Expression { get { return this.expr; } }
    
    public T ParsedValue { set { this.value = value; } }

    public char DecimalDot { get { return this.dotSymbol; } }

    public NumberFormatInfo NumberFormat
    {
      get { return this.numFormat; }
    }

    public Exception InvalidNumberFormat(
      string message, string literal, Exception exc)
    {
      var msg = new StringBuilder(message)
        .Append(" \"").Append(literal)
        .Append("\".").ToString();

      return new SyntaxException(
        msg, this.expr, this.curPos,
        literal.Length, exc);
    }

    public bool DiscardNegate()
    {
      Debug.Assert(this.curStack != null);

      if (this.curStack.Count == 0 ||
          this.curStack.Peek() != Code.Neg)
      {
        return false;
      }

      bool neg = false;
      while (this.curStack.Count > 0 &&
             this.curStack.Peek() == Code.Neg)
      {
        neg = !neg;
        this.curStack.Pop();
      }

      return neg;
    }

    #endregion
  }
}