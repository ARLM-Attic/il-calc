using System;

namespace ILCalc
	{
	/// <summary>
	/// Provides enumerated values to use to set expression optimizer options.
	/// Expression optimizer will be used by <see cref="CalcContext"/>
	/// when creating objects for evaluating expressions.
	/// </summary>
	[Flags]
	[Serializable]
	public enum OptimizeModes
		{
		/// <summary>
		/// Specifies that no optimizations are should be done.
		/// </summary>
		None = 0,

		/// <summary>
		/// Constant folding optimization should be done for the expression.
		/// It is used to perform partical evaluation of
		/// operators in parse-time when operands are known values:
		/// <c>2 + 8 / 4</c> will be replaced by <c>4</c>.
		/// </summary>
		ConstantFolding = 1 << 0,

		/// <summary>
		/// Function folding optimization should be done for the expression.
		/// It is used for invoking function in parse-time when all the
		/// arguments of function are known values: <c>sin(pi / 6)</c>
		/// will be replaced with <c>0.5</c>.<br/>
		/// <i>WARNING: Functions should not produce any side-effects
		/// or you may get an unexpected result.</i>
		/// </summary>
		FunctionFolding = 1 << 1,

		/// <summary>
		/// Power operator optimization should be done for the expression.
		/// It is used for replacing expressions like <c>x ^ 4</c> with 
		/// <c>x * x * x * x</c> that evaluates much faster.
		/// </summary>
		PowOptimize = 1 << 2,

		/// <summary>
		/// Specifies that all of optimizations are should be done.
		/// </summary>
		PerformAll = 0x0007
		}
	}
