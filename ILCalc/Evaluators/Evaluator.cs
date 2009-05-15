using System;
using System.Diagnostics;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

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

		/// <summary>
		/// Directly invokes the compiled expression with giving no arguments.
		/// This field is readonly.</summary>
		[Browsable(State.Never)]
		public readonly EvalFunc0 Evaluate0;

		/// <summary>
		/// Directly invokes the compiled expression with giving one argument.
		/// This field is readonly.</summary>
		[Browsable(State.Never)]
		public readonly EvalFunc1 Evaluate1;

		/// <summary>
		/// Directly invokes the compiled expression with giving two arguments.
		/// This field is readonly.</summary>
		[Browsable(State.Never)]
		public readonly EvalFunc2 Evaluate2;

		// NOTE: fix summary if impl redirect

		/// <summary>
		/// Directly invokes the compiled expression with giving three or more arguments.
		/// This field is readonly.</summary>
		[Browsable(State.Never)]
		public readonly EvalFuncN EvaluateN;
		
		[Browsable(State.Never)]
		private readonly string expression;

		[Browsable(State.Never)]
		private readonly int argsCount;

		#endregion
		#region Constructor

		internal Evaluator(string expression, Delegate method, int argsCount)
		{
			Debug.Assert(expression != null);
			Debug.Assert(argsCount >= 0);
			Debug.Assert(method != null);

			this.Evaluate0 = this.Throw0;
			this.Evaluate1 = this.Throw1;
			this.Evaluate2 = this.Throw2;

			if (argsCount == 0)
			{
				this.Evaluate0 = (EvalFunc0) method;
				this.EvaluateN = (a => this.Evaluate0());
			}
			else if (argsCount == 1)
			{
				this.Evaluate1 = (EvalFunc1) method;
				this.EvaluateN = (a => this.Evaluate1(a[0]));
			}
			else if (argsCount == 2)
			{
				this.Evaluate2 = (EvalFunc2) method;
				this.EvaluateN = (a => this.Evaluate2(a[0], a[1]));
			}
			else
			{
				this.EvaluateN = (EvalFuncN) method;
			}

			this.expression = expression;
			this.argsCount = argsCount;
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Evaluator"/> implemented for.</summary>
		[Browsable(State.Never)]
		public int ArgumentsCount
		{
			[DebuggerHidden]
			get { return this.argsCount; }
		}

		/// <summary>
		/// Returns the expression string,
		/// that this <see cref="Evaluator"/> represents.</summary>
		/// <returns>Expression string.</returns>
		[DebuggerHidden]
		public override string ToString()
		{
			return this.expression;
		}

		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the compiled expression evaluation
		/// with giving no arguments.</summary>
		/// <overloads>Invokes the compiled expression evaluation.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with no arguments is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate()
		{
			return this.Evaluate0();
		}

		/// <summary>
		/// Invokes the compiled expression evaluation
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException"><see cref="Evaluator"/>
		/// with one argument is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(double arg)
		{
			return this.Evaluate1(arg);
		}

		/// <summary>
		/// Invokes the compiled expression evaluation
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Evaluator"/> with two arguments is not compiled.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(double arg1, double arg2)
		{
			return this.Evaluate2(arg1, arg2);
		}

		/// <summary>
		/// Invokes the compiled expression evaluation
		/// with giving three arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <param name="arg3">Third expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <see cref="Evaluator"/> can't be evaluated
		/// with giving three arguments.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(double arg1, double arg2, double arg3)
		{
			return this.EvaluateN(arg1, arg2, arg3);
		}

		/// <summary>
		/// Invokes the compiled expression evaluation with the
		/// specified <paramref name="args">arguments</paramref>.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="args"/> doesn't specify needed
		/// <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(params double[] args)
		{
			if (args == null
			 || args.Length != this.argsCount)
			{
				this.WrongArgs(args);
			}

			return this.EvaluateN(args);
		}

		#endregion
		#region Throw Methods

		[DebuggerHidden]
		private double Throw0()
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongArgsCount, 0, this.argsCount));
		}

		[DebuggerHidden]
		private double Throw1(double arg)
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongArgsCount, 1, this.argsCount));
		}

		[DebuggerHidden]
		private double Throw2(double arg1, double arg2)
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongArgsCount, 2, this.argsCount));
		}

		[DebuggerHidden]
		private void WrongArgs(double[] args)
		{
			if( args == null )
				throw new ArgumentNullException("args");

			throw new ArgumentException(string.Format(
				Resource.errWrongArgsCount, args.Length, this.argsCount));
		}

		#endregion
		}
	}