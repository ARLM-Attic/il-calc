using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class MultiThreadTests
  {
    #region Fields

    readonly CalcContext<double> calc;
    readonly Interpret<double> intr;
    readonly Evaluator<double> eval;
    readonly Random rnd;

    readonly Interpret<double> syncInterp;
    readonly object syncRoot;

    const string Expression = "Sum(pi*x + 2sin(y)+x; -y+2x; 2)^2 + x^3";

    #endregion
    #region Initialize

    public MultiThreadTests()
    {
      this.calc = new CalcContext<double>("x", "y");

      Calc.Functions.ImportBuiltIn();
      Calc.Constants.ImportBuiltIn();
      Calc.Functions.Import("Sum", typeof(MultiThreadTests));

      Calc.Optimization = OptimizeModes.PerformAll;

      this.intr = Calc.CreateInterpret(Expression);
      this.eval = Calc.CreateEvaluator(Expression);

      this.syncInterp = Calc.CreateInterpret(Expression);
      this.syncRoot = new object();

      this.rnd = new Random();
    }

    CalcContext<double> Calc
    {
      get { return this.calc; }
    }

    public static double Sum(
      double one, double two, double[] args)
    {
      double sum = one + two;
      foreach (double arg in args)
      {
        sum += arg;
      }

      return sum;
    }

    #endregion
    #region AsyncTabulateTest

    [TestMethod]
    public void AsyncTabulateTest()
    {
      var oldArgs = Calc.Arguments;

      Calc.Arguments.Clear();
      Calc.Arguments.Add("x");

      var tab = Calc.CreateTabulator("2sin(x) + cos(3x)");
      var range = new ValueRange<double>(0, 1000000, 0.1);

      var async = tab.BeginTabulate(range, null, null);

      var res1 = tab.Tabulate(range);
      var res2 = tab.EndTabulate(async);

      CollectionAssert.AreEqual(res1, res2);

      Calc.Arguments.Clear();
      Calc.Arguments.AddRange(oldArgs);
    }

    #endregion
    #region ThreadSafetyTest

    [TestMethod]
    public void ThreadSafetyTest()
    {
      const int Count = 200;

      for (int i = 0; i < Count; i++)
      {
        new Thread(ThreadMethod).Start();
      }
    }

    void ThreadMethod(object state)
    {
      double arg1 = this.rnd.NextDouble();
      double arg2 = this.rnd.NextDouble();

      double res1 = this.intr.EvaluateSync(arg1, arg2);
      double res2 = this.eval.Evaluate(arg1, arg2);

      if (res1 != res2)
      {
        lock (this.syncRoot)
        {
          Trace.WriteLine(
            string.Format("{0} != {1}", res2, res1));

          res1 = this.syncInterp.Evaluate(arg1, arg2);
          Trace.WriteLine(res1);
          Trace.WriteLine(string.Empty);

          Assert.AreEqual(res1, res2);
        }
      }
    }

    #endregion
  }
}