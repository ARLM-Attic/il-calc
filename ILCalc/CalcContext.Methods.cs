using System;

namespace ILCalc
{
	public sealed partial class CalcContext
	{
		/// <summary>
		/// Compiles the <see cref="Evaluator"/> object for evaluating
		/// the specified <paramref name="expression"/>.</summary>
		/// <param name="expression">Expression to compile.</param>
		/// <exception cref="SyntaxException"><paramref name="expression"/>
		/// contains syntax error(s) and can't be compiled.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		/// <remarks>Not available in the .NET CF / Silverlight versions.</remarks>
		/// <returns><see cref="Evaluator"/> object for evaluating expression.</returns>
		public Evaluator CreateEvaluator(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			var compiler = new EvaluatorCompiler(ArgsCount, OverflowCheck);
			ParseOptimized(expression, compiler);

			return compiler.CreateEvaluator(expression);
		}

		/// <summary>
		/// Compiles the <see cref="Tabulator"/> object for evaluating
		/// the specified <paramref name="expression"/> in some ranges of arguments.
		/// </summary>
		/// <param name="expression">Expression to compile.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s) and can't be compiled.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Current expression's arguments <see cref="Arguments">count</see>
		/// is not supported (only 1 or 2 arguments supported by now).</exception>
		/// <remarks>Not available in the .NET CF / Silverlight versions.</remarks>
		/// <returns><see cref="Tabulator"/> object for evaluating expression.</returns>
		public Tabulator CreateTabulator(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (ArgsCount == 0)
				throw new ArgumentException(Resource.errTabulatorWrongArgs);

			var compiler = new TabulatorCompiler(ArgsCount, OverflowCheck);
			ParseOptimized(expression, compiler);

			return compiler.CreateTabulator(expression);
		}

#if DEBUG

		public string PostfixForm(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			var postfix = new PostfixWriter(arguments);
			
			ParseOptimized(expression, postfix);

			return postfix.ToString();
		}

#endif
	}
}