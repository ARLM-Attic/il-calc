namespace ILCalc
	{
	/// <summary>
	/// Represents the compiled expression with no arguments.
	/// </summary>
	/// <returns>Evaluated value.</returns>
	public delegate double EvalFunc0( );

	/// <summary>
	/// Represents the compiled expression with one argument.
	/// </summary>
	/// <param name="arg">Expression argument.</param>
	/// <returns>Evaluated value.</returns>
	public delegate double EvalFunc1( double arg );

	/// <summary>
	/// Represents the compiled expression with two arguments.
	/// </summary>
	/// <param name="arg1">First expression argument.</param>
	/// <param name="arg2">Second expression argument.</param>
	/// <returns>Evaluated value.</returns>
	public delegate double EvalFunc2( double arg1, double arg2 );

	/// <summary>
	/// Represents the compiled expression with three or more arguments.
	/// </summary>
	/// <param name="args">Expression arguments.</param>
	/// <returns>Evaluated value.</returns>
	public delegate double EvalFuncN( params double[] args );
	}
