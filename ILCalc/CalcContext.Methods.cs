using System;

namespace ILCalc
	{
	public sealed partial class CalcContext
		{
		/// <summary>
		/// Compiles the <see cref="Evaluator"/> object
		/// for evaluating the specified <paramref name="expression"/>.
		/// </summary>
		/// <param name="expression">Expression to compile.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s) and can't be compiled.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.
		/// </exception>
		/// <remarks>Not available in the .NET CF / Silverlight versions.</remarks>
		/// <returns><see cref="Evaluator"/> object for evaluating expression.</returns>
		public Evaluator CreateEvaluator( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			int argCount = (argsList != null) ? argsList.Count : 0;
			var compiler = new EvaluatorCompiler(argCount, checkedMode);

			if( parser == null )
				parser = new Parser(this);

			ExecuteParse(expression, compiler);

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
		public Tabulator CreateTabulator( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			if( argsList == null || argsList.Count < 1 || argsList.Count > 2 )
				{
				throw new ArgumentException(
					Resources.errTabulatorWrongArgs
					);
				}

			var compiler = new TabulatorCompiler((argsList.Count == 1), checkedMode);

			if( parser == null )
				parser = new Parser(this);

			ExecuteParse(expression, compiler);

			return compiler.CreateTabulator(expression);
			}

#if DEBUG

		public string PostfixForm( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			var postfix = new PostfixWriter(argsList);

			if( parser == null )
				parser = new Parser(this);

			ExecuteParse(expression, postfix);

			return postfix.ToString( );
			}

#endif
		}
	}
