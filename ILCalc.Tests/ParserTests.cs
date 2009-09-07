#region 

#endregion

#region 

#endregion

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class ParserTests
  {
    #region Initialize

    readonly CalcContext<int> calc;

    public ParserTests()
    {
      this.calc = new CalcContext<int>();
      Calc.Functions.Add("xin", Func);
      Calc.Functions.Add("bin", Func);
    }

    public int Func(int x) { return x; }

    CalcContext<int> Calc
    {
      get { return this.calc; }
    }

    #endregion
    #region ParserTests

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


      //      Assert.AreEqual(Calc.Evaluate("0b1011"), 0xB);

      Assert.AreEqual(Calc.Evaluate("xin(2)"), 2);

    }

    #endregion
    #region Helpers

    delegate void Action();

    private void AssertEqual(string expr, int value)
    {
      Debug.Assert(expr != null);
      Assert.AreEqual(Calc.Evaluate(expr), value);
    }

    private void AssertFail(string expr, int pos, int len)
    {
      Debug.Assert(expr != null);
      try
      {
        Calc.Evaluate(expr);
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
