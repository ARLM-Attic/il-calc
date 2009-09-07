using System;
using System.Globalization;
using ILCalc.Custom;

namespace ILCalc
{
  static class LiteralParser
  {
    #region Resolve

    static readonly SupportCollection<object> Support;

    static LiteralParser()
    {
      Support = new SupportCollection<object>();

      var realParser = new RealLiteralParser();
      Support.Add<Double>(realParser);
      Support.Add<Single>(realParser);
      Support.Add<Decimal>(realParser);

      Support.Add<Int32>(new Int32LiteralParser());
      Support.Add<Int64>(new Int64LiteralParser());
    }

    public static ILiteralParser<T> Resolve<T>()
    {
      var parser = Support.Find<T>();
      if (parser == null)
        return new UnknownLiteralParser<T>();

      return (ILiteralParser<T>) parser;
    }

    public static bool IsUnknown<T>(ILiteralParser<T> parser)
    {
      return parser is UnknownLiteralParser<T>;
    }

    #endregion
    #region Parsers

    sealed class RealLiteralParser
      : ILiteralParser<Double>,
        ILiteralParser<Single>,
        ILiteralParser<Decimal>
    {
      delegate T Parser<T>(string s, NumberStyles style);

      public int TryParse(int i, IParserSupport<float> p)
      {
        string expr = p.Expression;

        if ((expr[i] < '0' || expr[i] > '9') &&
             expr[i] != p.DecimalDot)
        {
          return -1;
        }

        return Parse(i, p, Single.Parse);
      }

      public int TryParse(int i, IParserSupport<double> p)
      {
        string expr = p.Expression;

        if ((expr[i] < '0' || expr[i] > '9') &&
             expr[i] != p.DecimalDot)
        {
          return -1;
        }

        return Parse(i, p, Double.Parse);
      }

      //TODO: does decimal support e+32?
      public int TryParse(int i, IParserSupport<decimal> p)
      {
        string expr = p.Expression;

        if ((expr[i] < '0' || expr[i] > '9') &&
             expr[i] != p.DecimalDot)
        {
          return -1;
        }

        return Parse(i, p, Decimal.Parse);
      }

      static int Parse<T>(int i, IParserSupport<T> p, Parser<T> parser)
      {
        string str = ScanNumber(i, p);
        try
        {
          p.ParsedValue = parser(str,
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowExponent);

          return str.Length;
        }
        catch (FormatException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberFormat, str, e);
        }
        catch (OverflowException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, e);
        }
      }

      static string ScanNumber<T>(int i, IParserSupport<T> p)
      {
        string expr = p.Expression;

        // Fractal part: ==========================

        // numbers not like ".123":
        if (expr[i] != p.DecimalDot)
        {
          // skip digits and decimal point:
          for (; i < expr.Length; i++)
          {
            if (expr[i] >= '0' && expr[i]<= '9') continue;
            if (expr[i] == p.DecimalDot) i++;
            break;
          }
        }

        // skip digits:
        while (IsDigit(expr, i)) i++;

        // Exponental part: =======================

        // at least 2 chars:
        if (i+1 < expr.Length)
        {
          // E character:
          char c = expr[i];
          if (c == 'e' || c == 'E')
          {
            int j = i;

            // exponetal sign:
            c = expr[++j];
            if (c == '-' || c == '+') j++;

            // exponental value:
            if (IsDigit(expr, j++))
            {
              while (IsDigit(expr, j)) j++;
              i = j;
            }
          }
        }

        return expr.Substring(
          p.BeginPos, i - p.BeginPos);
      }
    }

    sealed class Int32LiteralParser : ILiteralParser<Int32>
    {
      public int TryParse(int i, IParserSupport<int> p)
      {
        string expr = p.Expression;

        if (expr[i] < '0' || expr[i] > '9')
        {
          return -1;
        }

        // hex/bin literal support:
        if (expr[i] == '0' && i+1 < expr.Length)
        {
          i++;
          char c = expr[i++];
          if (c == 'x' || c == 'X') return ScanInt32Hex(p, i, expr);
          if (c == 'b' || c == 'B') return ScanInt32Bin(p, i, expr);

          p.ParsedValue = 0;
          return 1;
        }

        // skip digits
        while (IsDigit(expr, i)) i++;

        // Try to parse: =============

        // extract number substring
        string str = expr.Substring(p.BeginPos, i - p.BeginPos);
        try
        {
          p.ParsedValue = Int32.Parse(
            str,
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowExponent);

          return str.Length;
        }
        catch (FormatException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberFormat, str, e);
        }
        catch (OverflowException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, e);
        }
      }

      // TODO: generalize
      static int ScanInt32Hex(
        IParserSupport<int> p, int i, string expr)
      {
        int begin = i, value = 0;
        for (; i < expr.Length; i++)
        {
          int hex = HexDigit(expr[i]);
          if (hex < 0) break;

          value *= 0x10;
          value += hex;
        }

        int len = i - begin;
        if (len == 0) { p.ParsedValue = 0; return 1; }
        if (len > 8)
        {
          string str = expr.Substring(p.BeginPos, i - p.BeginPos);
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, null);
        }

        p.ParsedValue = value;
        return len + 2;
      }

      static int ScanInt32Bin(
        IParserSupport<int> p, int i, string expr)
      {
        int begin = i, value = 0;
        for (; i < expr.Length; i++)
        {
          if (expr[i] == '0')
          {
            value <<= 1;
          }
          else if (expr[i] == '1')
          {
            value <<= 1;
            value |= 1;
          }
          else break;
        }

        int len = i - begin;
        if (len == 0) { p.ParsedValue = 0; return 1; }
        if (len > 32)
        {
          string str = expr.Substring(p.BeginPos, i - p.BeginPos);
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, null);
        }

        p.ParsedValue = value;
        return len + 2;
      }
    }

    sealed class Int64LiteralParser : ILiteralParser<Int64>
    {
      public int TryParse(int i, IParserSupport<long> p)
      {
        string expr = p.Expression;

        if (expr[i] < '0' || expr[i] > '9')
        {
          return -1;
        }

        // hex/bin literal support:
        if (expr[i] == '0' && i+1 < expr.Length)
        {
          i++;
          char c = expr[i++];
          if (c == 'x' || c == 'X') return ScanInt64Hex(p, i, expr);
          if (c == 'b' || c == 'B') return ScanInt64Bin(p, i, expr);

          p.ParsedValue = 0;
          return 1;
        }

        // skip digits
        while (IsDigit(expr, i)) i++;

        // Try to parse: =============

        // extract number substring
        string str = expr.Substring(p.BeginPos, i - p.BeginPos);
        try
        {
          p.ParsedValue = Int64.Parse(
            str,
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowExponent);

          return str.Length;
        }
        catch (FormatException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberFormat, str, e);
        }
        catch (OverflowException e)
        {
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, e);
        }
      }

      // TODO: generalize
      static int ScanInt64Hex(
        IParserSupport<long> p, int i, string expr)
      {
        int begin = i;
        long value = 0;
        for (; i < expr.Length; i++)
        {
          int hex = HexDigit(expr[i]);
          if (hex < 0) break;

          value *= 0x10;
          value += hex;
        }

        int len = i - begin;
        if (len == 0) { p.ParsedValue = 0; return 1; }
        if (len > 16)
        {
          string str = expr.Substring(p.BeginPos, i - p.BeginPos);
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, null);
        }

        p.ParsedValue = value;
        return len + 2;
      }

      static int ScanInt64Bin(
        IParserSupport<long> p, int i, string expr)
      {
        int begin = i;
        long value = 0;
        for (; i < expr.Length; i++)
        {
          if (expr[i] == '0')
          {
            value <<= 1;
          }
          else if (expr[i] == '1')
          {
            value <<= 1;
            value |= 1;
          }
          else break;
        }

        int len = i - begin;
        if (len == 0) { p.ParsedValue = 0; return 1; }
        if (len > 32)
        {
          string str = expr.Substring(p.BeginPos, i - p.BeginPos);
          throw p.InvalidNumberFormat(
            Resource.errNumberOverflow, str, null);
        }

        p.ParsedValue = value;
        return len + 2;
      }
    }

    sealed class UnknownLiteralParser<T> : ILiteralParser<T>
    {
      public int TryParse(int i, IParserSupport<T> p)
      {
        return -1;
      }
    }

    #endregion
    #region Common

    static int HexDigit(char c)
    {
      if (c >= '0')
      {
        if (c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c-'\x57';
        if (c >= 'A' && c <= 'F') return c-'\x37';
      }

      return -1;
    }

    static bool IsDigit(string s, int i)
    {
      return i < s.Length
        && s[i] >= '0'
        && s[i] <= '9';
    }

    #endregion
  }
}
