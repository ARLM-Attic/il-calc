using System;
using System.Diagnostics;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	//TODO: Owner or Target property

	#region Delegates

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

	#endregion

	/// <summary>
	/// Represents the object for the compiled expression evaluation.<br/>
	/// Instance of this class can be get from the <see cref="CalcContext.CreateEvaluator"/>
	/// method.<br/>This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// Instance contains read-only fields with delegates for slightly
	/// more performance invokation of the compiled methods.<br/>
	/// This class absolutely atomic from parent <see cref="CalcContext"/> class.<br/>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	/// <threadsafety instance="true"/>
	
	[DebuggerDisplay("{ToString()} ({ArgumentsCount} argument(s))")]
	public sealed class Evaluator : IEvaluator
		{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly string _exprText;
		[DebuggerBrowsable(State.Never)]
		private readonly int _argCount;

		/// <summary>
		/// Directly invokes the compiled expression with giving no arguments.
		/// This field is readonly.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public readonly EvalFunc0 Evaluate0;

		/// <summary>
		/// Directly invokes the compiled expression with giving one argument.
		/// This field is readonly.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public readonly EvalFunc1 Evaluate1;

		/// <summary>
		/// Directly invokes the compiled expression with giving two arguments.
		/// This field is readonly.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public readonly EvalFunc2 Evaluate2;

		/// <summary>
		/// Directly invokes the compiled expression with giving three or more arguments.
		/// This field is readonly.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public readonly EvalFuncN EvaluateN;
		
		#endregion
		#region Properties

		/// <summary>
		/// Returns the expression string, that this Evaluator represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			return _exprText;
			}

		/// <summary>
		/// Gets the arguments count, that this Evaluator implemented for.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public int ArgumentsCount
			{
			[DebuggerHidden]
			get { return _argCount; }
			} 
		
		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the compiled expression with giving no arguments.
		/// </summary>
		/// <overloads>Invokes the compiled expression.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Evaluator"/> with no arguments is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( )
			{
			return Evaluate0();
			}

		/// <summary>
		/// Invokes the compiled expression with giving one argument.
		/// </summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Evaluator"/> with one argument is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg )
			{
			return Evaluate1(arg);
			}

		/// <summary>
		/// Invokes the compiled expression with giving two arguments.
		/// </summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Evaluator"/> with two arguments is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg1, double arg2 )
			{
			return Evaluate2(arg1, arg2);
			}
		
		/// <summary>
		/// Invokes the compiled expression with giving three or more arguments.
		/// </summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Evaluator"/> with three or more arguments is not compiled.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="args"/> doesn't specify needed 
		/// <see cref="ArgumentsCount">arguments count</see>.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( params double[] args )
			{
			if(args.Length != _argCount)
				{
				throw new ArgumentException(
					string.Format(Resources.errWrongArgsCount,
								  args.Length, _argCount)
					);
				}

			return EvaluateN(args);
			}

		#endregion
		#region Constructor

		internal Evaluator( string expr, Delegate method, int args )
			{
			Evaluate0 = (args == 0) ? (EvalFunc0) method : ThrowFunc0;
			Evaluate1 = (args == 1) ? (EvalFunc1) method : ThrowFunc1;
			Evaluate2 = (args == 2) ? (EvalFunc2) method : ThrowFunc2;
			EvaluateN = (args >= 3) ? (EvalFuncN) method : ThrowFuncN;
		  
			_exprText = expr;
			_argCount = args;
			}

		#endregion
		#region Throw Functions

		[DebuggerHidden]
		private double ThrowFunc0( )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 0, _argCount)
				);
			}

		[DebuggerHidden]
		private double ThrowFunc1( double arg )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 1, _argCount)
				);
			}

		[DebuggerHidden]
		private double ThrowFunc2( double arg1, double arg2 )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 2, _argCount)
				);
			}

		[DebuggerHidden]
		private double ThrowFuncN( double[] args )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount,
							  args.Length, _argCount)
				);
			}

		#endregion
		}
	}