using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
	{
	[TestClass]
	public class MultiThreadTests
		{
		#region Fields

		private readonly CalcContext calc;
		private readonly Interpret intr;
		private readonly Evaluator eval;
		private readonly Random rnd;

		private readonly Interpret syncInterp;
		private readonly object syncRoot;

		private const string expression = "(pi*x + 2sin(y)+x-y+2x)^2 + x^3";

		#endregion
		#region Initialize

		public MultiThreadTests( )
			{
			calc = new CalcContext("x", "y");

			calc.Functions.ImportBuiltIn( );
			calc.Constants.ImportBuiltIn( );
			calc.Functions.Add("Sum", typeof(MultiThreadTests));

			calc.Optimization = OptimizeModes.PerformAll;

			intr = calc.CreateInterpret(expression);
			eval = calc.CreateEvaluator(expression);

			syncInterp = calc.CreateInterpret(expression);
			syncRoot = new object( );

			rnd = new Random( );
			}

		public static double Sum( double one, double two, double[] args )
			{
			double sum = one + two;
			foreach( double arg in args ) sum += arg;
			return sum;
			}

		#endregion
		#region PerformanceTest

		//[TestMethod]
		public void SyncPerformanceTest( )
			{
			const int count = 1000000;

			var sw = Stopwatch.StartNew( );
			for( int i = 0; i < count; i++ ) eval.Evaluate(1, 2);

			Trace.WriteLine(sw.Elapsed, "eval version ");

			sw = Stopwatch.StartNew( );
			for( int i = 0; i < count; i++ ) intr.Evaluate(1, 2);

			Trace.WriteLine(sw.Elapsed, "norm version");

			sw = Stopwatch.StartNew( );
			for( int i = 0; i < count; i++ ) intr.EvaluateSync(1, 2);

			Trace.WriteLine(sw.Elapsed, "sync version ");
			}

		#endregion
		#region ThreadSafety Test

		[TestMethod]
		public void ThreadSafetyTest( )
			{
			const int count = 1000;

			for(int i = 0; i < count; i++)
				{
				new Thread(ThreadMethod).Start( );
				}
			}

		private void ThreadMethod( object state )
			{
			double arg1 = rnd.NextDouble( );
			double arg2 = rnd.NextDouble( );

			double res1 = intr.EvaluateSync(arg1, arg2);
			double res2 = eval.Evaluate(arg1, arg2);

			if( res1 == res2 ) return;

			lock( syncRoot )
				{
				Trace.WriteLine(string.Format("{0} != {1}", res2, res1));

				res1 = syncInterp.Evaluate(arg1, arg2);
				Trace.WriteLine(res1);
				Trace.WriteLine("");

				Assert.AreEqual(res1, res2);
				}
			}

		#endregion
		}
	}
