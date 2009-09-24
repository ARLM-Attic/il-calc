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

    #endregion
    #region ITestSupport

    interface ITestSupport<T>
    {
      CalcContext<T> Context { get; }
      T Value { get; }

      void EqualityAssert(T a, T b);
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
        double avg = 0;
        foreach (double c in args)
        {
          avg += c;
        }

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

            c.Optimization = OptimizeModes.PerformAll;

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
  }
}