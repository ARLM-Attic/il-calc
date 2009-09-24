using System;
using System.Reflection;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class FunctionsTests
  {
    #region Initialize

    readonly CalcContext<double> calc;
    readonly double x, v;

    public FunctionsTests()
    {
      var random = new Random();

      this.calc = new CalcContext<double>("x");
      this.x = random.NextDouble() * 20;
      this.v = random.NextDouble() * 10;

      // static methods:
      Calc.Functions.Import(typeof(FunctionsTests));

      // instance methods:
      Calc.Functions.Add("Inst0", Inst0);
      Calc.Functions.Add("Inst1", Inst1);
      Calc.Functions.Add("Inst2", Inst2);
      Calc.Functions.Add("InsParams", InsParams);
      Calc.Functions.AddInstance(
        typeof(FunctionsTests)
          .GetMethod("InsParams2",
           BindingFlags.Instance |
           BindingFlags.Public),
        this);
    }

    CalcContext<double> Calc
    {
      get { return this.calc; }
    }

    #endregion
    #region ImportMethods

    public double Inst0()
    {
      return this.v;
    }

    public double Inst1(double a)
    {
      return -a / this.v;
    }

    public double Inst2(double a, double b)
    {
      return -a / b + this.v;
    }

    public double InsParams(params double[] args)
    {
      if (args == null)
        throw new ArgumentNullException("args");

      double res = this.v;
      foreach (double value in args) res /= value;

      return res;
    }

    public double InsParams2(
      double a, double b, params double[] args)
    {
      return a + SParams(args) / b + this.v;
    }

    public static double Stat0()
    {
      return 0.001;
    }

    public static double Stat1(double x)
    {
      return -x;
    }

    public static double Stat2(double x, double y)
    {
      return -x / y;
    }

    public static double SParams(params double[] args)
    {
      if (args == null)
        throw new ArgumentNullException("args");

      double res = 1;
      foreach (double value in args) res /= value;

      return res;
    }

    public static double SParams2(
      double x, double y, params double[] args)
    {
      return x + SParams(args) / y;
    }

    #endregion
    #region CallsTests

    [TestMethod]
    public void QuickInterpretCallsTest()
    {
      DoTests(e => Calc
        .Evaluate(e, this.x));
    }

    [TestMethod]
    public void InterpretCallsTest()
    {
      DoTests(e => Calc
        .CreateInterpret(e)
        .Evaluate(this.x));
    }

#if !CF

    [TestMethod]
    public void EvaluatorCallsTest()
    {
      DoTests(e => Calc
        .CreateEvaluator(e)
        .Evaluate(this.x));
    }

#endif

    void DoTests(Evaluator eval)
    {
      AssertTester tester = (e, ex) =>
        Assert.AreEqual(ex, eval(e));

      Trace.WriteLine("Static calls test...");
      StaticTests(tester);

      Trace.WriteLine("Instance calls test...");
      InstanceTests(tester);
    }

    void StaticTests(AssertTester test)
    {
      // with no arguments
      test("1 + Stat0()",
            1 + Stat0());

      // with one argument
      test("1 / Stat1(x)",
            1 / Stat1(x));

      // with two arguments
      test("2 * Stat2(x, 3)",
            2 * Stat2(x, 3));

      // all together
      test("Stat2(Stat1(-x), Stat0())",
            Stat2(Stat1(-x), Stat0()));

      // empty params array
      test("7 * x + SParams()",
            7 * x + SParams());

      // filled params array
      test("SParams(1, x) * SParams(4, x, 6)",
            SParams(1, x) * SParams(4, x, 6));

      // static call with normal and params args:
      test("SParams2(1, x) * SParams2(x, 5, 6)",
            SParams2(1, x) * SParams2(x, 5, 6));

      // all together:
      test("2 * SParams(1, x, Stat2(1, SParams2(6, x, 7)), 3)",
            2 * SParams(1, x, Stat2(1, SParams2(6, x, 7)), 3));
    }

    void InstanceTests(AssertTester test)
    {
      // with no arguments
      test("1 + Inst0()",
            1 + Inst0());

      // with one argument
      test("1 / Inst1(x)",
            1 / Inst1(x));

      // with two arguments
      test("2 * Inst2(x, 3)",
            2 * Inst2(x, 3));

      // all together
      test("Inst2(Inst1(-x), Inst0())",
            Inst2(Inst1(-x), Inst0()));

      // empty params array
      test("7 * x + InsParams()",
            7 * x + InsParams());

      // filled params array
      test("InsParams(1, x) * InsParams(4, x, 6)",
            InsParams(1, x) * InsParams(4, x, 6));

      // static call with normal and params args:
      test("InsParams2(1, x) * InsParams2(x, 5, 6)",
            InsParams2(1, x) * InsParams2(x, 5, 6));

      // all together:
      test("2 * InsParams(1, x, Inst2(1, InsParams2(6, x, 7)), 3)",
            2 * InsParams(1, x, Inst2(1, InsParams2(6, x, 7)), 3));
    }

    #endregion
    #region ImportTests

    [TestMethod]
    public void ImportTest()
    {
      var c = new CalcContext<double>();

      c.Constants.Import(typeof(double));
      Assert.AreEqual(c.Constants.Count, 6);

      c.Constants.Clear();
      c.Constants.ImportBuiltIn();
      Assert.AreEqual(c.Constants.Count, 5);

      c.Constants.Clear();
      c.Constants.Import(typeof(ClassForImport));
      Assert.AreEqual(c.Constants.Count, 1);

#if !SILVERLIGHT

      c.Constants.Clear();
      c.Constants.Import(typeof(ClassForImport), true);
      Assert.AreEqual(c.Constants.Count, 4);

#endif

      c.Constants.Clear();
      c.Constants.Import(
        typeof(ClassForImport), typeof(Math), typeof(double));
      Assert.AreEqual(c.Constants.Count, 9);

      c.Functions.ImportBuiltIn();
#if SILVERLIGHT || CF
      Assert.AreEqual(c.Functions.Count, 21);
#else
      Assert.AreEqual(c.Functions.Count, 22);
#endif

      c.Functions.Clear();
      c.Functions.Import(typeof(Math));
#if SILVERLIGHT
      Assert.AreEqual(c.Functions.Count, 22);
#else
      Assert.AreEqual(c.Functions.Count, 23);
#endif

      c.Functions.Clear();
      c.Functions.Import(typeof(ClassForImport));
      Assert.AreEqual(c.Functions.Count, 6);

#if !SILVERLIGHT

      c.Functions.Clear();
      c.Functions.Import(typeof(ClassForImport), true);
      Assert.AreEqual(c.Functions.Count, 7);

      //TODO: enable for silverlight/cf
      c.Functions.Clear();
      c.Functions.Import(typeof(ClassForImport), typeof(Math));
      Assert.AreEqual(c.Functions.Count, 29);

#endif

      // delegates
      c.Functions.Add("f1", ClassForImport.ParamsMethod1);
      c.Functions.Add("f2", ClassForImport.JustFunc);
      c.Functions.Add("f3", ClassForImport.StaticMethod);
      c.Functions.Add("f4", ClassForImport.StaticMethod1);
    }

    #endregion
    #region Helpers

    delegate void AssertTester(string expr, double actual);

    delegate double Evaluator(string expr);


    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedParameter.Local
    public class ClassForImport
    {
#pragma warning disable 169

      public const double Test = 0.123;
      const double Foo = 2323;
      const double Bar = 434343;
      static readonly double X = 5.55;

#pragma warning restore 169

      public static double JustFunc()
      {
        return X;
      }

      public double InstanceMethod(double y)
      {
        return 0;
      }

      public static double StaticMethod(double y)
      {
        return 0;
      }

      public static double StaticMethod1(double y, double z)
      {
        return 0;
      }

      static double HiddenMethod(
        double a, double b, double c)
      {
        return 0;
      }

      public static double ParamsMethod1(double[] args)
      {
        return 0;
      }

      public static double ParamsMethod2(double a, double[] args)
      {
        return 0;
      }

      public static double ParamsMethod3(
        double a, double b, double c, double[] args)
      {
        return 0;
      }
    }

    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedParameter.Local

    #endregion
  }
}