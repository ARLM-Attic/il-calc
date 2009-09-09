using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ILCalc.Tests
{
  public class ExprGenerator
  {
    #region Support

    protected delegate void
      LiteralGen(StringBuilder buf, IFormatProvider format);

    static readonly SupportCollection<LiteralGen> Support;

    static ExprGenerator()
    {
      Support = new SupportCollection<LiteralGen>();

      Support.Add<Double>(DoubleGenerator);
      Support.Add<Int32>(Int32Generator);
    }

    protected static LiteralGen Resolve<T>()
    {
      var gen = Support.Find<T>();
      if (gen == null)
        throw new NotSupportedException();

      return gen;
    }

    #endregion
    #region Generators

    public static void DoubleGenerator(
      StringBuilder buf, IFormatProvider format)
    {
      // pow(int, int)
      int del = 1;
      for (int i = FromTo(6, 9); i > 0; i--)
      {
        del *= 10;
      }

      int frac = FromTo(int.MinValue / del, int.MaxValue / del);

      if (OneOf(4))
      {
        buf.AppendFormat(
          format,
          OneOf(3) ? "{0:E3}" : "{0:G}",
          frac * Math.Pow(10, FromTo(-250, 250)));
      }
      else
      {
        if (OneOf(3))
          buf.AppendFormat(format, "{0:F}", frac);
        else
          buf.Append(frac);
      }
    }

    public static void Int32Generator(
      StringBuilder buf, IFormatProvider format)
    {
      int x = Random.Next(int.MinValue, int.MaxValue);
      if (x == 0)
        x = 1; // protect from division by zero!

      // TODO: binary + octal!
      if (OneOf(4))
      {
#if CF2
        buf.AppendFormat(CultureInfo.CurrentCulture, "0x{0:X}", x);
#else
        buf.AppendFormat("0x{0:X}", x);
#endif
      }
      else
      {
        buf.Append(x);
      }
    }

    #endregion
    #region Helpers

    protected static readonly
      Random Random = new Random();

    protected static bool OneOf(int number)
    {
      return Random.Next() % number == 0;
    }

    protected static int FromTo(int min, int max)
    {
      return Random.Next(min, max);
    }

    #endregion
  }

  public sealed class ExprGenerator<T> : ExprGenerator
  {
    #region Fields

    static readonly LiteralGen PutLitaral = Resolve<T>();
    readonly CalcContext<T> context;
    readonly CultureInfo culture;
    readonly NumberFormatInfo format;
    readonly List<string> idens;
    readonly List<KeyValuePair<string, FunctionItem<T>>> funcs;

    readonly char separator;

    #endregion
    #region Constructor

    public ExprGenerator(CalcContext<T> calc)
    {
      this.context = calc;
      this.culture = calc.Culture ?? CultureInfo.InvariantCulture;
      this.format = this.culture.NumberFormat;
      this.separator = this.culture.TextInfo.ListSeparator[0];

      this.idens = new List<string>();

      if (calc.Arguments != null)
        this.idens.AddRange(calc.Arguments);

      if (calc.Constants != null)
        this.idens.AddRange(calc.Constants.Keys);

      this.funcs = new List<KeyValuePair<string, FunctionItem<T>>>();

      foreach (var item in calc.Functions)
      {
        string name = item.Key;
        foreach (var func in item.Value)
        {
          this.funcs.Add(
            new KeyValuePair<string, FunctionItem<T>>(name, func));
        }
      }
    }

    #endregion
    #region Generators

    public string Next()
    {
      var buf = new StringBuilder(16);
      PutExpression(buf, FromTo(3, 5));
      return buf.ToString();
    }

    public Enumerator Generate(int count)
    {
      return new Enumerator(this, count, FromTo(3, 5));
    }

    static void PutSpace(StringBuilder buf)
    {
      // 0-3 space characters
      for (int count = FromTo(0, 3); count > 0; count--)
      {
        buf.Append(' ');
      }
    }

    static void PutOperator(StringBuilder buf)
    {
      buf.Append(
        OneOf(7) ? '%' : "+-*/^"[FromTo(0, 4)]);
    }

    void PutIdentifier(StringBuilder buf)
    {
      string name = this.idens[FromTo(0, this.idens.Count)];

      buf.Append(
        this.context.IgnoreCase ?
        name : RandomCase(name, this.culture));
    }

    void PutValueItem(StringBuilder buf)
    {
      if (OneOf(3))
      {
        if (this.idens.Count > 0)
             PutIdentifier(buf);
        else PutLitaral(buf, this.format);
      }
      else PutLitaral(buf, this.format);
    }

    void PutFunction(StringBuilder buf, int depth)
    {
      var pair = this.funcs[FromTo(0, this.funcs.Count)];
      FunctionItem<T> func = pair.Value;

      buf.Append(pair.Key);
      PutSpace(buf);
      buf.Append('(');

      int count = func.ArgsCount;
      if (func.HasParamArray && !OneOf(4))
      {
        count += FromTo(0, 10);
      }

      for (int i = 0; i < count; i++)
      {
        PutExpression(buf, depth);

        if (i+1 != count) buf.Append(this.separator);
      }

      buf.Append(')');
    }

    void PutExpression(StringBuilder buf, int depth)
    {
      PutValueItem(buf);
      for (int i = 1; i < depth; i++)
      {
        if (OneOf(depth))
        {
          PutSpace(buf);
          PutOperator(buf);

          if (OneOf(3)) PutBraceExpr(buf, depth - 1);
          else
          {
            if (OneOf(4)) PutLitaral(buf, this.format);

            PutFunction(buf, depth - 1);
          }
        }
        else
        {
          PutSpace(buf);
          PutOperator(buf);
          PutSpace(buf);
          PutValueItem(buf);
        }
      }
    }

    void PutBraceExpr(StringBuilder buf, int depth)
    {
      PutSpace(buf);

      if (OneOf(3) && this.idens.Count != 0)
      {
        PutValueItem(buf);
        PutSpace(buf);
      }

      buf.Append('(');
      PutSpace(buf);
      PutExpression(buf, depth);
      PutSpace(buf);
      buf.Append(')');
    }

    #endregion
    #region Helpers

    static string RandomCase(string text, CultureInfo culture)
    {
      if (OneOf(3))
      {
        switch (Random.Next() % 3)
        {
          case 0: return text.ToLower();
          case 1: return text.ToUpper();
#if !SILVERLIGHT
          case 2: return culture.TextInfo.ToTitleCase(text);
#endif
          default: return text;
        }
      }

      var buf = new StringBuilder(text.Length);
      bool flag = OneOf(2);

      for (int i = 0, len = text.Length; i < len; flag = !flag)
      {
        int ln = FromTo(1, len - i);

        string part = text.Substring(i, ln);
        buf.Append(flag ? part.ToUpper() : part.ToLower());
        i += ln;
      }

      return buf.ToString();
    }

    #endregion
    #region Enumerator

    public struct Enumerator
      : IEnumerable<string>, IEnumerator<string>
    {
      readonly ExprGenerator<T> gen;
      readonly int count;
      readonly int depth;

      string expr;
      int i;

      internal Enumerator(
        ExprGenerator<T> gen, int count, int depth)
      {
        this.i = 0;
        this.gen = gen;
        this.count = count;
        this.depth = depth;
        this.expr = string.Empty;
      }

      public string Current      { get { return this.expr; } }
      object IEnumerator.Current { get { return this.expr; } }

      public void Dispose() { }

      public void Reset() { this.i = 0; }

      public bool MoveNext()
      {
        if (this.i < this.count)
        {
          this.i++;
          var buf = new StringBuilder(8);
          this.gen.PutExpression(buf, this.depth);
          this.expr = buf.ToString();
          return true;
        }

        return false;
      }

      public IEnumerator<string> GetEnumerator() { return this; }

      IEnumerator IEnumerable.GetEnumerator() { return this; }
    }

    #endregion
  }
}
