using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if !CF
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace ILCalc.Tests
{
  [TestClass]
  public sealed class ExceptionsTests
  {
    #region EvaluatorsTests

#if !CF

    [TestMethod]
    public void EvaluatorExceptions()
    {
      var calc = new CalcContext<double>("x", "y", "z");
      var eval = calc.CreateEvaluator("x+y+z");

      Assert.AreEqual(eval.ToString(), "x+y+z");
      Assert.AreEqual(eval.ArgumentsCount, 3);

      Throws<ArgumentException>(
        () => eval.Evaluate(),
        () => eval.Evaluate(1),
        () => eval.Evaluate0(),
        () => eval.Evaluate1(1),
        () => eval.Evaluate(1, 2),
        () => eval.Evaluate2(1, 2));

      // NOTE: bad, maybe doc it?
      Throws<NullReferenceException>(() => eval.EvaluateN(null));

      Throws<ArgumentException>(() => eval.Evaluate(1, 2, 3, 4));
      Throws<ArgumentNullException>(() => eval.Evaluate(null));

      calc.Arguments.Clear();
      calc.Arguments.Add("x");

      var eval2 = calc.CreateEvaluator("x");

      Throws<ArgumentException>(
        () => eval2.Evaluate(),
        () => eval2.Evaluate0(),
        () => eval2.Evaluate(1, 2),
        () => eval2.Evaluate2(1, 2));
    }

    [TestMethod]
    public void TabulatorExceptions()
    {
      var calc = new CalcContext<double>("x", "y", "z");
      var tab = calc.CreateTabulator("x+y+z");

      Assert.AreEqual(tab.ToString(), "x+y+z");
      Assert.AreEqual(tab.RangesCount, 3);

      var r = new ValueRange<double>(1, 10, 1);
      var arr1 = (double[]) Tabulator.Allocate(new[] { r });
      var arr2 = (double[][]) Tabulator.Allocate(new[] { r, r });

      Throws<ArgumentException>(
        () => tab.Tabulate(r),
        () => tab.Tabulate(r, r),
        () => tab.TabulateToArray(arr1, r),
        () => tab.TabulateToArray(arr2, r, r),
        () => tab.BeginTabulate(r, null, null),
        () => tab.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab.Tabulate(),
        () => tab.Tabulate(new ValueRange<double>[] { }),
        () => tab.Tabulate(new[] { r }),
        () => tab.Tabulate(new[] { r, r }),
        () => tab.Tabulate(r, r, r, r),
        () => tab.TabulateToArray(arr1),
        () => tab.TabulateToArray(arr1, new ValueRange<double>[] { }),
        () => tab.TabulateToArray(arr1, new[] { r }),
        () => tab.TabulateToArray(arr1, new[] { r, r }),
        () => tab.TabulateToArray(arr1, r, r, r, r));

      Throws<ArgumentNullException>(
        () => tab.Tabulate(null),
        () => tab.TabulateToArray(null),
        () => tab.TabulateToArray(null, r),
        () => tab.TabulateToArray(null, r, r),
        () => tab.TabulateToArray(null, 0, 1),
        () => tab.TabulateToArray(null, r, r, r),
        () => tab.TabulateToArray(arr1, null));

      calc.Arguments.Clear();
      calc.Arguments.Add("x");

      var tab2 = calc.CreateTabulator("x");

      tab2.Tabulate(0, 10, 1);
      tab2.TabulateToArray(arr1, 0, 1);

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r),
        () => tab2.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r, r),
        () => tab2.BeginTabulate(new ValueRange<double>[] { }, null, null),
        () => tab2.BeginTabulate(new[] { r, r }, null, null),
        () => tab2.BeginTabulate(new[] { r, r, r, r }, null, null),
        () => tab2.BeginTabulate(r, r, r, null, null));

      Throws<ArgumentNullException>(
        () => Tabulator.Allocate<double>(null),
        () => tab.BeginTabulate(null, null, null),
        () => tab.EndTabulate(null));

      var async = tab2.BeginTabulate(r, null, null);
      tab2.EndTabulate(async);

      Throws<InvalidOperationException>(
        () => tab2.EndTabulate(async));
    }

    [TestMethod]
    public void TabulatorTabExceptions()
    {
      var calc = new CalcContext<double>("x", "y", "z");
      var tab = calc.CreateInterpret("x+y+z");

      Assert.AreEqual(tab.ToString(), "x+y+z");
      Assert.AreEqual(tab.ArgumentsCount, 3);

      var r = new ValueRange<double>(1, 10, 1);
      var arr1 = (double[]) Tabulator.Allocate(new[] { r });
      var arr2 = (double[][]) Tabulator.Allocate(new[] { r, r });

      Throws<ArgumentException>(
        () => tab.Tabulate(r),
        () => tab.Tabulate(r, r),
        () => tab.TabulateToArray(arr1, r),
        () => tab.TabulateToArray(arr2, r, r),
        () => tab.BeginTabulate(r, null, null),
        () => tab.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab.Tabulate(),
        () => tab.Tabulate(new ValueRange<double>[] { }),
        () => tab.Tabulate(new[] { r }),
        () => tab.Tabulate(new[] { r, r }),
        () => tab.Tabulate(r, r, r, r),
        () => tab.TabulateToArray(arr1),
        () => tab.TabulateToArray(arr1, new ValueRange<double>[] { }),
        () => tab.TabulateToArray(arr1, new[] { r }),
        () => tab.TabulateToArray(arr1, new[] { r, r }),
        () => tab.TabulateToArray(arr1, r, r, r, r));

      Throws<ArgumentNullException>(
        () => tab.Tabulate(null),
        () => tab.TabulateToArray(null),
        () => tab.TabulateToArray(null, r),
        () => tab.TabulateToArray(null, r, r),
        () => tab.TabulateToArray(null, 0, 1),
        () => tab.TabulateToArray(null, r, r, r),
        () => tab.TabulateToArray(arr1, null));

      calc.Arguments.Clear();
      calc.Arguments.Add("x");

      var tab2 = calc.CreateInterpret("x");

      tab2.Tabulate(0, 10, 1);
      tab2.TabulateToArray(arr1, 0, 1);

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r),
        () => tab2.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r, r),
        () => tab2.BeginTabulate(new ValueRange<double>[] { }, null, null),
        () => tab2.BeginTabulate(new[] { r, r }, null, null),
        () => tab2.BeginTabulate(new[] { r, r, r, r }, null, null),
        () => tab2.BeginTabulate(r, r, r, null, null));

      Throws<ArgumentNullException>(
        () => Tabulator.Allocate<double>(null),
        () => tab.BeginTabulate(null, null, null),
        () => tab.EndTabulate(null));

      var async = tab2.BeginTabulate(r, null, null);
      tab2.EndTabulate(async);

      Throws<InvalidOperationException>(
        () => tab2.EndTabulate(async));
    }

#endif

    [TestMethod]
    public void InterpretExceptions()
    {
      var calc = new CalcContext<double>("x", "y", "z");

      var inter = calc.CreateInterpret("x+y+z");

      Assert.AreEqual(inter.ToString(), "x+y+z");
      Assert.AreEqual(inter.ArgumentsCount, 3);

      Throws<ArgumentException>(
        () => inter.Evaluate(),
        () => inter.Evaluate(1),
        () => inter.Evaluate(1, 2),
        () => inter.Evaluate(1, 2, 3, 4));

      Throws<ArgumentNullException>(
        () => inter.Evaluate(null));

      calc.Arguments.Clear();
      calc.Arguments.Add("x");

      var inter2 = calc.CreateInterpret("x");

      Throws<ArgumentException>(
        () => inter2.Evaluate(),
        () => inter2.Evaluate(1, 2));
    }

    [TestMethod]
    public void InterpretTabExceptions()
    {
      var calc = new CalcContext<double>("x", "y", "z");
      var tab = calc.CreateInterpret("x+y+z");

      Assert.AreEqual(tab.ToString(), "x+y+z");
      Assert.AreEqual(tab.ArgumentsCount, 3);

      var r = new ValueRange<double>(1, 10, 1);
      var arr1 = (double[]) Interpret.Allocate(new[] { r });
      var arr2 = (double[][]) Interpret.Allocate(new[] { r, r });

      Throws<ArgumentException>(
        () => tab.Tabulate(r),
        () => tab.Tabulate(r, r),
        () => tab.TabulateToArray(arr1, r),
        () => tab.TabulateToArray(arr2, r, r),
        () => tab.BeginTabulate(r, null, null),
        () => tab.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab.Tabulate(),
        () => tab.Tabulate(new ValueRange<double>[] { }),
        () => tab.Tabulate(new[] { r }),
        () => tab.Tabulate(new[] { r, r }),
        () => tab.Tabulate(r, r, r, r),
        () => tab.TabulateToArray(arr1),
        () => tab.TabulateToArray(arr1, new ValueRange<double>[] { }),
        () => tab.TabulateToArray(arr1, new[] { r }),
        () => tab.TabulateToArray(arr1, new[] { r, r }),
        () => tab.TabulateToArray(arr1, r, r, r, r));

      Throws<ArgumentNullException>(
        () => tab.Tabulate(null),
        () => tab.TabulateToArray(null),
        () => tab.TabulateToArray(null, r),
        () => tab.TabulateToArray(null, r, r),
        () => tab.TabulateToArray(null, 0, 1),
        () => tab.TabulateToArray(null, r, r, r),
        () => tab.TabulateToArray(arr1, null));

      calc.Arguments.Clear();
      calc.Arguments.Add("x");

      var tab2 = calc.CreateInterpret("x");

      tab2.Tabulate(0, 10, 1);
      tab2.TabulateToArray(arr1, 0, 1);

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r),
        () => tab2.BeginTabulate(r, r, null, null));

      Throws<ArgumentException>(
        () => tab2.Tabulate(r, r, r),
        () => tab2.BeginTabulate(new ValueRange<double>[] { }, null, null),
        () => tab2.BeginTabulate(new[] { r, r }, null, null),
        () => tab2.BeginTabulate(new[] { r, r, r, r }, null, null),
        () => tab2.BeginTabulate(r, r, r, null, null));

      Throws<ArgumentNullException>(
        () => Interpret.Allocate<double>(null),
        () => tab.BeginTabulate(null, null, null),
        () => tab.EndTabulate(null));

      var async = tab2.BeginTabulate(r, null, null);
      tab2.EndTabulate(async);

      Throws<InvalidOperationException>(
        () => tab2.EndTabulate(async));
    }

    #endregion
    #region ValueRangeTests

    [TestMethod]
    public void InvalidRangesTests()
    {
      Throws<InvalidRangeException>
      (
        // double:
        () => Validate(Double.NaN, 10, 1),
        () => Validate(Double.PositiveInfinity, 10, 1),
        () => Validate(Double.NegativeInfinity, 10, 1),
        () => Validate(1e+300, 2e+300, 0.0001),
        () => Validate(1e64, 2e64, 1e-32),
        () => Validate(10.0, 0.0, 1.0),
        () => Validate(0.0, 1.0, Double.Epsilon),

        // float:
        () => Validate(Single.NaN, 10, 1),
        () => Validate(Single.PositiveInfinity, 10, 1),
        () => Validate(Single.NegativeInfinity, 10, 1),
        () => Validate(1e+38f, 2e+38f, 0.0001f),
        () => Validate(1e38f, 2e38f, 1e-32f),
        () => Validate(10.0f, 0.0f, 1.0f),
        () => Validate(0.0f, 1.0f, Single.Epsilon),

        // int32:
        () => Validate(0, 10, 0),
        () => Validate(0, 10, -1),

        // int64:
        () => Validate(0, 10L, 0),
        () => Validate(0, 10L, -1),
        () => Validate(0, long.MaxValue, 1),

        // decimal:
        () => Validate(Decimal.MinValue, 0, -0.001m),
        () => Validate(0m, Decimal.MaxValue, 0.001m),
        () => Validate(1e+28m, 2e+28m, 0.000000001m),
        () => Validate(0m, 100, -1),

        // unknown:
        () => Validate(new Random(), null, null)
      );
    }

    static void Validate<T>(T begin, T end, T step)
    {
      ValueRange
        .Create(begin, end, step)
        .Validate();
    }

    #endregion
    #region VisibilityChecksTests

#if SILVERLIGHT

    [TestMethod]
    public void VisibilityTests()
    {
      var calc = new CalcContext<int>();
      var thisType = typeof(ExceptionsTests);
      var internalClass = typeof(InternalClass);

      calc.Functions.AddDel<Func<int>>("foo", NonPublicInstanceMethod);
      calc.Functions.AddDel<Func<int>>("foo2", NonPublicStaticMethod);
      calc.Functions.AddDel<Func<int>>("bar", new InternalClass().Bar);
      calc.Functions.AddDel<Func<int>>("bar2", InternalClass.Foo);

      Throws<ArgumentException>
      (
        () => calc.Functions.AddInstance(thisType
          .GetMethod("NonPublicInstanceMethod",
            BindingFlags.NonPublic | BindingFlags.Instance), this),
        () => calc.Functions.AddStatic(thisType
          .GetMethod("NonPublicStaticMethod",
            BindingFlags.NonPublic | BindingFlags.Static)),
        () => calc.Functions.AddInstance(internalClass
          .GetMethod("Bar",BindingFlags.Public | BindingFlags.Instance), this),
        () => calc.Functions.AddStatic(internalClass
          .GetMethod("Foo", BindingFlags.Public | BindingFlags.Static)),
        () => calc.CreateInterpret("foo()"),
        () => calc.CreateInterpret("bar()"),
        () => calc.CreateInterpret("foo2()"),
        () => calc.CreateInterpret("bar2()"),
        () => calc.CreateEvaluator("foo()"),
        () => calc.CreateEvaluator("bar()"),
        () => calc.CreateEvaluator("foo2()"),
        () => calc.CreateEvaluator("bar2()")
      );
    }

    private int NonPublicInstanceMethod() { return 0; }

    private static int NonPublicStaticMethod() { return 1; }

    class InternalClass
    {
      public int Bar() { return 0; }
      public static int Foo() { return 1; }
    }

#endif

    #endregion
    #region ImportExceptionsTests

#if !CF

    [TestMethod]
    public void ImportExceptionsTest()
    {
      var collection = new FunctionCollection<double>();

      var method = new System.Reflection.Emit.DynamicMethod(
        "test", typeof(double), Type.EmptyTypes);

      var il = method.GetILGenerator();
      il.Emit(System.Reflection.Emit.OpCodes.Ldc_R8, 1.0);
      il.Emit(System.Reflection.Emit.OpCodes.Ret);

      var func = (EvalFunc0<double>)
        method.CreateDelegate(typeof(EvalFunc0<double>));

      var func1 = typeof(ExceptionsTests).GetMethod("Func1");

      Throws<ArgumentException>(
        () => collection.AddStatic(func.Method),
        () => collection.AddInstance(func1, this));
    }

    public static double Func1()
    {
      return 0;
    }

#endif

    #endregion
    #region FunctionCollectionTests

    [TestMethod]
    public void FunctionCollectionTests()
    {
      var funcs = new FunctionCollection<int>();
      var type = typeof(ExceptionsTests);
      var foo1 = type.GetMethod("Foo1");
      var foo2 = type.GetMethod("Foo2");

      Throws<ArgumentNullException>(
#if !CF2
        () => funcs.Add<EvalFunc0<int>>(null),
        () => funcs.Add((EvalFunc0<int>) null),
        () => funcs.Add((EvalFunc1<int>) null),
        () => funcs.Add((EvalFunc2<int>) null),
        () => funcs.Add((EvalFuncN<int>) null),
#endif
        () => funcs.AddDel<EvalFunc0<int>>(null, null),
        () => funcs.Add(null, () => 1),
        () => funcs.Add(null, x => x),
        () => funcs.Add(null, (x,y) => x),
        () => funcs.Add(null, xs => xs.Length),

        () => funcs.AddInstance(null, this),
        () => funcs.AddInstance(foo1, null),

        () => funcs.AddInstance(null, null, this),
        () => funcs.AddInstance(null, foo1, null),
        () => funcs.AddInstance("sds", null, this),
        () => funcs.AddInstance("sds", foo1, null),

        () => funcs.AddStatic(null),
        () => funcs.AddStatic(null, foo1),
        () => funcs.AddStatic("sd", null)
      );

      EvalFunc1<int> multicast = z => z + 1;
      multicast += z => z - 1;

#if !CF
      var dyn = new DynamicMethod("dyn", typeof(int), Type.EmptyTypes);
      var il = dyn.GetILGenerator();
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Ret);

      var dynDeleg = (EvalFunc0<int>)
        dyn.CreateDelegate(typeof(EvalFunc0<int>));

      funcs.Add(dynDeleg);
      funcs.AddDel("abc", dynDeleg);
#endif

      Throws<ArgumentException>(
        () => funcs.AddStatic(foo2),
        () => funcs.AddStatic("sdsd", foo2),
        () => funcs.AddInstance(foo1, this),
        () => funcs.AddInstance("sds", foo1, this),

        // not assignable target
        () => funcs.AddInstance("sds", foo2, "xxx"),
        () => funcs.AddInstance(foo1, "xxx"),
        
        // multicast delegates
        () => funcs.Add("multi", multicast),
        () => funcs.AddDel("multi", multicast),

#if !CF
        // dynamic method
        () => funcs.AddStatic(dynDeleg.Method),
#endif
        // non applicable
        () => funcs.AddStatic("bar1", type.GetMethod("Bar1")),
        () => funcs.AddStatic("bar2", type.GetMethod("Bar2")),
        () => funcs.AddStatic("bar3", type.GetMethod("Bar3")),
        () => funcs.AddStatic("bar4", type.GetMethod("Bar4")),
        () => funcs.AddStatic("bar5", type.GetMethod("Bar5")),

        () => funcs.Import("Foo3", typeof(ExceptionsTests), 3)
      );

      funcs.Import("Foo1", typeof(ExceptionsTests));
      funcs.Remove("Foo1");
      funcs.Import("Foo1", typeof(ExceptionsTests), 0);

      Throws<ArgumentNullException>(
        () => funcs.Import((Type[]) null),
        () => funcs.Import(new Type[]{ null }),
        () => funcs.Import(null, type, 0),
        () => funcs.Import("sd", null, 0),
        () => funcs.Import((string) null, type),
        () => funcs.Import("sd", null)
        );

      Throws<ArgumentOutOfRangeException>(
        () => funcs.Import("foo1", type, -1)
        );
    }

    public static void Bar1() { }
    public static int Bar2(int x, double y) { return 0; }
    public static int Bar3(ref int y, int x) { return 0; }
    public static int Bar4(out int y, int x) { y = 2; return 0; }
    public static int Bar5(ref double[] args) { return 0; }

    public static int Foo1() { return 0; }
    public int Foo2() { return 0; }

    #endregion
    #region Helpers

    delegate void Action();

    static void Throws<TException>(params Action[] actions)
      where TException : Exception
    {
      if (actions == null)
        throw new ArgumentNullException("actions");

      foreach (Action action in actions)
      {
        try
        {
          action();
        }
        catch (TException e)
        {
          var buf = new StringBuilder();

          buf.Append('\'');
          buf.Append(e.Message);
          buf.Append("'.");

          Trace.WriteLine(buf.ToString(), typeof(TException).Name);
          continue;
        }
        catch
        {
          action();

          throw new InternalTestFailureException(
            typeof(TException).Name + " doesn't thrown!");
        }

        action();

        throw new InternalTestFailureException(
          typeof(TException).Name + " doesn't thrown!");
      }
    }

    #endregion
  }
}