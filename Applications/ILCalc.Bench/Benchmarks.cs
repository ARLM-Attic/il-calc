using System;
using System.Diagnostics;
using System.Globalization;
using Evaluant.Calculator;
using Ciloci.Flee;

namespace ILCalc.Bench
{
  public static class Benchmarks
  {
    #region Benchmarks

    public static void InitializeTest()
    {
      Tester.Run(
        "Initialization time", 10000,
        () => {
          var flee = new ExpressionContext();
          flee.Imports.AddType(typeof(Math));
          flee.Imports.AddType(typeof(Imports));
          return "FLEE";
        },
        () => {
          var ilcalc = new CalcContext<double>();
          ilcalc.Functions.ImportBuiltIn();
          ilcalc.Functions.Import(typeof(Imports));
          return "ILCalc";
        });
    }

    public static void EvaluateOnceTest()
    {
      Tester.Run(
        "Calculate 2+2", 1000,
        () => {
          new ExpressionContext()
            .CompileGeneric<int>("2+2")
            .Evaluate();
          return "FLEE";
        },
        () => {
          new Expression("2+2")
            .Evaluate();
          return "NCalc";
        },
        () => {
          new CalcContext<double>()
            .CreateEvaluator("2+2")
            .Evaluate();
          return "ILCalc Eval";
        },
        () => {
          new CalcContext<double>()
            .CreateInterpret("2+2")
            .Evaluate();
          return "ILCalc Interp";
        },
        () => {
          new CalcContext<double>()
            .Evaluate("2+2");
          return "ILCalc QInterp";
        });
    }

    public static void ManyEvaluationsTest()
    {
      const string ExprFlee = "4 * cos(2 + sin(x) / 6)";
      const string ExprILCalc = "4 * cos(2 + sin(x) / 6)";
      const string ExprNCalc = "4.0 * Cos(2.0 + Sin(x) / 6.0)";

      var flee = new ExpressionContext();
      flee.Variables["x"] = 0.0;
      flee.Options.ParseCulture = CultureInfo.InvariantCulture;
      flee.Options.IntegersAsDoubles = true;
      flee.Imports.AddType(typeof(Math));
      var fleeExpr = flee.CompileGeneric<double>(ExprFlee);

      var owner = new FleeOwner();
      var flee2 = new ExpressionContext(owner);
      flee2.Options.ParseCulture = CultureInfo.InvariantCulture;
      flee2.Options.IntegersAsDoubles = true;
      flee2.Imports.AddType(typeof(Math));
      var flee2Expr = flee2.CompileGeneric<double>(ExprFlee);

      var ilCalc = new CalcContext<double>("x");
      ilCalc.Functions.ImportBuiltIn();
      var ilCalcEvaluator = ilCalc.CreateEvaluator(ExprILCalc);
      var ilCalcInterpret = ilCalc.CreateInterpret(ExprILCalc);

      var rnd = new Random();

      Tester.Run(
        "Many evaluations test", 300000,
        () => {
          flee.Variables["x"] = rnd.NextDouble();
          fleeExpr.Evaluate();
          return "FLEE";
        },
        () => {
          owner.x = rnd.NextDouble();
          flee2Expr.Evaluate();
          return "FLEE + Owner";
        },
        () => {
          var e = new Expression(ExprNCalc);
          e.Parameters["x"] = rnd.NextDouble();
          e.Evaluate();
          return "NCalc";
        },
        () => {
          ilCalcEvaluator.Evaluate1(rnd.NextDouble());
          return "ILCalc Eval";
        },
        () => {
          ilCalcInterpret.Evaluate(rnd.NextDouble());
          return "ILCalc Interp";
        },
        () => {
          ilCalc.Evaluate(ExprILCalc, rnd.NextDouble());
          return "ILCalc QInterp";
        });
    }

    public static void ParseAndCompileTest()
    {
      const string ExprFlee = "4 * cos(2 + sin(x) / 6)";
      const string ExprILCalc = "4 * cos(2 + sin(x) / 6)";

      var flee = new ExpressionContext();
      flee.Options.IntegersAsDoubles = true;
      flee.Variables["x"] = 1.23;
      flee.Imports.AddType(typeof(Math));

      var ilCalc = new CalcContext<double>("x");
      ilCalc.Functions.ImportBuiltIn();

      Tester.Run(
        "Parse & compile", 10000,
        () => {
          flee.CompileGeneric<double>(ExprFlee);
          return "FLEE";
        },
        () => {
          ilCalc.CreateEvaluator(ExprILCalc);
          return "ILCalc";
        });
    }

    public static void ILCalcOptimizerTest()
    {
      const string Expression =
        "(2*6sin(5x^2) + 4tan(4pi + y^2)) / sin(2pi)";

      var calc = new CalcContext<double>("x", "y");
      calc.Constants.ImportBuiltIn();
      calc.Functions.ImportBuiltIn();

      var eval0 = calc.CreateEvaluator(Expression);
      var intr0 = calc.CreateInterpret(Expression);

      calc.Optimization = OptimizeModes.ConstantFolding;
      var eval1 = calc.CreateEvaluator(Expression);
      var intr1 = calc.CreateInterpret(Expression);

      calc.Optimization |= OptimizeModes.PowOptimize;
      var eval2 = calc.CreateEvaluator(Expression);
      var intr2 = calc.CreateInterpret(Expression);

      calc.Optimization |= OptimizeModes.FunctionFolding;
      var eval3 = calc.CreateEvaluator(Expression);
      var intr3 = calc.CreateInterpret(Expression);

      Tester.Run(
        "Optimizer test", 1000000,
        () => {
          eval0.Evaluate(1.23, 4.56);
          return "Evaluator";
        },
        () => {
          eval1.Evaluate(1.23, 4.56);
          return "+ConstFolding";
        },
        () => {
          eval2.Evaluate(1.23, 4.56);
          return "+PowOptimize";
        },
        () => {
          eval3.Evaluate(1.23, 4.56);
          return "+FuncFolding";
        },
        () => {
          intr0.Evaluate(1.23, 4.56);
          return "Interpret";
        },
        () => {
          intr1.Evaluate(1.23, 4.56);
          return "+ConstFolding";
        },
        () => {
          intr2.Evaluate(1.23, 4.56);
          return "+PowOptimize";
        },
        () => {
          intr3.Evaluate(1.23, 4.56);
          return "+FuncFolding";
        }
        );
    }

    public static void TabulationTest()
    {
      const string ExprFlee = "sin(x) + cos(x)";
      const string ExprILCalc = "sin(x) + cos(x)";

      var flee = new ExpressionContext(new FleeOwner());
      flee.Options.IntegersAsDoubles = true;
      flee.Imports.AddType(typeof(Math));

      var fleeExpr = flee.CompileGeneric<double>(ExprFlee);

      var ilCalc = new CalcContext<double>("x");
      ilCalc.Functions.ImportBuiltIn();

      var ilCalcEval = ilCalc.CreateEvaluator(ExprILCalc);
      var ilCalcIntr = ilCalc.CreateInterpret(ExprILCalc);
      var ilCalcTab = ilCalc.CreateTabulator(ExprILCalc);

      var range = new ValueRange<double>(0, 1000000, 1);

      Tester.Run(
        "Tabulation", 10,
        () => {
          FleeTabHelper(fleeExpr, range);
          return "FLEE + Owner";
        },
        () => {
          InterpretTabHelper(ilCalcIntr, range);
          return "ILCalc Interp";
        },
        () => {
          EvaluatorTabHelper(ilCalcEval, range);
          return "ILCalc Eval";
        },
        () => {
          ilCalcTab.Tabulate(range);
          return "ILCalc Tab";
        });
    }

    #endregion
    #region Helper

    static double[] FleeTabHelper(
      IGenericExpression<double> expr, ValueRange<double> range)
    {
      Debug.Assert(range.IsValid);

      var array = new double[range.Count];
      var owner = new FleeOwner { x = range.Begin };
      expr.Owner = owner;

      for (int i = 0; i < array.Length; i++)
      {
        array[i] = expr.Evaluate();
        owner.x += range.Step;
      }

      return array;
    }

    static double[] InterpretTabHelper(
      Interpret<double> eval, ValueRange<double> range)
    {
      Debug.Assert(range.IsValid);

      var array = new double[range.Count];
      double x = range.Begin;

      for (int i = 0; i < array.Length; i++)
      {
        array[i] = eval.Evaluate(x);
        x += range.Step;
      }

      return array;
    }

    static double[] EvaluatorTabHelper(
      Evaluator<double> eval, ValueRange<double> range)
    {
      Debug.Assert(range.IsValid);

      var array = new double[range.Count];
      double x = range.Begin;

      for (int i = 0; i < array.Length; i++)
      {
        array[i] = eval.Evaluate1(x);
        x += range.Step;
      }

      return array;
    }

    #endregion
    #region Imports

    class FleeOwner
    {
      public double x;
    }

    public static class Imports
    {
      public static double Twice(double x)
      {
        return 2.0 * x;
      }
    }

    #endregion
  }
}