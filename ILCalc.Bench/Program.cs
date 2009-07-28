using System;

namespace ILCalc.Bench
{
	static class Program
	{
		private static void Main()
		{
			Tester.BenchmarkCompleted += Console.WriteLine;

			var calc = new CalcContext("x", "y");
			calc.Functions.ImportBuiltIn();

			var eval = calc.CreateInterpret("10x + y");
			var range = new ValueRange(0, 100, 1);


			Tester.Run("hehe", 5000,
				() => {
					eval.Tabulate(range, range);
					return "hello";
				},
				() => {
					eval.Tabulate(range, range);
					return "bye";
				});

			if (!ShowMenu(
				Benchmarks.InitializeTest,
				Benchmarks.EvaluateOnceTest,
				Benchmarks.ParseAndCompileTest,
				Benchmarks.ManyEvaluationsTest,
				Benchmarks.ILCalcOptimizerTest,
				Benchmarks.TabulationTest
				)) return;
		}

		#region Main Menu

		private delegate void Action();

		private static bool ShowMenu(params Action[] items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			int i = 1;
			foreach (Action item in items)
			{
				Console.WriteLine(
					"{0}. {1}", i++,
					item.Method.Name);
			}

			Console.WriteLine();
			Console.WriteLine("0. Exit");

			while(true)
			{
				Console.Write("\nAnswer: ");
				string ans = Console.ReadLine();

				if (string.IsNullOrEmpty(ans)) continue;
				if (!int.TryParse(ans, out i)) continue;
				if (i < 0 || i >= items.Length + 1) continue;
				if (i == 0) return false;

				items[i-1]();
				return true;
			}
		}

		#endregion
	}
}