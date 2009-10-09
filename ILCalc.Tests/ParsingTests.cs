using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class ParsingTests
  {
    #region Initialize

    readonly CalcContext<int> calcInt;
    readonly CalcContext<double> calc;

    public ParsingTests()
    {
      this.calcInt = new CalcContext<int>();
      CalcI4.Constants.Add("pi", 3);
      CalcI4.Culture = CultureInfo.InvariantCulture;

      this.calc = new CalcContext<double>("x");
      Calc.Constants.ImportBuiltIn();
      Calc.Culture = CultureInfo.InvariantCulture;

      CalcI4.Functions.Add("xin", Func);
      CalcI4.Functions.Add("bin", Func);
      CalcI4.Functions.Add("max", Max);
      Calc.Functions.Add("max", Math.Max);
    }

    public int Func(int x) { return x; }

    public int Max(int a, int b) { return a; }

    CalcContext<double> Calc
    {
      get { return this.calc; }
    }

    CalcContext<int> CalcI4
    {
      get { return this.calcInt; }
    }

    #endregion
    #region SyntaxTests

    [TestMethod]
    public void Int32SyntaxTest()
    {
      SyntaxTest(CalcI4.Validate);
    }

    [TestMethod]
    public void DoubleSyntaxTest()
    {
      SyntaxTest(Calc.Validate);
    }

    private static void SyntaxTest(Action<string> a)
    {
      Action<string> TestGood = a;

      // numbers:
      TestErr(a, "(2+2)2+3", 4, 2);
      TestErr(a, "(2+2 2+3", 3, 3);
      TestErr(a, "(2+pi2+3", 3, 3);
      TestGood("123+(23+4)");
      TestGood("2+4-5");
      TestGood("max(1,2)");

      // operators:
      TestGood("(1+1)*2");
      TestGood("1+1*2");
      TestGood("pi+2");

      TestErr(a, "+12", 0, 1);
      TestErr(a, "2**3", 1, 2);
      TestErr(a, "max(1,*5)", 5, 2);

      TestGood("-2+Max(-1,-2)");
      TestGood("2*(-32)");
      TestGood("2*-3 + 2/-6 + 2^-3");
      TestGood("--2+ 3---4 + 5+-3");

      // separator:
      TestErr(a, ",", 0, 1);
      TestErr(a, "Max(2+,3)", 5, 2);
      TestErr(a, "Max(2,,3)", 5, 2);
      TestErr(a, "Max(,2)", 3, 2);
      TestGood("Max(1,3)");
      TestGood("Max(0,-1)");
      TestGood("Max(0,Max(1,3))");

      // brace open:
      TestGood("(2+2)(3+3)");
      TestGood("3(3+3)");
      TestGood("pi(3+3)");
      TestGood("pi+(3+3)+Max(12,(34))");

      // brace close:
      TestErr(a, "(2+)", 2, 2);
      TestErr(a, "3+()", 2, 2);
      TestErr(a, "Max(1,)", 5, 2);
      TestGood("(2+2)");
      TestGood("(2+pi)");
      TestGood("(2+(3))");

      // identifiers:
      TestErr(a, "pi pi", 0, 5);
      TestGood("(2+2)pi");
      TestGood("3pi");
      TestGood("Max(pi,pi)");
      TestGood("2+pi+3");

      // brace disbalance:
      TestErr(a, "(3+(2+3)+3))+3", 11, 1);
      TestErr(a, "((3+(2+3)+3)+3", 0, 1);
    }

    #endregion
    #region LiteralParseTest

    [TestMethod]
    public void Int32LiteralsParseTest()
    {
      // decimal literals:
      AssertEqual("123", 123);
      AssertEqual(int.MaxValue.ToString(), int.MaxValue);
      // TODO: fix it!
      // AssertEqual(int.MinValue.ToString(), int.MinValue);
      AssertEqual("-456", -456);
      AssertEqual("0", 0);
      AssertEqual("0+0", 0);

      // hex literals:
      AssertEqual("0xFF1E", 0xFF1E);
      AssertEqual("0xFFFFFFFF", -1);
      AssertEqual("0XFFFFFFF", 0xFFFFFFF);
      AssertFail("0x12345678AB+1", 0, 12);
      AssertEqual("0XABCDEFxin(1)", 0xABCDEF * 1);
      AssertEqual("0xin(1)", 0);
      AssertFail("0xixi", 1, 4);

      // binary literals:
      AssertEqual("0b1011", 0xB);
      AssertEqual("0b11111111", 0xFF);
      AssertEqual("0bin(1)", 0);
      AssertEqual("0b0001bin(2)", 1 * 2);
    }

    #endregion
    #region IdentifiersTest

    [TestMethod]
    public void IdentifiersTest()
    {
      TestErr("2+sinus(2+2)", 2, 5);
      TestErr("2+dsdsd", 2, 5);

      // simple match:
      TestErr("1+Sin+3", 2, 3);
      TestErr("1+sin(1;2;3)", 2, 3);
      TestErr("1+Params()", 2, 6);

      // ambiguous match:
      Calc.Constants.Add("x", 123);
      Calc.Functions.AddStatic("SIN", typeof(Math).GetMethod("Sin"));
      Calc.Functions.AddStatic("sin", typeof(Math).GetMethod("Sin"));

      TestErr("2+x+3", 2, 1);
      TestErr("1-sIN+32", 2, 3);
      TestErr("7+sin(1;2;3)", 2, 3);
      TestErr("0+Sin(3)+4", 2, 3);

      Calc.Arguments[0] = "sin";
      TestGood("1+sin*4");

      Calc.Constants.Add("sin", 1.23);
      TestErr("1+sin/4", 2, 3);

      Calc.Constants.Remove("sin");
      Calc.Constants.Remove("x");
      Calc.Functions.Remove("SIN");
      Calc.Functions.Remove("sin");

      Calc.Arguments[0] = "max";
      TestGood("2+max(3+3)");

      Calc.Constants.Add("max", double.MaxValue);
      TestErr("2+max(3+3)", 2, 3);

      Calc.Functions.AddStatic("maX",
        typeof(Math).GetMethod("Sin"));

      TestErr("2+max(3+3)", 2, 3);

      Calc.Constants.Remove("max");

      TestErr("1+max(1;2;3)+4", 2, 3);
      TestErr("2+max(1;2)/3", 2, 3);
      Calc.Functions.Remove("max", 2, false);
      TestErr("2+max(1;2)/3", 2, 3);

      // TODO: append MAX & max situations
    }

    #endregion
    #region LiteralsParseTests

    [TestMethod]
    public void IntegralLiteralsTest()
    {
      var c32 = new CalcContext<Int32>();
      var c64 = new CalcContext<Int64>();

      Action<int> check32 = x =>
      {
        Assert.AreEqual(c32.Evaluate(x.ToString()), x);
        Assert.AreEqual(c32.Evaluate("0x" + x.ToString("x")), x);
        Assert.AreEqual(c32.Evaluate(ToBin(x)), x);
      };

      Action<long> check64 = x =>
      {
        Assert.AreEqual(c64.Evaluate(x.ToString()), x);
        Assert.AreEqual(c64.Evaluate("0x" + x.ToString("x")), x);
        Assert.AreEqual(c64.Evaluate(ToBin(x)), x);
      };

      FromTo(int.MinValue, int.MinValue + 10, check32);
      FromTo(int.MaxValue - 10, int.MaxValue, check32);
      FromTo(-1000, 1000, check32);
      check32(int.MaxValue);

      FromTo(long.MinValue, long.MinValue + 10, check64);
      FromTo(long.MaxValue - 10, long.MaxValue, check64);
      FromTo(-1000, 1000, check64);
      check64(long.MaxValue);

      const long OvMax32 = int.MaxValue + 1L;
      const long OvMin32 = int.MinValue - 1L;
      const decimal OvMax64 = long.MaxValue + 1M;
      const decimal OvMin64 = long.MinValue - 1M;

      Throws<SyntaxException>(
        () => c32.Evaluate(OvMax32.ToString()),
        () => c32.Evaluate(OvMin32.ToString()),
        () => c32.Evaluate(string.Format("0x{0:X}", OvMax32 * 2)),
        () => c32.Evaluate(string.Format("0x{0:X}", OvMin32 * 2)),
        () => c32.Evaluate(ToBin(OvMax32)),
        () => c32.Evaluate(ToBin(OvMin32)),
        () => c64.Evaluate(OvMax64.ToString()),
        () => c64.Evaluate(OvMin64.ToString())
        );
    }

    [TestMethod]
    public void RealLiteralsTest()
    {
      var c8 = new CalcContext<Double>();
      var c4 = new CalcContext<Single>();

      Action<double> test8 = x =>
        Assert.AreEqual(c8.Evaluate(x.ToString("r")), x);
      Action<float> test4 = x =>
        Assert.AreEqual(c4.Evaluate(x.ToString("r")), x);

      test4(Single.MaxValue);
      test4(Single.MaxValue);
      test4(Single.Epsilon);

      test8(Double.MinValue);
      test8(Double.MaxValue);
      test8(Double.Epsilon);
    }

    #endregion
    #region CultureTests

    [TestMethod]
    public void CultureTests()
    {
      foreach(var culture in
#if FULL_FW
        CultureInfo.GetCultures(
          CultureTypes.SpecificCultures))
#else
        CultureHelper.GetCultures())
#endif
      {
        TestHelperReal<Double>(culture);
        TestHelperReal<Single>(culture);
        TestHelperReal<Decimal>(culture);

        TestHelperIntegral<Int32>(culture);
        TestHelperIntegral<Int64>(culture);
      }
    }

    static void TestHelperReal<T>(CultureInfo culture)
    {
      var calc = new CalcContext<T>();
      calc.Functions.Add("f", TwoArgsFunc);
      calc.Culture = culture;

      string expr = string.Format(
        "- 0{0}123 + -f(--45{0}67{1}f(78{0}9{1} 123))",
        culture.NumberFormat.NumberDecimalSeparator,
        culture.TextInfo.ListSeparator);

      calc.Validate(expr);
    }

    static void TestHelperIntegral<T>(CultureInfo culture)
    {
      var calc = new CalcContext<T>();
      calc.Functions.Add("f", TwoArgsFunc);
      calc.Culture = culture;

      string expr = string.Format(
        "-123 + -f(--4567{0}f(789{0} 123))",
        culture.TextInfo.ListSeparator);

      calc.Validate(expr);
    }

    public static T TwoArgsFunc<T>(T a, T b)
    {
      return default(T);
    }

    #endregion
    #region Helpers

    static void FromTo(
      int from, int to, Action<int> check)
    {
      for (int x = from; x < to; x++) check(x);
    }

    static void FromTo(
      long from, long to, Action<long> check)
    {
      for (long x = from; x < to; x++) check(x);
    }

    static string ToBin(int x)
    {
      if (x == 0) return "0b0";

      var buf = new StringBuilder("0b", 34);
      for (int i = 31; i >= 0; i--)
      {
        buf.Append((x >> i) & 1);
      }

      return buf.ToString();
    }

    static string ToBin(long x)
    {
      if (x == 0) return "0b0";

      var buf = new StringBuilder("0b", 66);
      for (int i = 63; i >= 0; i--)
      {
        buf.Append((x >> i) & 1);
      }

      return buf.ToString();
    }

    void TestGood(string expr)
    {
      Calc.Validate(expr);
    }

    void TestErr(string expr, int pos, int len)
    {
      try { Calc.Validate(expr); }
      catch (SyntaxException e)
      {
        Assert.AreEqual(e.Position, pos);
        Assert.AreEqual(e.Length, len);
      }
    }

    static void TestErr(
      Action<string> action,
      string expr, int pos, int len)
    {
      try { action(expr); }
      catch (SyntaxException e)
      {
        Assert.AreEqual(e.Position, pos);
        Assert.AreEqual(e.Length, len);
      }
    }

    private void AssertEqual(string expr, int value)
    {
      Debug.Assert(expr != null);
      Assert.AreEqual(CalcI4.Evaluate(expr), value);
    }

    private void AssertFail(string expr, int pos, int len)
    {
      Debug.Assert(expr != null);

      try { CalcI4.Evaluate(expr); }
      catch(SyntaxException e)
      {
        Assert.AreEqual(e.Position, pos);
        Assert.AreEqual(e.Length, len);
        return;
      }

      throw new AssertFailedException("Action doesn't throw!");
    }

    delegate void Action();

    static void Throws<TException>(params Action[] actions)
      where TException : Exception
    {
      if (actions == null)
        throw new ArgumentNullException("actions");

      foreach (var action in actions)
      {
        try { action(); }
        catch (TException e)
        {
          var msg = new StringBuilder()
            .Append('\'').Append(e.Message)
            .Append("'.").ToString();

          Trace.WriteLine(msg, typeof(TException).Name);
          continue;
        }
        catch
        {
          throw new InternalTestFailureException(
            typeof(TException).Name + " doesn't thrown!");
        }

        throw new InternalTestFailureException(
          typeof(TException).Name + " doesn't thrown!");
      }
    }

    #endregion
  }
}
