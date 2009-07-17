using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
	[TestClass]
	public sealed class ExceptionsTests
	{
		// TODO: think about diff exceptions in Evaluator/Tabulator

		[TestMethod]
		public void EvaluatorExceptions()
		{
			var calc = new CalcContext("x", "y", "z");

			Evaluator eval = calc.CreateEvaluator("x+y+z");

			Assert.AreEqual(eval.ToString(), "x+y+z");
			Assert.AreEqual(eval.ArgumentsCount, 3);

			Throws<InvalidOperationException>(
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

			Throws<InvalidOperationException>(
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

			TabRange r = new TabRange(1, 10, 1);
			var arr1 = (double[])   Tabulator.Allocate(new[] { r });
			var arr2 = (double[][]) Tabulator.Allocate(new[] { r, r });

			Throws<InvalidOperationException>(
				() => tab.Tabulate(r),
				() => tab.Tabulate(r, r),
				() => tab.TabulateToArray(arr1, r),
				() => tab.TabulateToArray(arr2, r, r),
				() => tab.BeginTabulate(r, null, null),
				() => tab.BeginTabulate(r, r, null, null));

			Throws<ArgumentException>(
				() => tab.Tabulate(),
				() => tab.Tabulate(new TabRange[]{ }),
				() => tab.Tabulate(new[] { r }),
				() => tab.Tabulate(new[] { r, r }),
				() => tab.Tabulate(r, r, r, r),

				() => tab.TabulateToArray(arr1),
				() => tab.TabulateToArray(arr1, new TabRange[]{ }),
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

			Throws<InvalidOperationException>(
				() => tab2.Tabulate(r, r),
				() => tab2.BeginTabulate(r, r, null, null));

			Throws<ArgumentException>(
				() => tab2.Tabulate(r, r, r),
				() => tab2.BeginTabulate(new TabRange[] { }, null, null),
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
