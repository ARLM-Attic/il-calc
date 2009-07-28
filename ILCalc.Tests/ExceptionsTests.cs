using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
	[TestClass]
	public sealed class ExceptionsTests
	{
		#region Evaluators Tests

		[TestMethod]
		public void EvaluatorExceptions()
		{
			var calc = new CalcContext("x", "y", "z");

			Evaluator eval = calc.CreateEvaluator("x+y+z");

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

			Evaluator eval2 = calc.CreateEvaluator("x");

			Throws<ArgumentException>(
				() => eval2.Evaluate(),
				() => eval2.Evaluate0(),
				() => eval2.Evaluate(1, 2),
				() => eval2.Evaluate2(1, 2));
		}

		[TestMethod]
		public void TabulatorExceptions()
		{
			var calc = new CalcContext("x", "y", "z");
			Tabulator tab = calc.CreateTabulator("x+y+z");

			Assert.AreEqual(tab.ToString(), "x+y+z");
			Assert.AreEqual(tab.RangesCount, 3);

			var r = new ValueRange(1, 10, 1);
			var arr1 = (double[])   Tabulator.Allocate(new[] { r });
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
				() => tab.Tabulate(new ValueRange[]{ }),
				() => tab.Tabulate(new[] { r }),
				() => tab.Tabulate(new[] { r, r }),
				() => tab.Tabulate(r, r, r, r),

				() => tab.TabulateToArray(arr1),
				() => tab.TabulateToArray(arr1, new ValueRange[]{ }),
				() => tab.TabulateToArray(arr1, new[]{ r }),
				() => tab.TabulateToArray(arr1, new[]{ r, r }),
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

			Tabulator tab2 = calc.CreateTabulator("x");

			tab2.Tabulate(0, 10, 1);
			tab2.TabulateToArray(arr1, 0, 1);

			Throws<ArgumentException>(
				() => tab2.Tabulate(r, r),
				() => tab2.BeginTabulate(r, r, null, null));

			Throws<ArgumentException>(
				() => tab2.Tabulate(r, r, r),
				() => tab2.BeginTabulate(new ValueRange[] { }, null, null),
				() => tab2.BeginTabulate(new[] { r, r }, null, null),
				() => tab2.BeginTabulate(new[] { r, r, r, r }, null, null),
				() => tab2.BeginTabulate(r, r, r, null, null));

			Throws<ArgumentNullException>(
				() => Tabulator.Allocate(null),
				() => tab.BeginTabulate(null, null, null),
				() => tab.EndTabulate(null));

			var async = tab2.BeginTabulate(r, null, null);
			tab2.EndTabulate(async);

			Throws<InvalidOperationException>(
				() => tab2.EndTabulate(async));
		}

		[TestMethod]
		public void InterpretExceptions()
		{
			var calc = new CalcContext("x", "y", "z");

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

			Interpret inter2 = calc.CreateInterpret("x");

			Throws<ArgumentException>(
				() => inter2.Evaluate(),
				() => inter2.Evaluate(1, 2));
		}

		[TestMethod]
		public void TabulatorTabExceptions()
		{
			var calc = new CalcContext("x", "y", "z");
			Interpret tab = calc.CreateInterpret("x+y+z");

			Assert.AreEqual(tab.ToString(), "x+y+z");
			Assert.AreEqual(tab.ArgumentsCount, 3);

			var r = new ValueRange(1, 10, 1);
			var arr1 = (double[])   Tabulator.Allocate(new[] { r });
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
				() => tab.Tabulate(new ValueRange[]{ }),
				() => tab.Tabulate(new[] { r }),
				() => tab.Tabulate(new[] { r, r }),
				() => tab.Tabulate(r, r, r, r),

				() => tab.TabulateToArray(arr1),
				() => tab.TabulateToArray(arr1, new ValueRange[]{ }),
				() => tab.TabulateToArray(arr1, new[]{ r }),
				() => tab.TabulateToArray(arr1, new[]{ r, r }),
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

			Interpret tab2 = calc.CreateInterpret("x");

			tab2.Tabulate(0, 10, 1);
			tab2.TabulateToArray(arr1, 0, 1);

			Throws<ArgumentException>(
				() => tab2.Tabulate(r, r),
				() => tab2.BeginTabulate(r, r, null, null));

			Throws<ArgumentException>(
				() => tab2.Tabulate(r, r, r),
				() => tab2.BeginTabulate(new ValueRange[] { }, null, null),
				() => tab2.BeginTabulate(new[] { r, r }, null, null),
				() => tab2.BeginTabulate(new[] { r, r, r, r }, null, null),
				() => tab2.BeginTabulate(r, r, r, null, null));

			Throws<ArgumentNullException>(
				() => Tabulator.Allocate(null),
				() => tab.BeginTabulate(null, null, null),
				() => tab.EndTabulate(null));

			var async = tab2.BeginTabulate(r, null, null);
			tab2.EndTabulate(async);

			Throws<InvalidOperationException>(
				() => tab2.EndTabulate(async));
		}

		#endregion

		[TestMethod]
		public void ImportExceptionsTest()
		{
			var collection = new FunctionCollection();

			var method = new DynamicMethod(
				"test", typeof(double), Type.EmptyTypes);

			var il = method.GetILGenerator();
			il.Emit(OpCodes.Ldc_R8, 1.0);
			il.Emit(OpCodes.Ret);

			var func = (EvalFunc0)
				method.CreateDelegate(typeof(EvalFunc0));

			var func1 = typeof(ExceptionsTests).GetMethod("Func1");

			Throws<ArgumentException>(
				() => collection.AddStatic(func.Method));
				//() => collection.AddInstance(func1, this)); // TODO: fix it!
		}

		public static double Func1() { return 0; }

		#region Helpers

		private delegate void Action();

		private static void Throws<TException>(Action action)
			where TException : Exception
		{
			try { action(); }
			catch(TException e)
			{
				var buf = new StringBuilder();

				buf.Append('\'');
				buf.Append(e.Message);
				buf.Append("'.");

				Trace.WriteLine(buf.ToString(), typeof(TException).Name);
				return;
			}

			throw new InternalTestFailureException(
				typeof(TException).Name + " doesn't thrown!");
		}

		private static void Throws<TException>(params Action[] actions)
			where TException : Exception
		{
			if (actions == null)
				throw new ArgumentNullException("actions");

			foreach(Action action in actions)
			{
				try { action(); }
				catch (TException e)
				{
					var buf = new StringBuilder();

					buf.Append('\'');
					buf.Append(e.Message);
					buf.Append("'.");

					Trace.WriteLine(buf.ToString(), typeof(TException).Name);
					continue;
				}

				throw new InternalTestFailureException(
					typeof(TException).Name + " doesn't thrown!");
			}
		}

		#endregion
	}
}
