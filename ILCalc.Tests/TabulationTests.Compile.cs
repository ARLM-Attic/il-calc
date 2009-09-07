using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  public sealed partial class TabulationTests
  {
    #region TabTester

    sealed partial class TabTester<T, TRangeSupport>
    {
      #region Tests

      public void TabulatorTest1D(string expr, EvalFunc1<T> func)
      {
        SetArgsCount(1);
        var range = Range.GetRandom();

        var tab = context.CreateTabulator(expr);
        var async = tab.BeginTabulate(range, null, null);

        var expected = Tabulate1D(func, range);

        var a1 = tab.Tabulate(range);
        var a2 = (T[]) tab.EndTabulate(async);
        var a3 = Tabulator.Allocate(range);
        tab.TabulateToArray(a3, range);

        Range.EqualityAssert(expected, a1, a2, a3);
      }

      public void TabulatorTest2D(string expr, EvalFunc2<T> func)
      {
        SetArgsCount(2);
        var rx = Range.GetRandom();
        var ry = Range.GetRandom();

        var tab = context.CreateTabulator(expr);
        var async = tab.BeginTabulate(rx, ry, null, null);

        var expected = Tabulate2D(func, rx, ry);

        var a1 = tab.Tabulate(rx, ry);
        var a2 = (T[][]) tab.EndTabulate(async);
        var a3 = Tabulator.Allocate(rx, ry);
        tab.TabulateToArray(a3, rx, ry);

        AssertEquality2D(expected, a1, a2, a3);
      }

      public void TabulatorTest3D(string expr, EvalFunc3<T> func)
      {
        SetArgsCount(3);
        ValueRange<T>
          rx = Range.GetRandom(),
          ry = Range.GetRandom(),
          rz = Range.GetRandom();

        var tab = context.CreateTabulator(expr);
        var async = tab.BeginTabulate(rx, ry, rz, null, null);

        var expected = Tabulate3D(func, rx, ry, rz);

        var a1 = (T[][][]) tab.Tabulate(rx, ry, rz);
        var a2 = (T[][][]) tab.EndTabulate(async);
        var a3 = (T[][][]) Tabulator.Allocate(rx, ry, rz);
        tab.TabulateToArray(a3, rx, ry, rz);

        AssertEquality3D(expected, a1, a2, a3);
      }

      public void TabulatorTest4D(string expr, EvalFunc4<T> func)
      {
        SetArgsCount(4);
        ValueRange<T>
          rX = Range.GetRandom(), rY = Range.GetRandom(),
          rZ = Range.GetRandom(), rW = Range.GetRandom();

        var tab = context.CreateTabulator(expr);

        IAsyncResult async = tab.BeginTabulate(
          new[] { rX, rY, rZ, rW }, null, null);

        var expected = Tabulate4D(func, rX, rY, rZ, rW);

        var a1 = (T[][][][]) tab.Tabulate(rX, rY, rZ, rW);
        var a2 = (T[][][][]) tab.EndTabulate(async);
        var a3 = (T[][][][]) Tabulator.Allocate(rX, rY, rZ, rW);

        tab.TabulateToArray(a3, rX, rY, rZ, rW);

        Assert.AreEqual(expected.Length, a1.Length);
        Assert.AreEqual(expected.Length, a2.Length);
        Assert.AreEqual(expected.Length, a3.Length);

        for (int i = 0; i < expected.Length; i++)
        {
          AssertEquality3D(expected[i], a1[i], a2[i], a3[i]);
        }
      }

      #endregion
    }

    #endregion
    #region DoubleTests

    [TestMethod]
    public void DoubleTabulator1DTest()
    {
      doubleTester.TabulatorTest1D(
        "2sin(x)",
        x => 2 * Math.Sin(x));
    }

    [TestMethod]
    public void DoubleTabulator2DTest()
    {
      doubleTester.TabulatorTest2D(
        "cos(x) * sin(y)",
        (x, y) => Math.Cos(x) * Math.Sin(y));
    }

    [TestMethod]
    public void DoubleTabulator3DTest()
    {
      doubleTester.TabulatorTest3D(
        "cos(x) * sin(y) * tan(z)",
        (x, y, z) => Math.Cos(x) * Math.Sin(y) * Math.Tan(z));
    }

    [TestMethod]
    public void DoubleTabulator4DTest()
    {
      doubleTester.TabulatorTest4D(
        "cos(x) * sin(y) * tan(z) * sin(w)",
        (x, y, z, w) =>
          Math.Cos(x) * Math.Sin(y) *
          Math.Tan(z) * Math.Sin(w));
    }

    #endregion
    #region Int32Tests

    [TestMethod]
    public void Int32Tabulator1DTest()
    {
      int32Tester.TabulatorTest1D(
        "2 + Func(x)",
        x => 2 + Func(x));
    }

    [TestMethod]
    public void Int32Tabulator2DTest()
    {
      int32Tester.TabulatorTest2D(
        "2 + Func(x, y)",
        (x, y) => 2 + Func(x, y));
    }

    [TestMethod]
    public void Int32Tabulator3DTest()
    {
      int32Tester.TabulatorTest3D(
        "Func(z + Func(x, y))",
        (x, y, z) => Func(z + Func(x, y)));
    }

    [TestMethod]
    public void Int32Tabulator4DTest()
    {
      int32Tester.TabulatorTest4D(
        "Func(w, z + Func(x, y))",
        (x, y, z, w) => Func(w, z + Func(x, y)));
    }

    #endregion
  }
}
