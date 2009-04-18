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

		public MainTests( )
			{
			calc = new CalcContext("x");

			calc.Culture = CultureInfo.CurrentCulture;

			calc.Constants.Add("pi", Math.PI);
			calc.Constants.Add("e", Math.E);
			calc.Constants.Add("fi", 1.234);

			calc.Functions.ImportBuiltIn( );
			calc.Functions.Add("Params", typeof(MainTests));
			calc.Functions.Add("Params2", typeof(MainTests));
			calc.Functions.Add("Params3", typeof(MainTests));
			}

		public static double Params( double arg, params double[] args )
			{
			return arg + (args.Length > 0 ? args[0] : 0.0);
			}

		public static double Params2( params double[] args )
			{
			double avg = 0;
			foreach( double c in args ) avg += c;

			return avg / args.Length;
			}

		public static double Params3( double a, double b, params double[] args )
			{
			return a + b;
			}

		#endregion
		#region SyntaxTest

		[TestMethod]
		public void SyntaxTest( )
			{
			//numbers
			TestErr("(2+2)2+3", 4, 2);
			TestErr("(2+2 2+3", 3, 3);
			TestErr("(2+pi2+3", 3, 3);
			TestGood("123+(23+4)");
			TestGood("2+4-5");
			TestGood("max(1;2)");

			//operators
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

			//separator
			TestErr(";", 0, 1);
			TestErr("Max(2+;3)", 5, 2);
			TestErr("Max(2;;3)", 5, 2);
			TestErr("Max(;2)", 3, 2);
			TestGood("Max(1;3)");
			TestGood("Max(0;-1)");
			TestGood("Max(0;Max(1;3))");

			//brace open
			TestGood("(2+2)(3+3)");
			TestGood("3(3+3)");
			TestGood("pi(3+3)");
			TestGood("pi+(3+3)+Max(12;(34))");

			//brace close
			TestErr("(2+)", 2, 2);
			TestErr("3+()", 2, 2);
			TestErr("Max(1;)", 5, 2);
			TestGood("(2+2)");
			TestGood("(2+pi)");
			TestGood("(2+(3))");

			//identifiers
			TestErr("pi pi", 0, 5);
			TestGood("(2+2)pi");
			TestGood("3pi");
			TestGood("Max(pi;pi)");
			TestGood("2+pi+3");

			//brace disbalance
			TestErr("(3+(2+3)+3))+3", 11, 1);
			TestErr("((3+(2+3)+3)+3", 0, 1);
			}

		private void TestErr( string expr, int pos, int len )
			{
			try
				{
				calc.Validate(expr);
				}
			catch(SyntaxException e)
				{
				Assert.AreEqual(e.Position, pos);
				Assert.AreEqual(e.Length, len);
				}
			}

		private void TestGood( string expr )
			{
			calc.Validate(expr);
			}

		#endregion
		#region EvaluationTest

		[TestMethod]
		public void EvaluationTest( )
			{
			var gen = new ExprGenerator(calc);

			foreach( var mode in OptimizerModes.All( ))
				{
				calc.Optimization = mode;
				
				foreach( string expr in gen.Generate(5000) )
					{
					double eval = calc.CreateEvaluator(expr).Evaluate(1.0);
					double int1 = calc.CreateInterpret(expr).Evaluate(1.0);
					double int2 = calc.Evaluate(expr, 1.0);

					if( double.IsNaN(eval)
					 || double.IsNaN(int1)
					 || double.IsNaN(int1) )
						{
						continue;
						}

					if( eval == int1
					 && int1 == int2 ) continue;

					//Trace.WriteLine(expr, "=> ");
					//Trace.WriteLine(eval, "[1]");
					//Trace.WriteLine(int1, "[2]");
					//Trace.WriteLine(int2, "[3]");
					//Trace.WriteLine("");
					}
				}
			}

		#endregion
		#region IdentifiersTest

		[TestMethod]
		public void IdentifiersTest( )
			{
			TestErr("2+sinus(2+2)", 2, 5);
			TestErr("2+dsdsd", 2, 5);

			//simple match
			TestErr("1+Sin+3", 2, 3);
			TestErr("1+sin(1;2;3)", 2, 3);
			TestErr("1+Params()", 2, 6);

			//ambiguous match
			calc.Constants.Add("x", 123);
			calc.Functions.Add("SIN", typeof(Math).GetMethod("Sin"));
			calc.Functions.Add("sin", typeof(Math).GetMethod("Sin"));

			TestErr("2+x+3", 2, 1);
			TestErr("1-sIN+32", 2, 3);
			TestErr("7+sin(1;2;3)", 2, 3);
			TestErr("0+Sin(3)+4", 2, 3);

			calc.Arguments[0] = "sin";
			TestGood("1+sin*4");

			calc.Constants.Add("sin", 1.23);
			TestErr("1+sin/4", 2, 3);

			calc.Constants.Remove("sin");
			calc.Constants.Remove("x");
			calc.Functions.Remove("SIN");
			calc.Functions.Remove("sin");

			calc.Functions.Add("max",
				typeof(Math).GetMethod("Max", new[] { typeof(double), typeof(double) })
				);

			calc.Arguments[0] = "max";
			TestGood("2+max(3+3)");

			calc.Constants.Add("max", double.MaxValue);
			TestErr("2+max(3+3)", 2, 3);

			calc.Functions.Add("maX", typeof(Math).GetMethod("Sin"));
			TestErr("2+max(3+3)", 2, 3);

			calc.Constants.Remove("max");

			TestErr("1+max(1;2;3)+4", 2, 3);
			TestErr("2+max(1;2)/3", 2, 3);
			calc.Functions.Remove("max", 2, false);
			TestErr("2+max(1;2)/3", 2, 3);

			//TODO: append MAX & max situations
			}

		#endregion
		#region OptimizerTest

		[TestMethod]
		public void OptimizerTest( )
			{
			var gen = new ExprGenerator(calc);

			foreach( string expr in gen.Generate(20000) )
				{
				double	res1n, res1o,
						res2n, res2o;

				try
					{
					calc.Optimization = OptimizeModes.None;
					
					res1n = calc.CreateInterpret(expr).Evaluate(1.0);
					res2n = calc.Evaluate(expr, 1.0);

					calc.Optimization = OptimizeModes.PerformAll;

					res1o = calc.CreateInterpret(expr).Evaluate(1.0);
					res2o = calc.Evaluate(expr, 1.0);
					}
				catch
					{
					Trace.WriteLine(expr);
					throw;
					}

				if(	res1n == res1o
				&&	res2n == res2o ) continue;

				if(	double.IsNaN(res1n)
				||	double.IsNaN(res2n) ) continue;

				Trace.WriteLine(expr);
				Trace.Indent( );
				Trace.WriteLine(string.Format("Normal:    {0}", res2n));
				Trace.WriteLine(string.Format("Optimized: {0}", res2o));
				Trace.Unindent( );
				Trace.WriteLine("");
				}
			}

		#endregion
		#region SerializationTests

		[TestMethod]
		public void InterpretSerializeTest( )
			{
			var gen = new ExprGenerator(calc);
			var list1 = new List<double>( );
			var list2 = new List<double>( );
			var binFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					FilterLevel = TypeFilterLevel.Low
				};

			const int count = 10000;

			using( var tempMem = new MemoryStream( ) )
				{
				foreach( string expr in gen.Generate(count) )
					{
					Interpret A = calc.CreateInterpret(expr);

					binFormatter.Serialize(tempMem, A);
					list1.Add(A.Evaluate(1.23));
					}

				tempMem.Position = 0;
				for( int i = 0; i < count; i++ )
					{
					var B = (Interpret) binFormatter.Deserialize(tempMem);
					list2.Add(B.Evaluate(1.23));
					}
				}

			CollectionAssert.AreEqual(list1, list2);
			}

		[TestMethod]
		public void ContextSerializeTest( )
			{
			var binFormatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					FilterLevel = TypeFilterLevel.Low
				};

			using( var tempMem = new MemoryStream( ) )
				{
				var range1 = new TabRange(1, 200, 1.50);
				var exception1 = new SyntaxException("hehe");
				var exception2 = new InvalidRangeException("wtf?");

				binFormatter.Serialize(tempMem, calc);
				binFormatter.Serialize(tempMem, range1);
				binFormatter.Serialize(tempMem, exception1);
				binFormatter.Serialize(tempMem, exception2);

				tempMem.Position = 0;

				var deCalc = (CalcContext) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(calc.Arguments.Count, deCalc.Arguments.Count);
				Assert.AreEqual(calc.Constants.Count, deCalc.Constants.Count);
				Assert.AreEqual(calc.Functions.Count, deCalc.Functions.Count);
				Assert.AreEqual(calc.OverflowCheck, deCalc.OverflowCheck);
				Assert.AreEqual(calc.Optimization, deCalc.Optimization);
				Assert.AreEqual(calc.IgnoreCase, deCalc.IgnoreCase);
				Assert.AreEqual(calc.Culture, calc.Culture);

				var range2 = (TabRange) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(range1, range2);

				var exception1d = (SyntaxException) binFormatter.Deserialize(tempMem);
				var exception2d = (InvalidRangeException) binFormatter.Deserialize(tempMem);

				Assert.AreEqual(exception1.Message, exception1d.Message);
				Assert.AreEqual(exception2.Message, exception2d.Message);
				}
			}

		#endregion
		}
	}