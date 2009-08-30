using System;
using System.Globalization;
using System.Text;

namespace ILCalc
{
  interface IParserSupport<T>
  {
    string Expression { get; }
    int BeginPos { get; }
    char DecimalDot { get; }
    NumberFormatInfo NumberFormat { get; }

    T ParsedValue { set; }

    Exception InvalidNumberFormat(
      string message,
      string badLiteral,
      Exception innerException);
  }

  interface INewLiteralParser<T>
  {
    int TryParse(int i, IParserSupport<T> p);
  }

  sealed class NewDoubleParser : INewLiteralParser<Double>
  {
    public int TryParse(int i, IParserSupport<double> p)
    {
      string expr = p.Expression;

      if ((expr[i] < '0' || expr[i] > '9')
        && expr[i] != p.DecimalDot)
      {
        return -1;
      }

      //int begin = i;
      // Fractal part: =====================

      // numbers not like ".123"
      if (expr[i] != p.DecimalDot)
      {
        // skip digits and decimal point
        for (; i < expr.Length; i++)
        {
          if (char.IsDigit(expr[i])) continue;
          if (expr[i] == p.DecimalDot) i++;
          break;
        }
      }

      // skip digits
      while (i < expr.Length
        && char.IsDigit(expr[i])) i++;

      // Exponental part: ==================

      // at least 2 chars
      if (i + 1 < expr.Length)
      {
        // E character
        char c = expr[i];
        if (c == 'e' || c == 'E')
        {
          int j = i;

          // exponetal sign
          c = expr[++j];
          if (c == '-' || c == '+') j++;

          // eponental part
          if (i < expr.Length && char.IsDigit(expr[j]))
          {
            j++;
            while (j < expr.Length && char.IsDigit(expr[j])) j++;
            i = j;
          }
        }
      }

      // Try to parse: =============

      // extract number substring
      string number = expr.Substring(p.BeginPos, i - p.BeginPos);
      try
      {
        p.ParsedValue = Double.Parse(
          number,
          NumberStyles.AllowDecimalPoint |
          NumberStyles.AllowExponent);

        return number.Length;
      }
      catch (FormatException e)
      {
        throw p.InvalidNumberFormat(
          Resource.errNumberFormat, number, e);
      }
      catch (OverflowException e)
      {
        throw p.InvalidNumberFormat(
          Resource.errNumberOverflow, number, e);
      }
    }
  }

  sealed class Int32LiteralParser : INewLiteralParser<Int32>
  {
    public int TryParse(int i, IParserSupport<int> p)
    {
      string expr = p.Expression;

      if (expr[i] < '0' || expr[i] > '9')
      {
        return -1;
      }

      // skip digits
      while (i < expr.Length && char.IsDigit(expr[i])) i++;

      // Try to parse: =============

      // extract number substring
      string number = expr.Substring(p.BeginPos, i - p.BeginPos);
      try
      {
        p.ParsedValue = Int32.Parse(
          number,
          NumberStyles.AllowDecimalPoint |
          NumberStyles.AllowExponent);

        return number.Length;
      }
      catch (FormatException e)
      {
        throw p.InvalidNumberFormat(
          Resource.errNumberFormat, number, e);
      }
      catch (OverflowException e)
      {
        throw p.InvalidNumberFormat(
          Resource.errNumberOverflow, number, e);
      }
    }
  }

  static class Parser
  {
    static readonly SupportCollection<object> Support;

    static Parser()
    {
      Support = new SupportCollection<object>(); //TODO: make заглушка

      Support.Add<Double>(new NewDoubleParser());
      Support.Add<Int32>(new Int32LiteralParser());
    }

    public static INewLiteralParser<T> Resolve<T>()
    {
#if CF
      return null;
#else
      return (INewLiteralParser<T>) Support.Find<T>();
#endif
    }
  }

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