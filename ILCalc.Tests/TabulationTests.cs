using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed partial class TabulationTests
  {
    #region Initialize

    readonly TabTester<Double, DoubleRangeSupport> doubleTester;
    readonly TabTester<Int32, Int32RangeSupport> int32Tester;
    readonly TabTester<Int64, Int64RangeSupport> int64Tester;

    public TabulationTests()
    {
      this.doubleTester = new TabTester<double, DoubleRangeSupport>();
      var cR8 = this.doubleTester.Context;

      this.int32Tester = new TabTester<Int32, Int32RangeSupport>();
      var c32 = this.int32Tester.Context;

      this.int64Tester = new TabTester<Int64, Int64RangeSupport>();
      var c64 = this.int64Tester.Context;

      cR8.Functions.Add("Sin", Math.Sin);
      cR8.Functions.Add("Cos", Math.Cos);
      cR8.Functions.Add("Tan", Math.Tan);

      c32.Functions.Add("Func", Func1);
      c32.Functions.Add("Func", Func2);
      c64.Functions.Add("Func", Func1);
      c64.Functions.Add("Func", Func2);
    }

    #endregion
    #region TabTester

    sealed partial class TabTester<T, TRangeSupport>
      where TRangeSupport : IRangeSupport<T>, new()
    {
      #region Initialize

      readonly CalcContext<T> context;

      static readonly TRangeSupport Range = new TRangeSupport();

      public TabTester()
      {
        this.context = new CalcContext<T>();
      }

      public CalcContext<T> Context
      {
        get { return this.context; }
      }

      #endregion
      #region Tabulators

      static T[] Tabulate1D(
        EvalFunc1<T> func,
        ValueRange<T> range)
      {
        var array = new T[range.Count];
        T x = range.Begin;

        for (int i = 0; i < array.Length; i++)
        {
          array[i] = func(x);
          x = Range.AddStep(x, range.Step);
        }

        return array;
      }

      static T[][] Tabulate2D(
        EvalFunc2<T> func, ValueRange<T> r1, ValueRange<T> r2)
      {
        var array = new T[r1.Count][];
        T x = r1.Begin;
        for (int i = 0; i < array.Length; i++)
        {
          var row = new T[r2.Count];
          T y = r2.Begin;
          for (int j = 0; j < row.Length; j++)
          {
            row[j] = func(x, y);
            y = Range.AddStep(y, r2.Step);
          }

          array[i] = row;
          x = Range.AddStep(x, r1.Step);
        }

        return array;
      }

      static T[][][] Tabulate3D(
        EvalFunc3<T> func,
        ValueRange<T> r1,
        ValueRange<T> r2,
        ValueRange<T> r3)
      {
        var array3D = new T[r1.Count][][];
        T x = r1.Begin;
        for (int i = 0; i < array3D.Length; i++)
        {
          var array = new T[r2.Count][];
          T y = r2.Begin;
          for (int j = 0; j < array.Length; j++)
          {
            var row = new T[r3.Count];
            T z = r3.Begin;
            for (int k = 0; k < row.Length; k++)
            {
              row[k] = func(x, y, z);
              z = Range.AddStep(z, r3.Step);
            }

            array[j] = row;
            y = Range.AddStep(y, r2.Step);
          }

          array3D[i] = array;
          x = Range.AddStep(x, r1.Step);
        }

        return array3D;
      }

      static T[][][][] Tabulate4D(
        EvalFunc4<T> func,
        ValueRange<T> r1,
        ValueRange<T> r2,
        ValueRange<T> r3,
        ValueRange<T> r4)
      {
        var array4D = new T[r1.Count][][][];
        T x = r1.Begin;
        for (int i = 0; i < array4D.Length; i++)
        {
          var array3D = new T[r2.Count][][];
          T y = r2.Begin;
          for (int j = 0; j < array3D.Length; j++)
          {
            var array = new T[r3.Count][];
            T z = r3.Begin;
            for (int k = 0; k < array.Length; k++)
            {
              var row = new T[r4.Count];
              T w = r4.Begin;
              for (int g = 0; g < row.Length; g++)
              {
                row[g] = func(x, y, z, w);
                w = Range.AddStep(w, r4.Step);
              }

              array[k] = row;
              z = Range.AddStep(z, r3.Step);
            }

            array3D[j] = array;
            y = Range.AddStep(y, r2.Step);
          }

          x = Range.AddStep(x, r1.Step);
          array4D[i] = array3D;
        }

        return array4D;
      }

      #endregion
      #region AssertEquality

      static void AssertEquality2D(
        T[][] ex, T[][] a1, T[][] a2, T[][] a3)
      {
        Assert.AreEqual(ex.Length, a1.Length);
        Assert.AreEqual(ex.Length, a2.Length);
        Assert.AreEqual(ex.Length, a3.Length);

        for (int i = 0; i < ex.Length; i++)
        {
          Range.EqualityAssert(ex[i], a1[i], a2[i], a3[i]);
        }
      }

      static void AssertEquality3D(
        T[][][] ex, T[][][] a1, T[][][] a2, T[][][] a3)
      {
        Assert.AreEqual(ex.Length, a1.Length);
        Assert.AreEqual(ex.Length, a2.Length);
        Assert.AreEqual(ex.Length, a3.Length);

        for (int i = 0; i < ex.Length; i++)
        {
          AssertEquality2D(ex[i], a1[i], a2[i], a3[i]);
        }
      }

      #endregion
      #region Tests

      public void InterpretTest1D(string expr, EvalFunc1<T> func)
      {
        SetArgsCount(1);
        var range = Range.GetRandom();

        var tab = context.CreateInterpret(expr);
        var async = tab.BeginTabulate(range, null, null);

        var expected = Tabulate1D(func, range);

        var a1 = tab.Tabulate(range);
        var a2 = (T[]) tab.EndTabulate(async);
        var a3 = Interpret.Allocate(range);
        tab.TabulateToArray(a3, range);

        Range.EqualityAssert(expected, a1, a2, a3);
      }

      public void InterpretTest2D(string expr, EvalFunc2<T> func)
      {
        SetArgsCount(2);
        var rx = Range.GetRandom();
        var ry = Range.GetRandom();

        var tab = context.CreateInterpret(expr);
        var async = tab.BeginTabulate(rx, ry, null, null);

        var expected = Tabulate2D(func, rx, ry);

        var a1 = tab.Tabulate(rx, ry);
        var a2 = (T[][]) tab.EndTabulate(async);
        var a3 = Interpret.Allocate(rx, ry);
        tab.TabulateToArray(a3, rx, ry);

        AssertEquality2D(expected, a1, a2, a3);
      }

      public void InterpretTest3D(string expr, EvalFunc3<T> func)
      {
        SetArgsCount(3);
        ValueRange<T>
          rx = Range.GetRandom(),
          ry = Range.GetRandom(),
          rz = Range.GetRandom();

        var tab = context.CreateInterpret(expr);
        var async = tab.BeginTabulate(rx, ry, rz, null, null);

        var expected = Tabulate3D(func, rx, ry, rz);

        var a1 = (T[][][]) tab.Tabulate(rx, ry, rz);
        var a2 = (T[][][]) tab.EndTabulate(async);
        var a3 = (T[][][]) Interpret.Allocate(rx, ry, rz);
        tab.TabulateToArray(a3, rx, ry, rz);

        AssertEquality3D(expected, a1, a2, a3);
      }

      public void InterpretTest4D(string expr, EvalFunc4<T> func)
      {
        SetArgsCount(4);
        ValueRange<T>
          rX = Range.GetRandom(), rY = Range.GetRandom(),
          rZ = Range.GetRandom(), rW = Range.GetRandom();

        var tab = context.CreateInterpret(expr);

        IAsyncResult async = tab.BeginTabulate(
          new[] { rX, rY, rZ, rW }, null, null);

        var expected = Tabulate4D(func, rX, rY, rZ, rW);

        var a1 = (T[][][][]) tab.Tabulate(rX, rY, rZ, rW);
        var a2 = (T[][][][]) tab.EndTabulate(async);
        var a3 = (T[][][][]) Interpret.Allocate(rX, rY, rZ, rW);

        tab.TabulateToArray(a3, rX, rY, rZ, rW);

        Assert.AreEqual(expected.Length, a1.Length);
        Assert.AreEqual(expected.Length, a2.Length);
        Assert.AreEqual(expected.Length, a3.Length);

        for (int i = 0; i < expected.Length; i++)
        {
          AssertEquality3D(expected[i], a1[i], a2[i], a3[i]);
        }
      }

      void SetArgsCount(int count)
      {
        Debug.Assert(count > 0);
        Debug.Assert(count <= 4);

        string[] names = { "x", "y", "z", "w" };
        context.Arguments.Clear();
        for(int i = 0; i < count; i++)
        {
          context.Arguments.Add(names[i]);
        }
      }

      #endregion
    }

    #endregion
    #region IRangeSupport

    interface IRangeSupport<T>
    {
      T AddStep(T value, T step);
      ValueRange<T> GetRandom();
      void EqualityAssert(T[] expected, T[] a1, T[] a2, T[] a3);
    }

    struct DoubleRangeSupport : IRangeSupport<Double>
    {
      static readonly Random Random = new Random();

      public double AddStep(double value, double step)
      {
        return value + step;
      }

      public ValueRange<double> GetRandom()
      {
        int from = Random.Next(-100, 100);
        int to = Random.Next(10, 20) + from;
        int step = Random.Next(1, 4);

        return new ValueRange<double>(from, to, step);
      }

      public void EqualityAssert(
        double[] expected, double[] a1, double[] a2, double[] a3)
      {
        const double Delta = 1e-12;

        for (int i = 0; i < expected.Length; i++)
        {
          Assert.AreEqual(expected[i], a1[i], Delta);
          Assert.AreEqual(expected[i], a2[i], Delta);
          Assert.AreEqual(expected[i], a3[i], Delta);
        }
      }
    }

    struct Int32RangeSupport : IRangeSupport<Int32>
    {
      static readonly Random Random = new Random();

      public int AddStep(int value, int step)
      {
        return value + step;
      }

      public ValueRange<int> GetRandom()
      {
        int from = Random.Next(-100, 100);
        int to   = Random.Next(10, 20) + from;
        int step = Random.Next(1, 4);

        return new ValueRange<int>(from, to, step);
      }

      public void EqualityAssert(
        int[] expected, int[] a1, int[] a2, int[] a3)
      {
        for (int i = 0; i < expected.Length; i++)
        {
          Assert.AreEqual(expected[i], a1[i]);
          Assert.AreEqual(expected[i], a2[i]);
          Assert.AreEqual(expected[i], a3[i]);
        }
      }
    }

    struct Int64RangeSupport : IRangeSupport<Int64>
    {
      static readonly Random Random = new Random();

      public long AddStep(long value, long step)
      {
        return value + step;
      }

      public ValueRange<long> GetRandom()
      {
        long from = Random.Next(-100, 100);
        long to   = Random.Next(10, 20) + from;
        long step = Random.Next(1, 4);

        return ValueRange.Create(from, to, step);
      }

      public void EqualityAssert(
        long[] expected, long[] a1, long[] a2, long[] a3)
      {
        for (long i = 0; i < expected.Length; i++)
        {
          Assert.AreEqual(expected[i], a1[i]);
          Assert.AreEqual(expected[i], a2[i]);
          Assert.AreEqual(expected[i], a3[i]);
        }
      }
    }

    #endregion
    #region Helpers

    delegate T EvalFunc3<T>(T arg1, T arg2, T arg3);
    delegate T EvalFunc4<T>(T arg1, T arg2, T arg3, T arg4);

    public static int Func1(int x) { return -x; }

    public static int Func2(int x, int y) { return x + -y; }

    public static long Func1(long x) { return -x; }

    public static long Func2(long x, long y) { return x + -y; }

    #endregion
    #region DoubleTests

    [TestMethod]
    public void DoubleInterpret1DTest()
    {
      doubleTester.InterpretTest1D(
        "2sin(x)",
        x => 2 * Math.Sin(x));
    }

    [TestMethod]
    public void DoubleInterpret2DTest()
    {
      doubleTester.InterpretTest2D(
        "cos(x) * sin(y)",
        (x, y) => Math.Cos(x) * Math.Sin(y));
    }

    [TestMethod]
    public void DoubleInterpret3DTest()
    {
      doubleTester.InterpretTest3D(
        "cos(x) * sin(y) * tan(z)",
        (x, y, z) => Math.Cos(x) * Math.Sin(y) * Math.Tan(z));
    }

    [TestMethod]
    public void DoubleInterpret4DTest()
    {
      doubleTester.InterpretTest4D(
        "cos(x) * sin(y) * tan(z) * sin(w)",
        (x, y, z, w) =>
          Math.Cos(x) * Math.Sin(y) *
          Math.Tan(z) * Math.Sin(w));
    }

    #endregion
    #region Int32Tests

    [TestMethod]
    public void Int32Interpret1DTest()
    {
      int32Tester.InterpretTest1D(
        "2 + Func(x/2*x)",
        x => 2 + Func1(x/2*x));
    }

    [TestMethod]
    public void Int32Interpret2DTest()
    {
      int32Tester.InterpretTest2D(
        "2 + Func(x, y)",
        (x, y) => 2 + Func2(x, y));
    }

    [TestMethod]
    public void Int32Interpret3DTest()
    {
      int32Tester.InterpretTest3D(
        "Func(z + Func(x, y))",
        (x, y, z) => Func1(z + Func2(x, y)));
    }

    [TestMethod]
    public void Int32Interpret4DTest()
    {
      int32Tester.InterpretTest4D(
        "Func(w, z + Func(x, y))",
        (x, y, z, w) => Func2(w, z + Func2(x, y)));
    }

    #endregion
    #region Int64Tests

    [TestMethod]
    public void Int64Interpret1DTest()
    {
      int64Tester.InterpretTest1D(
        "2 + Func(x/2*x)",
        x => 2 + Func1(x/2*x));
    }

    [TestMethod]
    public void Int64Interpret2DTest()
    {
      int64Tester.InterpretTest2D(
        "2 + Func(x, y)",
        (x, y) => 2 + Func2(x, y));
    }

    [TestMethod]
    public void Int64Interpret3DTest()
    {
      int64Tester.InterpretTest3D(
        "Func(z + Func(x, y))",
        (x, y, z) => Func1(z + Func2(x, y)));
    }

    [TestMethod]
    public void Int64Interpret4DTest()
    {
      int64Tester.InterpretTest4D(
        "Func(w, z + Func(x, y))",
        (x, y, z, w) => Func2(w, z + Func2(x, y)));
    }

    #endregion
  }
}