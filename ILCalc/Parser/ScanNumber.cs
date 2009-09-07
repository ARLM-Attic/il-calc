using System;
using System.Globalization;
using System.Text;
using ILCalc.Custom;

namespace ILCalc
{
  //TODO: ScanLiteral?
  //TODO: fix resource "Number constant overflow"

  sealed partial class Parser<T> : IParserSupport<T>
  {
    #region IParserSupport<T> Members

    public string Expression { get { return this.expr; } }

    public int BeginPos { get { return this.curPos; } }

    public char DecimalDot { get { return this.dotSymbol; } }

    public NumberFormatInfo NumberFormat
    {
      get { return this.numFormat; }
    }

    public T ParsedValue
    {
      set { this.value = value; }
    }

    public Exception InvalidNumberFormat(
      string message,
      string badLiteral,
      Exception innerException)
    {
      var buf = new StringBuilder(message);

      buf.Append(" \"");
      buf.Append(badLiteral);
      buf.Append("\".");

      return new SyntaxException(
          buf.ToString(),
          this.expr,
          this.curPos,
          badLiteral.Length,
          innerException);
    }

    #endregion
  }
}