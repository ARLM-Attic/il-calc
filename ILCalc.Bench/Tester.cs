using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ILCalc.Bench
{
	public static class Tester
	{
		public delegate string Test();

		public static event Action<string> BenchmarkCompleted;

		public static void Run(
			string testName,
			int iterations,
			params Test[] tests)
		{
			if (tests == null)
				throw new ArgumentNullException("tests");

			var thread = Thread.CurrentThread;
			var oldPriority = thread.Priority;
			var timer = new Stopwatch();
			var buf = new StringBuilder();

			buf.Append("Benchmark \"");
			buf.Append(testName);
			buf.Append("\" (");
			buf.Append(iterations);
			buf.Append(" iterations) results:\n\n");

			try
			{
				thread.Priority = ThreadPriority.Highest;

				int n = 1;
				foreach (Test test in tests)
				{
					timer.Start();

					string name = null;
					for (int i = 0; i < iterations; i++)
					{
						name = test();
					}

					timer.Stop();

					buf.AppendFormat(
						"Test #{0} {1,16}: {2} ({3} ms)\n", n++,
						name,
						timer.Elapsed,
						timer.ElapsedMilliseconds);

					timer.Reset();
				}
			}
			finally
			{
				thread.Priority = oldPriority;
			}

			Action<string> completed = BenchmarkCompleted;
			if (completed != null)
			{
				completed(buf.ToString());
			}
		}
	}
}