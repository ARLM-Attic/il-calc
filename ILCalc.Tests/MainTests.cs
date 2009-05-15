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
	public class MainTests
	{
		#region Initialize

		private readonly CalcContext calc;

		public MainTests()
		{
			this.calc = new CalcContext("x");

			this.calc.Culture = CultureInfo.CurrentCulture;

			this.calc.Constants.Add("pi", Math.PI);
			this.calc.Constants.Add("e", Math.E);
			this.calc.Constants.Add("fi", 1.234);

			this.calc.Functions.ImportBuiltIn();
			this.calc.Functions.Add("Params", typeof(MainTests));
			this.calc.Functions.Add("Params2", typeof(MainTests));
			this.calc.Functions.Add("Params3", typeof(MainTests));
		}

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

		#endregion
		#region SyntaxTest

		[TestMethod]
		public void SyntaxTest()
		{
			// numbers:
			this.TestErr("(2+2)2+3", 4, 2);
			this.TestErr("(2+2 2+3", 3, 3);
			this.TestErr("(2+pi2+3", 3, 3);
			this.TestGood("123+(23+4)");
			this.TestGood("2+4-5");
			this.TestGood("max(1;2)");

			// operators:
			this.TestGood("(1+1)*2");
			this.TestGood("1+1*2");
			this.TestGood("pi+2");

			this.TestErr("+12", 0, 1);
			this.TestErr("2**3", 1, 2);
			this.TestErr("max(1;*5)", 5, 2);

			this.TestGood("-2+Max(-1;-2)");
			this.TestGood("2*(-32)");
			this.TestGood("2*-3 + 2/-6 + 2^-3");
			this.TestGood("--2+ 3---4 + 5+-3");

			// separator:
			this.TestErr(";", 0, 1);
			this.TestErr("Max(2+;3)", 5, 2);
			this.TestErr("Max(2;;3)", 5, 2);
			this.TestErr("Max(;2)", 3, 2);
			this.TestGood("Max(1;3)");
			this.TestGood("Max(0;-1)");
			this.TestGood("Max(0;Max(1;3))");

			// brace open:
			this.TestGood("(2+2)(3+3)");
			this.TestGood("3(3+3)");
			this.TestGood("pi(3+3)");
			this.TestGood("pi+(3+3)+Max(12;(34))");

			// brace close:
			this.TestErr("(2+)", 2, 2);
			this.TestErr("3+()", 2, 2);
			this.TestErr("Max(1;)", 5, 2);
			this.TestGood("(2+2)");
			this.TestGood("(2+pi)");
			this.TestGood("(2+(3))");

			// identifiers:
			this.TestErr("pi pi", 0, 5);
			this.TestGood("(2+2)pi");
			this.TestGood("3pi");
			this.TestGood("Max(pi;pi)");
			this.TestGood("2+pi+3");

			// brace disbalance:
			this.TestErr("(3+(2+3)+3))+3", 11, 1);
			this.TestErr("((3+(2+3)+3)+3", 0, 1);
		}

		#endregion
		#region EvaluationTest

		[TestMethod]
		public void EvaluationTest()
		{
			var gen = new ExprGenerator(this.calc);
			string now = string.Empty;

			foreach (var mode in Optimizer.Modes)
			{
				this.calc.Optimization = mode;
				foreach (string expr in gen.Generate(5000))
				{
					double eval, int1, int2;
					try
					{
						now = "Evaluator";
						eval = this.calc.CreateEvaluator(expr).Evaluate(1.0);
						now = "Interpret";
						int1 = this.calc.CreateInterpret(expr).Evaluate(1.0);
						now = "Quick Interpret";
						int2 = this.calc.Evaluate(expr, 1.0);
					}
					catch (Exception e)
					{
						Trace.WriteLine(now);
						Trace.WriteLine(expr);
						throw e;
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
			this.TestErr("2+sinus(2+2)", 2, 5);
			this.TestErr("2+dsdsd", 2, 5);

			// simple match:
			this.TestErr("1+Sin+3", 2, 3);
			this.TestErr("1+sin(1;2;3)", 2, 3);
			this.TestErr("1+Params()", 2, 6);

			// ambiguous match:
			this.calc.Constants.Add("x", 123);
			this.calc.Functions.Add("SIN", typeof(Math).GetMethod("Sin"));
			this.calc.Functions.Add("sin", typeof(Math).GetMethod("Sin"));

			this.TestErr("2+x+3", 2, 1);
			this.TestErr("1-sIN+32", 2, 3);
			this.TestErr("7+sin(1;2;3)", 2, 3);
			this.TestErr("0+Sin(3)+4", 2, 3);

			this.calc.Arguments[0] = "sin";
			this.TestGood("1+sin*4");

			this.calc.Constants.Add("sin", 1.23);
			this.TestErr("1+sin/4", 2, 3);

			this.calc.Constants.Remove("sin");
			this.calc.Constants.Remove("x");
			this.calc.Functions.Remove("SIN");
			this.calc.Functions.Remove("sin");

			this.calc.Functions.Add(
				"max",
				typeof(Math).GetMethod("Max", new[] { typeof(double), typeof(double) }));

			this.calc.Arguments[0] = "max";
			this.TestGood("2+max(3+3)");

			this.calc.Constants.Add("max", double.MaxValue);
			this.TestErr("2+max(3+3)", 2, 3);

			this.calc.Functions.Add("maX", typeof(Math).GetMethod("Sin"));
			this.TestErr("2+max(3+3)", 2, 3);

			this.calc.Constants.Remove("max");

			this.TestErr("1+max(1;2;3)+4", 2, 3);
			this.TestErr("2+max(1;2)/3", 2, 3);
			this.calc.Functions.Remove("max", 2, false);
			this.TestErr("2+max(1;2)/3", 2, 3);

			// TODO: append MAX & max situations
		}

		#endregion
		#region OptimizerTest

		[TestMethod]
		public void OptimizerTest()
		{
			var gen = new ExprGenerator(this.calc);

			foreach (string expr in gen.Generate(20000))
			{
				double	res1N, res1O,
						res2N, res2O;

				try
				{
					this.calc.Optimization = OptimizeModes.None;
					
					res1N = this.calc.CreateInterpret(expr).Evaluate(1.0);
					res2N = this.calc.Evaluate(expr, 1.0);

					this.calc.Optimization = OptimizeModes.PerformAll;

					res1O = this.calc.CreateInterpret(expr).Evaluate(1.0);
					res2O = this.calc.Evaluate(expr, 1.0);
				}
				catch
				{
					Trace.WriteLine(expr);
					throw;
				}

				if (res1N == res1O
				 && res2N == res2O)
				{
					continue;
				}

				if (double.IsNaN(res1N)
				 || double.IsNaN(res2N))
				{
					continue;
				}

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
			
			var gen = new ExprGenerator(this.calc);
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
					Interpret a = this.calc.CreateInterpret(expr);

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

				binFormatter.Serialize(tempMem, this.calc);
				binFormatter.Serialize(tempMem, range1);
				binFormatter.Serialize(tempMem, exception1);
				binFormatter.Serialize(tempMem, exception2);

				tempMem.Position = 0;

				var other = (CalcContext) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(this.calc.Arguments.Count, other.Arguments.Count);
				Assert.AreEqual(this.calc.Constants.Count, other.Constants.Count);
				Assert.AreEqual(this.calc.Functions.Count, other.Functions.Count);
				Assert.AreEqual(this.calc.OverflowCheck, other.OverflowCheck);
				Assert.AreEqual(this.calc.Optimization, other.Optimization);
				Assert.AreEqual(this.calc.IgnoreCase, other.IgnoreCase);
				Assert.AreEqual(this.calc.Culture, this.calc.Culture);

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
				this.calc.Validate(expr);
			}
			catch (SyntaxException e)
			{
				Assert.AreEqual(e.Position, pos);
				Assert.AreEqual(e.Length, len);
			}
		}

		private void TestGood(string expr)
		{
			this.calc.Validate(expr);
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