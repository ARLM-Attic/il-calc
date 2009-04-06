using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
	{
	[TestClass]
	public class CalcTest
		{
		#region Initialize

		private readonly CalcContext calc;

		public CalcTest( )
			{
			calc = new CalcContext("x");

			calc.Culture = CultureInfo.CurrentCulture;
			calc.Constants.ImportBuiltin( );
			calc.Functions.ImportBuiltin( );
			calc.Functions.Add(typeof(CalcTest), "Params");
			}

		public static double Params( double arg, params double[] args )
			{
			return 0;
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
			var gen = new Generator(calc);

			for(var n = 0; n < 10000; n++)
				{
				string expr = gen.Next();

				double eval = calc.CreateEvaluator(expr).Evaluate(1.0);
				double int1 = calc.CreateInterpret(expr).Evaluate(1.0);
				double int2 = calc.Evaluate(expr, 1.0);

				if(eval == int1 && int1 == int2) continue;

				if(	double.IsNaN(eval)
				||	double.IsNaN(int1)
				||	double.IsNaN(int1) ) continue;

				Debug.WriteLine(expr);
				Debug.Indent();
				Debug.WriteLine(string.Format("Evaluator: {0}", eval));
				Debug.WriteLine(string.Format("Interpret: {0}", int1));
				Debug.WriteLine(string.Format("Evaluate : {0}", int2));
				Debug.Unindent();
				Debug.WriteLine("");
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
			}

		#endregion
		#region OptimizerTest

		[TestMethod]
		public void OptimizerTest( )
			{
			var gen = new Generator(calc);

			for(var n = 0; n < 10000; n++)
				{
				string expr = gen.Next();

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
					calc.Optimization = OptimizeModes.PerformAll;
					//Debug.WriteLine(calc.PostfixForm(expr));
					Debug.WriteLine(expr);
					throw new Exception( );
					}

				if(	res1n == res1o
				&&	res2n == res2o ) continue;

				if(	double.IsNaN(res1n)
				||	double.IsNaN(res2n) ) continue;

				Debug.WriteLine(expr);
				Debug.Indent();
				Debug.WriteLine(string.Format("Normal:    {0}", res2n));
				Debug.WriteLine(string.Format("Optimized: {0}", res2o));
				Debug.Unindent();
				Debug.WriteLine("");
				}
			}

		#endregion
		
		//TODO: serialization tests
		}
	}