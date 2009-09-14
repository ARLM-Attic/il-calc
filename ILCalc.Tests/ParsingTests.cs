using System;
using System.Diagnostics;
using System.Globalization;
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
    #region LiteralParseTests

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
    #region IdentifiersTests

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
    #region Helpers

    void TestGood(string expr)
    {
      Calc.Validate(expr);
    }

    void TestErr(string expr, int pos, int len)
    {
      try
      {
        Calc.Validate(expr);
      }
      catch (SyntaxException e)
      {
        Assert.AreEqual(e.Position, pos);
        Assert.AreEqual(e.Length, len);
      }
    }

    static void TestErr(Action<string> action,
      string expr, int pos, int len)
    {
      try
      {
        action(expr);
      }
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
      try
      {
        CalcI4.Evaluate(expr);
      }
      catch(SyntaxException e)
      {
        Assert.AreEqual(e.Position, pos);
        Assert.AreEqual(e.Length, len);
        return;
      }

      throw new AssertFailedException("Action doesn't throw!");
    }

    #endregion
  }
}
