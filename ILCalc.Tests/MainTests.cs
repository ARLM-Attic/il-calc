using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
	[TestClass]
	[Serializable]
	public sealed class MainTests
	{
		#region Initialize

		[NonSerialized]
		private readonly CalcContext calc;
		private readonly double x;

		public MainTests()
		{
			this.calc = new CalcContext("x");
			this.x = new Random().NextDouble();

			Calc.Culture = CultureInfo.CurrentCulture;

			Calc.Constants.Add("pi", Math.PI);
			Calc.Constants.Add("e", Math.E);
			Calc.Constants.Add("fi", 1.234);

			Calc.Functions.ImportBuiltIn();
			Calc.Functions.Import("Params", typeof(MainTests));
			Calc.Functions.Import("Params2", typeof(MainTests));
			Calc.Functions.Import("Params3", typeof(MainTests));

			Calc.Functions.Add(Inst0);
			Calc.Functions.Add(Inst1);
			Calc.Functions.Add(Inst2);
			Calc.Functions.Add(InstP);
		}

		private CalcContext Calc
		{
			get { return this.calc; }
		}

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

		public static double Params3(double a, double b, params double[] args)
		{
			return a + b;
		}

		private double Inst0()
		{
			return this.x;
		}

		private double Inst1(double arg)
		{
			return this.x + arg;
		}

		private double Inst2(double arg1, double arg2)
		{
			return this.x + arg1 / arg2;
		}

		private double InstP(params double[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			double res = this.x;
			foreach (double d in args) res += d;

			return res;
		}

		// ReSharper restore UnusedMember.Global

		#endregion
		#region SyntaxTest

		[TestMethod]
		public void SyntaxTest()
		{
			// numbers:
			TestErr("(2+2)2+3", 4, 2);
			TestErr("(2+2 2+3", 3, 3);
			TestErr("(2+pi2+3", 3, 3);
			TestGood("123+(23+4)");
			TestGood("2+4-5");
			TestGood("max(1;2)");

			// operators:
			TestGood("(1+1)*2");
			TestGood("1+1*2");
			TestGood("pi+2");

			TestErr("+12", 0, 1);
			TestErr("2**3", 1, 2);
			TestErr("max(1;*5)", 5, 2);

			TestGood("-2+Max(-1;-2)");
			TestGood("2*(-32)");
			TestGood("2*-3 + 2/-6 + 2^-3");
			TestGood("--2+ 3---4 + 5+-3");

			// separator:
			TestErr(";", 0, 1);
			TestErr("Max(2+;3)", 5, 2);
			TestErr("Max(2;;3)", 5, 2);
			TestErr("Max(;2)", 3, 2);
			TestGood("Max(1;3)");
			TestGood("Max(0;-1)");
			TestGood("Max(0;Max(1;3))");

			// brace open:
			TestGood("(2+2)(3+3)");
			TestGood("3(3+3)");
			TestGood("pi(3+3)");
			TestGood("pi+(3+3)+Max(12;(34))");

			// brace close:
			TestErr("(2+)", 2, 2);
			TestErr("3+()", 2, 2);
			TestErr("Max(1;)", 5, 2);
			TestGood("(2+2)");
			TestGood("(2+pi)");
			TestGood("(2+(3))");

			// identifiers:
			TestErr("pi pi", 0, 5);
			TestGood("(2+2)pi");
			TestGood("3pi");
			TestGood("Max(pi;pi)");
			TestGood("2+pi+3");

			// brace disbalance:
			TestErr("(3+(2+3)+3))+3", 11, 1);
			TestErr("((3+(2+3)+3)+3", 0, 1);
		}

		#endregion
		#region EvaluationTest

		[TestMethod]
		public void EvaluationTest()
		{
			var gen = new ExprGenerator(Calc);
			string now = string.Empty;

			foreach (var mode in Optimizer.Modes)
			{
				Calc.Optimization = mode;
				foreach (string expr in gen.Generate(5000))
				{
					double eval, int1, int2;
					try
					{
						now = "Evaluator";
						eval = Calc.CreateEvaluator(expr).Evaluate(1.0);
						now = "Interpret";
						int1 = Calc.CreateInterpret(expr).Evaluate(1.0);
						now = "Quick Interpret";
						int2 = Calc.Evaluate(expr, 1.0);
					}
					catch (Exception)
					{
						Trace.WriteLine(now);
						Trace.WriteLine(expr);
						throw;
					}

					if (double.IsNaN(eval)
					 || double.IsNaN(int1)
					 || double.IsNaN(int1))
					{
						continue;
					}

					if (eval == int1 && int1 == int2)
					{
						continue;
					}

					// Trace.WriteLine(expr, "=> ");
					// Trace.WriteLine(eval, "[1]");
					// Trace.WriteLine(int1, "[2]");
					// Trace.WriteLine(int2, "[3]");
					// Trace.WriteLine("");
				}
			}
		}

		#endregion
		#region IdentifiersTest

		[TestMethod]
		public void IdentifiersTest()
		{
			TestErr("2+sinus(2+2)", 2, 5);
			TestErr("2+dsdsd", 2, 5);

			// simple match:
			TestErr("1+Sin+3", 2, 3);
			TestErr("1+sin(1;2;3)", 2, 3);
			TestErr("1+Params()", 2, 6);

			// ambiguous match:
			Calc.Constants.Add("x", 123);
			Calc.Functions.AddStatic("SIN", typeof(Math).GetMethod("Sin"));
			Calc.Functions.AddStatic("sin", typeof(Math).GetMethod("Sin"));

			TestErr("2+x+3", 2, 1);
			TestErr("1-sIN+32", 2, 3);
			TestErr("7+sin(1;2;3)", 2, 3);
			TestErr("0+Sin(3)+4", 2, 3);

			Calc.Arguments[0] = "sin";
			TestGood("1+sin*4");

			Calc.Constants.Add("sin", 1.23);
			TestErr("1+sin/4", 2, 3);

			Calc.Constants.Remove("sin");
			Calc.Constants.Remove("x");
			Calc.Functions.Remove("SIN");
			Calc.Functions.Remove("sin");

			Calc.Functions.AddStatic(
				"max",
				typeof(Math).GetMethod("Max", new[] { typeof(double), typeof(double) }));

			Calc.Arguments[0] = "max";
			TestGood("2+max(3+3)");

			Calc.Constants.Add("max", double.MaxValue);
			TestErr("2+max(3+3)", 2, 3);

			Calc.Functions.AddStatic("maX", typeof(Math).GetMethod("Sin"));
			TestErr("2+max(3+3)", 2, 3);

			Calc.Constants.Remove("max");

			TestErr("1+max(1;2;3)+4", 2, 3);
			TestErr("2+max(1;2)/3", 2, 3);
			Calc.Functions.Remove("max", 2, false);
			TestErr("2+max(1;2)/3", 2, 3);

			// TODO: append MAX & max situations
		}

		#endregion
		#region OptimizerTest

		[TestMethod]
		public void OptimizerTest()
		{
			var gen = new ExprGenerator(Calc);

			foreach (string expr in gen.Generate(20000))
			{
				double	res1N, res1O,
						res2N, res2O;

				try
				{
					Calc.Optimization = OptimizeModes.None;
					
					res1N = Calc.CreateInterpret(expr).Evaluate(1.0);
					res2N = Calc.Evaluate(expr, 1.0);

					Calc.Optimization = OptimizeModes.PerformAll;

					res1O = Calc.CreateInterpret(expr).Evaluate(1.0);
					res2O = Calc.Evaluate(expr, 1.0);
				}
				catch
				{
					Trace.WriteLine(expr); throw;
				}

				if (res1N == res1O
				 && res2N == res2O) continue;

				if (double.IsNaN(res1N)
				 || double.IsNaN(res2N)) continue;

				Trace.WriteLine(expr);
				Trace.Indent();
				Trace.WriteLine(string.Format("Normal:    {0}", res2N));
				Trace.WriteLine(string.Format("Optimized: {0}", res2O));
				Trace.Unindent();
				Trace.WriteLine(string.Empty);
			}
		}

		#endregion
		#region SerializationTests

		[TestMethod]
		public void InterpretSerializeTest()
		{
			const int Count = 10000;
			
			var gen = new ExprGenerator(Calc);
			var list1 = new List<double>();
			var list2 = new List<double>();
			var binFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					FilterLevel = TypeFilterLevel.Low
				};

			using (var tempMem = new MemoryStream())
			{
				foreach (string expr in gen.Generate(Count))
				{
					Interpret a = Calc.CreateInterpret(expr);

					binFormatter.Serialize(tempMem, a);
					list1.Add(a.Evaluate(1.23));
				}

				tempMem.Position = 0;
				for (int i = 0; i < Count; i++)
				{
					var b = (Interpret) binFormatter.Deserialize(tempMem);
					list2.Add(b.Evaluate(1.23));
				}
			}

			CollectionAssert.AreEqual(list1, list2);
		}

		[TestMethod]
		public void ContextSerializeTest()
		{
			var binFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					FilterLevel = TypeFilterLevel.Low
				};

			using (var tempMem = new MemoryStream())
			{
				var range1 = new TabRange(1, 200, 1.50);
				var exception1 = new SyntaxException("hehe");
				var exception2 = new InvalidRangeException("wtf?");

				binFormatter.Serialize(tempMem, Calc);
				binFormatter.Serialize(tempMem, range1);
				binFormatter.Serialize(tempMem, exception1);
				binFormatter.Serialize(tempMem, exception2);

				tempMem.Position = 0;

				var other = (CalcContext) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(Calc.Arguments.Count, other.Arguments.Count);
				Assert.AreEqual(Calc.Constants.Count, other.Constants.Count);
				Assert.AreEqual(Calc.Functions.Count, other.Functions.Count);
				Assert.AreEqual(Calc.OverflowCheck, other.OverflowCheck);
				Assert.AreEqual(Calc.Optimization, other.Optimization);
				Assert.AreEqual(Calc.IgnoreCase, other.IgnoreCase);
				Assert.AreEqual(Calc.Culture, other.Culture);

				var range2 = (TabRange) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(range1, range2);

				var exception1D = (SyntaxException) binFormatter.Deserialize(tempMem);
				var exception2D = (InvalidRangeException) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(exception1.Message, exception1D.Message);
				Assert.AreEqual(exception2.Message, exception2D.Message);
			}
		}

		#endregion
		#region Import Test

		[TestMethod]
		public void ImportTest()
		{
			var c = new CalcContext();

			c.Constants.Import(typeof(double));
			Assert.AreEqual(c.Constants.Count, 6);

			c.Constants.Clear();
			c.Constants.ImportBuiltIn();
			Assert.AreEqual(c.Constants.Count, 4);

			c.Constants.Clear();
			c.Constants.Import(typeof(ClassForImport));
			Assert.AreEqual(c.Constants.Count, 1);

			c.Constants.Clear();
			c.Constants.Import(typeof(ClassForImport), true);
			Assert.AreEqual(c.Constants.Count, 3);

			c.Constants.Clear();
			c.Constants.Import(typeof(ClassForImport), typeof(Math), typeof(double));
			Assert.AreEqual(c.Constants.Count, 9);

			c.Functions.ImportBuiltIn();
			Assert.AreEqual(c.Functions.Count, 22);

			c.Functions.Clear();
			c.Functions.Import(typeof(Math));
			Assert.AreEqual(c.Functions.Count, 23);

			c.Functions.Clear();
			c.Functions.Import(typeof(ClassForImport));
			Assert.AreEqual(c.Functions.Count, 6);

			c.Functions.Clear();
			c.Functions.Import(typeof(ClassForImport), true);
			Assert.AreEqual(c.Functions.Count, 7);

			c.Functions.Clear();
			c.Functions.Import(typeof(ClassForImport), typeof(Math));
			Assert.AreEqual(c.Functions.Count, 29);

			// delegates
			c.Functions.Add("f1", ClassForImport.ParamsMethod1);
			c.Functions.Add("f2", ClassForImport.JustFunc);
			c.Functions.Add("f3", ClassForImport.StaticMethod);
			c.Functions.Add("f4", ClassForImport.StaticMethod1);
		}

		#endregion
		#region Helpers

		private void TestErr(string expr, int pos, int len)
		{
			try
			{
				Calc.Validate(expr);
			}
			catch (SyntaxException e)
			{
				Assert.AreEqual(e.Position, pos);
				Assert.AreEqual(e.Length, len);
			}
		}

		private void TestGood(string expr)
		{
			Calc.Validate(expr);
		}

		// ReSharper disable UnusedMember.Local
		// ReSharper disable UnusedParameter.Local
		private class ClassForImport
		{
			#pragma warning disable 169

			public const double Test = 0.123;
			private const double Foo = 2323;
			private const double Bar = 434343;
			private static readonly double X = 5.55;

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

			private static double HiddenMethod(double a, double b, double c)
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

			public static double ParamsMethod3(double a, double b, double c, double[] args)
			{
				return 0;
			}
		}

		// ReSharper restore UnusedMember.Local
		// ReSharper restore UnusedParameter.Local
		#endregion
	}
}