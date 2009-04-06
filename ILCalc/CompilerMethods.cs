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

			int argCount = (_args != null) ? _args.Count : 0;
			var compiler = new EvaluatorCompiler(argCount, _checked);

			if( _parser == null )
				_parser = new Parser(this);

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
		/// <paramref name="expression"/> is null.<br/>-or-<br/>
		/// <exception cref="ArgumentException">
		/// The espression's <see cref="Arguments"/> count is not supported (only one or two arguments allowed).</exception>
		/// </exception>
		/// <remarks>Not available in the .NET CF / Silverlight versions.</remarks>
		/// <returns><see cref="Tabulator"/> object for evaluating expression.</returns>
		public Tabulator CreateTabulator( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			if( _args == null || _args.Count < 1 || _args.Count > 2 )
				{
				throw new ArgumentException(
					Resources.errTabulatorWrongArgs
					);
				}

			var compiler = new TabulatorCompiler((_args.Count == 1), _checked);

			if( _parser == null )
				_parser = new Parser(this);

			ExecuteParse(expression, compiler);

			return compiler.CreateTabulator(expression);
			}
		}
	}
