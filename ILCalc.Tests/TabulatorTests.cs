using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ILCalc.Tests
{
	[TestClass]
	public sealed class TabulatorTests
	{
		#region Initialize

		private static readonly Random Random = new Random();

		#endregion
		#region Helpers

		private delegate double EvalFunc3(
			double arg1,
			double arg2, double arg3);

		private delegate double EvalFunc4(
			double arg1, double arg2,
			double arg3, double arg4);

		private static double[] Tabulate1D(
			EvalFunc1 func, TabRange range)
		{
			var array = new double[range.Count];
			double x = range.Begin;

			for(int i = 0; i < array.Length; i++)
			{
				array[i] = func(x);
				x += range.Step;
			}

			return array;
		}

		private static double[][] Tabulate2D(
			EvalFunc2 func, TabRange r1, TabRange r2)
		{
			var array = new double[r1.Count][];
			double x = r1.Begin;
			for (int i = 0; i < array.Length; i++)
			{
				var row = new double[r2.Count];
				double y = r2.Begin;
				for (int j = 0; j < row.Length; j++)
				{
					row[j] = func(x, y);
					y += r2.Step;
				}

				array[i] = row;
				x += r1.Step;
			}

			return array;
		}

		private static double[][][] Tabulate3D(
			EvalFunc3 func, TabRange r1, TabRange r2, TabRange r3)
		{
			var array3D = new double[r1.Count][][];
			double x = r1.Begin;
			for (int i = 0; i < array3D.Length; i++)
			{
				var array = new double[r2.Count][];
				double y = r2.Begin;
				for (int j = 0; j < array.Length; j++)
				{
					var row = new double[r3.Count];
					double z = r3.Begin;
					for (int k = 0; k < row.Length; k++)
					{
						row[k] = func(x, y, z);
						z += r3.Step;
					}

					array[j] = row;
					y += r2.Step;
				}

				array3D[i] = array;
				x += r1.Step;
			}

			return array3D;
		}

		private static double[][][][] Tabulate4D(
			EvalFunc4 func,
			TabRange r1, TabRange r2, 
			TabRange r3, TabRange r4)
		{
			var array4D = new double[r1.Count][][][];
			double x = r1.Begin;
			for (int i = 0; i < array4D.Length; i++)
			{
				var array3D = new double[r2.Count][][];
				double y = r2.Begin;
				for (int j = 0; j < array3D.Length; j++)
				{
					var array = new double[r3.Count][];
					double z = r3.Begin;
					for (int k = 0; k < array.Length; k++)
					{
						var row = new double[r4.Count];
						double w = r4.Begin;
						for (int g = 0; g < row.Length; g++)
						{
							row[g] = func(x, y, z, w);
							w += r4.Step;
						}

						array[k] = row;
						z += r3.Step;
					}

					array3D[j] = array;
					y += r2.Step;
				}

				x += r1.Step;
				array4D[i] = array3D;
			}

			return array4D;
		}

		private static void AssertEquality(
			double[] ex, double[] a1,
			double[] a2, double[] a3)
		{
			CollectionAssert.AreEqual(ex, a1);
			CollectionAssert.AreEqual(ex, a2);
			CollectionAssert.AreEqual(ex, a3);
		}

		private static void AssertEquality(
			double[][] ex, double[][] a1,
			double[][] a2, double[][] a3)
		{
			Assert.AreEqual(ex.Length, a1.Length);
			Assert.AreEqual(ex.Length, a2.Length);
			Assert.AreEqual(ex.Length, a3.Length);

			for(int i = 0; i < ex.Length; i++)
			{
				AssertEquality(ex[i], a1[i], a2[i], a3[i]);
			}
		}

		private static void AssertEquality(
			double[][][] ex, double[][][] a1,
			double[][][] a2, double[][][] a3)
		{
			Assert.AreEqual(ex.Length, a1.Length);
			Assert.AreEqual(ex.Length, a2.Length);
			Assert.AreEqual(ex.Length, a3.Length);

			for(int i = 0; i < ex.Length; i++)
			{
				AssertEquality(ex[i], a1[i], a2[i], a3[i]);
			}
		}

		private static TabRange GetRandomRange()
		{
			int from = Random.Next(-100, 100);
			int to   = Random.Next(50, 100) + from;
			int step = Random.Next(1, 4);

			return new TabRange(from, to, step);
		}

		#endregion

		[TestMethod]
		public void TabulateTest1D()
		{
			var calc = new CalcContext("x");
			calc.Functions.Add(Math.Sin);

			TabRange range = GetRandomRange();

			Tabulator tab = calc.CreateTabulator("2sin(x)");
			IAsyncResult async = tab.BeginTabulate(range, null, null);

			var expected = Tabulate1D(x => 2 * Math.Sin(x), range);

			var actual1 = tab.Tabulate(range);
			var actual2 = (double[]) tab.EndTabulate(async);
			var actual3 = Tabulator.Allocate(range);
			tab.TabulateToArray(actual3, range);

			AssertEquality(expected, actual1, actual2, actual3);
		}

		[TestMethod]
		public void TabulateTest2D()
		{
			var calc = new CalcContext("x", "y");
			calc.Functions.Add(Math.Sin);
			calc.Functions.Add(Math.Cos);

			TabRange rangeX = GetRandomRange();
			TabRange rangeY = GetRandomRange();

			Tabulator tab = calc.CreateTabulator("cos(x) * sin(y)");
			IAsyncResult async = tab.BeginTabulate(rangeX, rangeY, null, null);

			var expected = Tabulate2D(
				(x, y) => Math.Cos(x) * Math.Sin(y),
				rangeX, rangeY);

			var actual1 = tab.Tabulate(rangeX, rangeY);
			var actual2 = (double[][]) tab.EndTabulate(async);
			var actual3 = Tabulator.Allocate(rangeX, rangeY);
			tab.TabulateToArray(actual3, rangeX, rangeY);

			AssertEquality(expected, actual1, actual2, actual3);
		}

		[TestMethod]
		public void TabulateTest3D()
		{
			var calc = new CalcContext("x", "y", "z");
			calc.Functions.Add(Math.Sin);
			calc.Functions.Add(Math.Cos);
			calc.Functions.Add(Math.Tan);

			TabRange
				rangeX = GetRandomRange(),
				rangeY = GetRandomRange(),
				rangeZ = GetRandomRange();

			Tabulator tab = calc.CreateTabulator("cos(x) * sin(y) * tan(z)");
			IAsyncResult async = tab.BeginTabulate(
				rangeX, rangeY, rangeZ, null, null);

			var expected = Tabulate3D(
				(x, y, z) => Math.Cos(x) * Math.Sin(y) * Math.Tan(z),
				rangeX, rangeY, rangeZ);

			var actual1 = (double[][][]) tab.Tabulate(rangeX, rangeY, rangeZ);
			var actual2 = (double[][][]) tab.EndTabulate(async);
			var actual3 = (double[][][]) Tabulator.Allocate(rangeX, rangeY, rangeZ);
			tab.TabulateToArray(actual3, rangeX, rangeY, rangeZ);

			AssertEquality(expected, actual1, actual2, actual3);
		}

		[TestMethod]
		public void TabulateTest4D()
		{
			var calc = new CalcContext("x", "y", "z", "w");
			calc.Functions.Add(Math.Sin);
			calc.Functions.Add(Math.Cos);
			calc.Functions.Add(Math.Tan);

			TabRange rX = GetRandomRange(), rY = GetRandomRange(),
			         rZ = GetRandomRange(), rW = GetRandomRange();

			Tabulator tab = calc.CreateTabulator(
				"cos(x) * sin(y) * tan(z) * sin(w)");

			IAsyncResult async = tab.BeginTabulate(
				new[] { rX, rY, rZ, rW }, null, null);

			var expected = Tabulate4D(
				(x,y,z,w) => Math.Cos(x) * Math.Sin(y)
				           * Math.Tan(z) * Math.Sin(w),
				rX, rY, rZ, rW);

			var actual1 = (double[][][][]) tab.Tabulate(rX, rY, rZ, rW);
			var actual2 = (double[][][][]) tab.EndTabulate(async);
			var actual3 = (double[][][][]) Tabulator.Allocate(rX, rY, rZ, rW);

			tab.TabulateToArray(actual3, rX, rY, rZ, rW);

			Assert.AreEqual(expected.Length, actual1.Length);
			Assert.AreEqual(expected.Length, actual2.Length);
			Assert.AreEqual(expected.Length, actual3.Length);

			for(int i = 0; i < expected.Length; i++)
			{
				AssertEquality(expected[i], actual1[i], actual2[i], actual3[i]);
			}
		}
	}
}
