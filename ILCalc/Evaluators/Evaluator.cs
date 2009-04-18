using System;
using System.Diagnostics;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	// TODO: Serialization!
	// bool IsSerializible { get; }
	// BufferWriter data;

	/// <summary>
	/// Represents the object for the compiled expression evaluation.<br/>
	/// Instance of this class can be get from the <see cref="CalcContext.CreateEvaluator"/>
	/// method.<br/>This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// Instance contains read-only fields with delegates for slightly
	/// more performance invokation of the compiled methods.<br/>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	/// <threadsafety instance="true"/>
	
	[DebuggerDisplay("{ToString()} ({ArgumentsCount} argument(s))")]

	public sealed class Evaluator : IEvaluator
		{
		#region Fields

		[DebuggerBrowsable(State.Never)] private readonly string exprString;
		[DebuggerBrowsable(State.Never)] private readonly int argsCount;

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

		// NOTE: fix summary if impl redirect
		/// <summary>
		/// Directly invokes the compiled expression with giving three or more arguments.
		/// This field is readonly.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public readonly EvalFuncN EvaluateN;
		
		#endregion
		#region Properties

		/// <summary>
		/// Returns the expression string,
		/// that this <see cref="Evaluator"/> represents.</summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			return exprString;
			}

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Evaluator"/> implemented for.</summary>
		[DebuggerBrowsable(State.Never)]
		public int ArgumentsCount
			{
			[DebuggerHidden] get { return argsCount; }
			} 
		
		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the compiled expression with giving no arguments.</summary>
		/// <overloads>Invokes the compiled expression.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with no arguments is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate( )
			{
			return Evaluate0();
			}

		/// <summary>
		/// Invokes the compiled expression with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with one argument is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate( double arg )
			{
			return Evaluate1(arg);
			}

		/// <summary>
		/// Invokes the compiled expression with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with two arguments is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate( double arg1, double arg2 )
			{
			return Evaluate2(arg1, arg2);
			}

		// NOTE: fix summary if impl redirect
		/// <summary>
		/// Invokes the compiled expression with giving
		/// three or more arguments.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with three or more arguments is not compiled.</exception>
		/// <exception cref="ArgumentException"><paramref name="args"/> doesn't specify
		/// needed arguments <see cref="ArgumentsCount">count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate( params double[] args )
			{
			if( args.Length != argsCount )
				{
				throw new ArgumentException(
					string.Format(Resources.errWrongArgsCount,
								  args.Length, argsCount)
					);
				}

			return EvaluateN(args);
			}

		// TODO: redirect call?
		// TODO: Evaluate(1,2...) => Evaluate(1,2)

		#endregion
		#region Constructor

		internal Evaluator( string expr, Delegate method, int args )
			{
			Evaluate0 = (args == 0) ? (EvalFunc0) method : ThrowMethod0;
			Evaluate1 = (args == 1) ? (EvalFunc1) method : ThrowMethod1;
			Evaluate2 = (args == 2) ? (EvalFunc2) method : ThrowMethod2;
			EvaluateN = (args >= 3) ? (EvalFuncN) method : ThrowMethodN;
		  
			exprString = expr;
			argsCount = args;
			}

		#endregion
		#region Throw Methods

		[DebuggerHidden]
		private double ThrowMethod0( )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 0, argsCount)
				);
			}

		[DebuggerHidden]
		private double ThrowMethod1( double arg )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 1, argsCount)
				);
			}

		[DebuggerHidden]
		private double ThrowMethod2( double arg1, double arg2 )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, 2, argsCount)
				);
			}

		[DebuggerHidden]
		private double ThrowMethodN( double[] args )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount,
							  args.Length, argsCount)
				);
			}

		#endregion
		}
	}