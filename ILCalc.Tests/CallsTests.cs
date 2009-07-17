using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
	[TestClass]
	public sealed class CallsTests
	{
		#region Initialize

		private readonly CalcContext calc;
		private readonly double x, v;

		public CallsTests()
		{
			var random = new Random();

			this.calc = new CalcContext("x");
			this.x = random.NextDouble() * 20;
			this.v = random.NextDouble() * 10;

			// static methods:
			Calc.Functions.Import(typeof(CallsTests), true);

			// instance methods:
			Calc.Functions.Add(Inst0);
			Calc.Functions.Add(Inst1);
			Calc.Functions.Add(Inst2);
			Calc.Functions.Add(InsParams);
			Calc.Functions.AddInstance(
				typeof(CallsTests)
				.GetMethod("InsParams2",
				 BindingFlags.Instance | BindingFlags.NonPublic),
				this);
		}

		private CalcContext Calc
		{
			get { return this.calc; }
		}

		#endregion
		#region Import Methods

		private double Inst0()
		{
			return this.v;
		}

		private double Inst1(double a)
		{
			return -a / this.v;
		}

		private double Inst2(double a, double b)
		{
			return -a / b + this.v;
		}

		private double InsParams(params double[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			double res = this.v;
			foreach (double value in args) res /= value;

			return res;
		}

		private double InsParams2(double a, double b, params double[] args)
		{
			return a + SParams(args) / b + this.v;
		}

		private static double Stat0()
		{
			return 0.001;
		}

		private static double Stat1(double x)
		{
			return -x;
		}

		private static double Stat2(double x, double y)
		{
			return -x / y;
		}

		private static double SParams(params double[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			double res = 1;
			foreach (double value in args) res /= value;

			return res;
		}

		private static double SParams2(double x, double y, params double[] args)
		{
			return x + SParams(args) / y;
		}

		#endregion
		#region Test Helpers

		private delegate void AssertTester(string expression, double actual);
		private delegate double Evaluator(string expression);

		private void DoTests(Evaluator eval)
		{
			AssertTester tester =
				(expr, expected) => Assert.AreEqual(expected, eval(expr));

			Trace.WriteLine("Static calls test...");
			StaticTests(tester);

			Trace.WriteLine("Instance calls test...");
			InstanceTests(tester);
		}

		private void StaticTests(AssertTester test)
		{
			// with no arguments
			test("1 + Stat0()",
				  1 + Stat0());

			// with one argument
			test("1 / Stat1(x)",
				  1 / Stat1(x));

			// with two arguments
			test("2 * Stat2(x,3)",
				  2 * Stat2(x,3));

			// all together
			test("Stat2(Stat1(-x), Stat0())",
				  Stat2(Stat1(-x), Stat0()));

			// empty params array
			test("7*x + SParams()",
				  7*x + SParams());

			// filled params array
			test("SParams(1,x) * SParams(4,x,6)",
				  SParams(1,x) * SParams(4,x,6));

			// static call with normal and params args:
			test("SParams2(1,x) * SParams2(x,5,6)",
				  SParams2(1,x) * SParams2(x,5,6));

			// all together:
			test("2 * SParams(1, x, Stat2(1, SParams2(6,x,7)), 3)",
				  2 * SParams(1, x, Stat2(1, SParams2(6,x,7)), 3));
		}

		private void InstanceTests(AssertTester test)
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
		#region Test Methods

		[TestMethod]
		public void QuickInterpretCallsTest()
		{
			DoTests(e => Calc.Evaluate(e, this.x));
		}

		[TestMethod]
		public void InterpretCallsTest()
		{
			DoTests(e => Calc.CreateInterpret(e).Evaluate(this.x));
		}

		[TestMethod]
		public void EvaluatorCallsTest()
		{
			DoTests(e => Calc.CreateEvaluator(e).Evaluate(this.x));
		}

		#endregion
	}
}
