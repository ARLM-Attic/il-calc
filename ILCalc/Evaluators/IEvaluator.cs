namespace ILCalc
	{
	/// <summary>
	/// Represents the object for the expression evaluation.
	/// </summary>
	/// <seealso cref="Evaluator"/>
	/// <seealso cref="Interpret"/>
	public interface IEvaluator
		{
		/// <summary>
		/// Gets the arguments count, that this <see cref="IEvaluator"/> implemented for.
		/// </summary>
		int ArgumentsCount { get; }

		/// <summary>
		/// Invokes the expression evaluation with giving no arguments.
		/// </summary>
		/// <returns>Evaluated value.</returns>
		double Evaluate();

		/// <summary>
		/// Invokes the expression evaluation with giving one argument.
		/// </summary>
		/// <overloads>Invokes the expression evaluation.</overloads>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		double Evaluate(double arg);

		/// <summary>
		/// Invokes the expression evaluation with giving two arguments.
		/// </summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		double Evaluate(double arg1, double arg2);

		// TODO: fix summary?

		/// <summary>
		/// Invokes the expression evaluation with giving three or more arguments.
		/// </summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		double Evaluate(params double[] args);

		/// <summary>
		/// Returns the expression string, that this <see cref="IEvaluator"/> represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		string ToString();
		}
	}
