using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class EvaluationTests
  {
    #region Initialize

    static readonly Random Rnd = new Random();

    #endregion
    #region EvaluationTests

    [TestMethod]
    public void Int32EvaluationTest()
    {
      var t = new EvalTester<Int32, Int32TestSupport>();
      t.EvaluationTest();
    }

    [TestMethod]
    public void Int32OptimizerTest()
    {
      var t = new EvalTester<Int32, Int32TestSupport>();
      t.OptimizerTest();
    }

    [TestMethod]
    public void Int64EvaluationTest()
    {
      var t = new EvalTester<Int64, Int64TestSupport>();
      t.EvaluationTest();
    }

    [TestMethod]
    public void Int64OptimizerTest()
    {
      var t = new EvalTester<Int64, Int64TestSupport>();
      t.OptimizerTest();
    }

    [TestMethod]
    public void DoubleEvaluationTest()
    {
      var t = new EvalTester<Double, DoubleTestSupport>();
      t.EvaluationTest();
    }

    [TestMethod]
    public void DoubleOptimizerTest()
    {
      var t = new EvalTester<Double, DoubleTestSupport>();
      t.OptimizerTest();
    }

    [TestMethod]
    public void SingleEvaluationTest()
    {
      var t = new EvalTester<Single, SingleTestSupport>();
      t.EvaluationTest();
    }

    [TestMethod]
    public void SingleOptimizerTest()
    {
      var t = new EvalTester<Single, SingleTestSupport>();
      t.OptimizerTest();
    }

    [TestMethod]
    public void DecimalEvaluationTest()
    {
      var t = new EvalTester<Decimal, DecimalTestSupport>();
      t.EvaluationTest();
    }

    [TestMethod]
    public void DecimalOptimizerTest()
    {
      var t = new EvalTester<Decimal, DecimalTestSupport>();
      t.OptimizerTest();
    }

    #endregion
    #region ITestSupport

    interface ITestSupport<T>
    {
      CalcContext<T> Context { get; }
      T Value { get; }

      void EqualityAssert(T a, T b);
    }

    public sealed class Int32TestSupport : ITestSupport<int>
    {
      #region Initialize

      readonly CalcContext<int> context;
      readonly int x;

      public Int32TestSupport()
      {
        this.context = new CalcContext<int>("x");
        this.x = Rnd.Next(int.MinValue, int.MaxValue);

        Context.Culture = CultureInfo.CurrentCulture;

        Context.Constants.Add("max", int.MaxValue);
        Context.Constants.Add("min", int.MinValue);

        Context.Functions.Import("Params",  typeof(Int32TestSupport));
        Context.Functions.Import("Params2", typeof(Int32TestSupport));
        Context.Functions.Import("Params3", typeof(Int32TestSupport));

        Context.Functions.Add("Inst0", Inst0);
        Context.Functions.Add("Inst1", Inst1);
        Context.Functions.Add("Inst2", Inst2);
        Context.Functions.Add("InstP", InstP);
      }

      #endregion
      #region Imports

      // ReSharper disable UnusedMember.Global

      public static int Params(int arg, params int[] args)
      {
        return arg + (args.Length > 0 ? args[0] : 0);
      }

      public static int Params2(params int[] args)
      {
        int avg = 0;
        foreach (int c in args) avg += c;

        if (args.Length == 0) return 1;
        int y= avg / args.Length;
        return y == 0? 1 : y;
      }

      public static int Params3(int a, int b, params int[] args)
      {
        int y = a + b;
        return y == 0 ? 1 : y;
      }

      public int Inst0()
      {
        return this.x;
      }

      public int Inst1(int arg)
      {
        int y = this.x + arg;
        return y == 0 ? 1 : y;
      }

      public int Inst2(int arg1, int arg2)
      {
        if (arg2 == 0) return 1;
        int y = this.x + arg1 / arg2;
        return y == 0 ? 1 : y;
      }

      public int InstP(params int[] args)
      {
        if (args == null)
          throw new ArgumentNullException("args");

        int res = this.x;
        foreach (int d in args) res += d;

        return res == 0? 1 : res;
      }

      // ReSharper restore UnusedMember.Global

      #endregion
      #region ITestSupport

      public CalcContext<int> Context
      {
        get { return this.context; }
      }

      public int Value
      {
        get { return this.x; }
      }

      public void EqualityAssert(int a, int b)
      {
        Assert.AreEqual(a, b);
      }

      #endregion
    }

    public sealed class Int64TestSupport : ITestSupport<long>
    {
      #region Initialize

      readonly CalcContext<long> context;
      readonly long x;

      public Int64TestSupport()
      {
        this.context = new CalcContext<long>("x");
        this.x = Rnd.Next(int.MinValue, int.MaxValue);

        Context.Culture = CultureInfo.CurrentCulture;

        Context.Constants.Add("max", long.MaxValue);
        Context.Constants.Add("min", long.MinValue);

        Context.Functions.Import("Params",  typeof(Int64TestSupport));
        Context.Functions.Import("Params2", typeof(Int64TestSupport));
        Context.Functions.Import("Params3", typeof(Int64TestSupport));

        Context.Functions.Add("Inst0", Inst0);
        Context.Functions.Add("Inst1", Inst1);
        Context.Functions.Add("Inst2", Inst2);
        Context.Functions.Add("InstP", InstP);
      }

      #endregion
      #region Imports

      // ReSharper disable UnusedMember.Global

      public static long Params(long arg, params long[] args)
      {
        return arg + (args.Length > 0 ? args[0] : 0);
      }

      public static long Params2(params long[] args)
      {
        long avg = 0;
        foreach (long c in args) avg += c;

        if (args.Length == 0) return 1;
        long y = avg / args.Length;
        return y == 0? 1 : y;
      }

      public static long Params3(long a, long b, params long[] args)
      {
        long y = a + b;
        return y == 0 ? 1 : y;
      }

      public long Inst0()
      {
        return this.x;
      }

      public long Inst1(long arg)
      {
        long y = this.x + arg;
        return y == 0 ? 1 : y;
      }

      public long Inst2(long arg1, long arg2)
      {
        if (arg2 == 0) return 1;
        long y = this.x + arg1 / arg2;
        return y == 0 ? 1 : y;
      }

      public long InstP(params long[] args)
      {
        if (args == null)
          throw new ArgumentNullException("args");

        long res = this.x;
        foreach (long d in args) res += d;

        return res == 0? 1 : res;
      }

      // ReSharper restore UnusedMember.Global

      #endregion
      #region ITestSupport

      public CalcContext<long> Context
      {
        get { return this.context; }
      }

      public long Value
      {
        get { return this.x; }
      }

      public void EqualityAssert(long a, long b)
      {
        Assert.AreEqual(a, b);
      }

      #endregion
    }

    public sealed class DoubleTestSupport : ITestSupport<double>
    {
      #region Initialize

      readonly CalcContext<double> context;
      readonly double x;

      public DoubleTestSupport()
      {
        this.context = new CalcContext<double>("x");
        this.x = Rnd.NextDouble();

        Context.Culture = CultureInfo.CurrentCulture;

        Context.Constants.Add("pi", Math.PI);
        Context.Constants.Add("e", Math.E);
        Context.Constants.Add("fi", 1.234);

        Context.Functions.ImportBuiltIn();
        Context.Functions.Import("Params", typeof(DoubleTestSupport));
        Context.Functions.Import("Params2", typeof(DoubleTestSupport));
        Context.Functions.Import("Params3", typeof(DoubleTestSupport));

        Context.Functions.Add("Inst0", Inst0);
        Context.Functions.Add("Inst1", Inst1);
        Context.Functions.Add("Inst2", Inst2);
        Context.Functions.Add("InstP", InstP);
      }

      #endregion
      #region Imports

      // ReSharper disable UnusedMember.Global

      public static double Params(double arg, params double[] args)
      {
        return arg + (args.Length > 0 ? args[0] : 0.0);
      }

      public static double Params2(params double[] args)
      {
        if (args.Length == 0) return 1;

        double avg = 0;
        foreach (double c in args) avg += c;
        return avg / args.Length;
      }

      public static double Params3(
        double a, double b, params double[] args)
      {
        return a + b;
      }

      public double Inst0()
      {
        return this.x;
      }

      public double Inst1(double arg)
      {
        return this.x + arg;
      }

      public double Inst2(double arg1, double arg2)
      {
        return this.x + arg1 / arg2;
      }

      public double InstP(params double[] args)
      {
        if (args == null)
          throw new ArgumentNullException("args");

        double res = this.x;
        foreach (double d in args) res += d;

        return res;
      }

      // ReSharper restore UnusedMember.Global

      #endregion
      #region ITestSupport

      public CalcContext<double> Context
      {
        get { return this.context; }
      }

      public double Value
      {
        get { return this.x; }
      }

      public void EqualityAssert(double a, double b)
      {
        if (double.IsInfinity(a) || double.IsNaN(a) ||
            double.IsInfinity(b) || double.IsNaN(b)) return;

        const double Eps = 1e-6;

        var delta = Math.Max(Math.Abs(a), Math.Abs(b)) * Eps;
        Assert.AreEqual(a, b, delta);
      }

      #endregion
    }

    public sealed class SingleTestSupport : ITestSupport<float>
    {
      #region Initialize

      readonly CalcContext<float> context;
      readonly float x;

      public SingleTestSupport()
      {
        this.context = new CalcContext<float>("x");
        this.x = (float) Rnd.NextDouble();

        Context.Culture = CultureInfo.CurrentCulture;

        Context.Constants.Add("pi", (float) Math.PI);
        Context.Constants.Add("e", (float) Math.E);
        Context.Constants.Add("fi", 1.234f);

        Context.Functions.ImportBuiltIn();
        Context.Functions.Import("Params", typeof(SingleTestSupport));
        Context.Functions.Import("Params2", typeof(SingleTestSupport));
        Context.Functions.Import("Params3", typeof(SingleTestSupport));

        Context.Functions.Add("Inst0", Inst0);
        Context.Functions.Add("Inst1", Inst1);
        Context.Functions.Add("Inst2", Inst2);
        Context.Functions.Add("InstP", InstP);
      }

      #endregion
      #region Imports

      // ReSharper disable UnusedMember.Global

      public static float Params(float arg, params float[] args)
      {
        return (float) (arg + (args.Length > 0 ? args[0] : 0.0));
      }

      public static float Params2(params float[] args)
      {
        if (args.Length == 0) return 0;

        float avg = 0;
        foreach (float c in args) avg += c;
        return avg / args.Length;
      }

      public static float Params3(
        float a, float b, params float[] args)
      {
        return a + b;
      }

      public float Inst0()
      {
        return this.x;
      }

      public float Inst1(float arg)
      {
        return this.x + arg;
      }

      public float Inst2(float arg1, float arg2)
      {
        return this.x + arg1 / arg2;
      }

      public float InstP(params float[] args)
      {
        if (args == null)
          throw new ArgumentNullException("args");

        float res = this.x;
        foreach (float d in args) res += d;

        return res;
      }

      // ReSharper restore UnusedMember.Global

      #endregion
      #region ITestSupport

      public CalcContext<float> Context
      {
        get { return this.context; }
      }

      public float Value
      {
        get { return this.x; }
      }

      public void EqualityAssert(float a, float b)
      {
        if (float.IsInfinity(a) || float.IsNaN(a) ||
            float.IsInfinity(b) || float.IsNaN(b)) return;

        const float Eps = 1e+1f;

        var delta = Math.Max(Math.Abs(a), Math.Abs(b)) * Eps;
        Assert.AreEqual(a, b, delta);
      }

      #endregion
    }

    public sealed class DecimalTestSupport : ITestSupport<decimal>
    {
      #region Initialize

      readonly CalcContext<decimal> context;
      readonly decimal x;

      public DecimalTestSupport()
      {
        this.context = new CalcContext<decimal>("x");
        this.x = (decimal) Rnd.NextDouble();

        Context.Culture = CultureInfo.CurrentCulture;

        Context.Constants.Add("pi", (decimal) Math.PI);
        Context.Constants.Add("e", (decimal) Math.E);
        Context.Constants.Add("fi", 1.234m);

        Context.Functions.ImportBuiltIn();
        Context.Functions.Import("Params", typeof(DecimalTestSupport));
        Context.Functions.Import("Params2", typeof(DecimalTestSupport));
        Context.Functions.Import("Params3", typeof(DecimalTestSupport));

        Context.Functions.Add("Inst0", Inst0);
        Context.Functions.Add("Inst1", Inst1);
        Context.Functions.Add("Inst2", Inst2);
        Context.Functions.Add("InstP", InstP);
      }

      #endregion
      #region Imports

      // ReSharper disable UnusedMember.Global

      public static decimal Params(decimal arg, params decimal[] args)
      {
        return arg + (args.Length > 0 ? args[0] : 0.0m);
      }

      public static decimal Params2(params decimal[] args)
      {
        if (args.Length == 0) return 1;

        decimal avg = 0;
        foreach (decimal c in args) avg += c;
        return avg / args.Length;
      }

      public static decimal Params3(
        decimal a, decimal b, params decimal[] args)
      {
        return a + b;
      }

      public decimal Inst0()
      {
        return this.x;
      }

      public decimal Inst1(decimal arg)
      {
        return this.x + arg;
      }

      public decimal Inst2(decimal arg1, decimal arg2)
      {
        return this.x + arg1 / arg2;
      }

      public decimal InstP(params decimal[] args)
      {
        if (args == null)
          throw new ArgumentNullException("args");

        decimal res = this.x;
        foreach (decimal d in args) res += d;

        return res;
      }

      // ReSharper restore UnusedMember.Global

      #endregion
      #region ITestSupport

      public CalcContext<decimal> Context
      {
        get { return this.context; }
      }

      public decimal Value
      {
        get { return this.x; }
      }

      public void EqualityAssert(decimal a, decimal b)
      {
        try
        {
          Assert.IsTrue(a == b, "{0} <> {1}", a, b);
        }
        catch
        {
          throw;
        }
      }

      #endregion
    }

    #endregion
    #region EvalTester

    sealed class EvalTester<T, TSupport>
      where TSupport : ITestSupport<T>, new()
    {
      #region Support

      readonly TSupport support = new TSupport();

      TSupport Support
      {
        get { return this.support;}
      }

      #endregion
      #region Tests

      public void EvaluationTest()
      {
        var c = Support.Context;
        var x = Support.Value;
        var gen = new ExprGenerator<T>(c);

        string now = string.Empty;

        foreach (var mode in OptimizerModes)
        {
          c.Optimization = mode;
          foreach (string expr in gen.Generate(500))
          {
            try
            {
              now = "Quick Interpret";
              T int2 = c.Evaluate(expr, x);

              now = "Interpret";
              var itr = c.CreateInterpret(expr);
              T int1 = itr.Evaluate(x);

              Support.EqualityAssert(int1, int2);

#if !CF

              now = "Evaluator";
              var evl = c.CreateEvaluator(expr);
              T eval = evl.Evaluate(x);

              Support.EqualityAssert(int1, eval);

#endif

              // yeeah!
              if (Rnd.Next() % 25 == 0)
              {
                string name = GenRandomName();
                now = "Add " + name + " func";

#if !CF

                if (Rnd.Next() % 2 == 0)
                  c.Functions.Add(name, evl.Evaluate1);
                else

#endif
#if !CF2
                c.Functions.Add(name,
                  (EvalFunc1<T>) itr.Evaluate);
#endif
              }
            }
            catch (OverflowException) { }
            catch (DivideByZeroException) { }
            catch (Exception)
            {
              Trace.WriteLine(now);
              Trace.WriteLine(expr);
              throw;
            }

            // Trace.WriteLine(expr, "=> ");
            // Trace.WriteLine(eval, "[1]");
            // Trace.WriteLine(int1, "[2]");
            // Trace.WriteLine(int2, "[3]");
            // Trace.WriteLine("");
          }
        }
      }

      //TODO: compiler?
      public void OptimizerTest()
      {
        var c = Support.Context;
        var x = Support.Value;
        var gen = new ExprGenerator<T>(c);

        foreach (string expr in gen.Generate(20000))
        {
          try
          {
            c.Optimization = OptimizeModes.None;

            T res1N = c.CreateInterpret(expr).Evaluate(x);
            T res2N = c.Evaluate(expr, x);

            c.Optimization = OptimizeModes.ConstantFolding | OptimizeModes.FunctionFolding;

            T res1O = c.CreateInterpret(expr).Evaluate(x);
            T res2O = c.Evaluate(expr, x);

            Support.EqualityAssert(res1N, res1O);
            Support.EqualityAssert(res2N, res2O);
          }
          catch (OverflowException) { }
          catch (DivideByZeroException) { }
          catch
          {
            Trace.WriteLine(expr);
            throw;
          }

          //Trace.WriteLine(expr);
          //Trace.Indent();
          //Trace.WriteLine(string.Format("Normal:    {0}", res2N));
          //Trace.WriteLine(string.Format("Optimized: {0}", res2O));
          //Trace.Unindent();
          //Trace.WriteLine(string.Empty);
        }
      }

      #endregion
      #region Helpers

      static string GenRandomName()
      {
        var buf = new StringBuilder(30);

        for (int i = 0; i < 30; i++)
        {
          char c = (char) ('a' + Rnd.Next(0, 26));
          if (Rnd.Next() % 2 == 0) c = char.ToUpper(c);
          buf.Append(c);
        }

        return buf.ToString();
      }

      static IEnumerable<OptimizeModes> OptimizerModes
      {
        get
        {
          var mode = OptimizeModes.None;
          while (mode <= OptimizeModes.PerformAll)
          {
            yield return mode;
            mode++;
          }
        }
      }

      #endregion
    }

    #endregion
    #region DecimalLoadConstant

#if !CF

    [TestMethod]
    public void DecimalLoadConstantTest()
    {
      var rnd = new Random();
      var calc = new CalcContext<decimal>();

      for (int i = 0; i < 10000; i++)
      {
        decimal d;
        if (rnd.Next() % 2 == 1)
        {
          int hi = rnd.Next(int.MinValue, int.MaxValue);
          int mid = rnd.Next(int.MinValue, int.MaxValue);
          int low = rnd.Next(int.MinValue, int.MaxValue);
          bool sign = rnd.Next() % 2 == 0;
          byte scale = (byte) rnd.Next(0, 28);

          d = new decimal(hi, mid, low, sign, scale);
        }
        else
        {
          d = new decimal(rnd.NextDouble());
        }

        decimal d1 = calc.CreateEvaluator(d.ToString()).Evaluate();

        Assert.IsTrue(d == d1);
      }
    }

#endif

    #endregion
    #region CheckedOpsTests

    [TestMethod]
    public void CheckedOperationsTest()
    {
      TestOverflows<Int32>(
        Int32.MaxValue.ToString(),
        Int32.MinValue.ToString());

      TestOverflows<Int64>(
        Int64.MaxValue.ToString(),
        Int64.MinValue.ToString());

      TestOverflows<Decimal>(
        Decimal.MaxValue.ToString(),
        Decimal.MinValue.ToString());
    }

    static void TestOverflows<T>(string max, string min)
    {
      var context = new CalcContext<T>
        { OverflowCheck = true };

      Action<string> assert = s =>
        AssertOverflow(context, s);

      if (typeof(T) != typeof(decimal))
      {
        assert(string.Format("-({0})", min));
      }

      assert(string.Format("{0}-1", min));
      assert(string.Format("{0}+1", max));
      assert(string.Format("{0}*{0}", max));
      assert(string.Format("{0}^2", max));
      assert(string.Format("{0}^2", min));
    }

    static void AssertOverflow<T>(
      CalcContext<T> context, string expr)
    {
      bool flag = false;

      try { context.Evaluate(expr); }
      catch(OverflowException) { flag = true; }

      if (!flag) throw new Exception();

#if !CF
      try { context.CreateEvaluator(expr).Evaluate(); }
      catch(OverflowException) { flag = false; }

      if (flag) throw new Exception();
#endif
    }

    #endregion
  }
}