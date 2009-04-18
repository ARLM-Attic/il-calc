using System;
using System.Diagnostics;
using System.Threading;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	/// <summary>
	/// Represents the object for evaluating expression by interpreter.<br/>
	/// Instance of this class can be get from
	/// the <see cref="CalcContext.CreateInterpret"/> method.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety>
	/// Instance <see cref="Evaluate()"/> methods are not thread-safe.
	/// Use the <see cref="EvaluateSync()"/> method group instead.
	/// </threadsafety>
	
	[DebuggerDisplay("{ToString()} ({ArgumentsCount} argument(s))")]
	[Serializable]

	public sealed partial class Interpret
		{
		#region Fields

		// expression info:
		[Browsable(State.Never)] private readonly string exprString;
		[Browsable(State.Never)] private readonly int argsCount;

		// interpretation data:
		[Browsable(State.Never)] private readonly int[] code;
		[Browsable(State.Never)] private readonly double[] numbers;
		[Browsable(State.Never)] private readonly FuncCall[] funcs;
		[Browsable(State.Never)] private readonly Delegate[] delegates;
		[Browsable(State.Never)] private readonly int stackMax;
		[Browsable(State.Never)] private readonly bool checkedMode;

		// pre-allocated stack & params arrays, sync object:
		[NonSerialized, Browsable(State.Never)] private double[] stackArray;
		[NonSerialized, Browsable(State.Never)] private double[] paramArray;
		[NonSerialized, Browsable(State.Never)] private object syncRoot;

		#endregion
		#region Properties

		/// <summary>
		/// Returns the expression string, that this
		/// <see cref="Interpret"/> represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			return exprString;
			}

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Interpret"/> implemented for.
		/// </summary>
		public int ArgumentsCount
			{
			[DebuggerHidden] get { return argsCount; }
			}

		/// <summary>Gets the checking mode for the expression evaluation.</summary>
		/// <value>Inherited from <see cref="CalcContext.OverflowCheck"/>
		/// property of parent context value.</value>
		public bool OverflowCheck
			{
			[DebuggerHidden] get { return checkedMode; }
			}

		#endregion
		#region Constructor

		internal Interpret( string expr, int args, bool check,
							InterpretCreator creator )
			{
			exprString = expr;
			argsCount = args;

			code = creator.code.ToArray( );
			funcs = creator.funcs.ToArray( );
			numbers = creator.numbers.ToArray( );
			delegates = creator.delegates.ToArray( );

			stackMax = creator.stMax;
			checkedMode = check;

			stackArray = new double[stackMax];
			paramArray = new double[argsCount];

			syncRoot = new object( );
			}

		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving no arguments.</summary>
		/// <overloads>Invokes the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate()"/> method
		/// with no arguments.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( )
			{
			if( argsCount != 0 )
				throw WrongArgsCount(0);

			return RunInterp(stackArray, paramArray);
			}
		
		/// <summary>
		/// Invokes the expression interpreter
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double)"/> method
		/// with one argument.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg )
			{
			if( argsCount != 1 ) 
				throw WrongArgsCount(1);

			paramArray[0] = arg;

			return RunInterp(stackArray, paramArray);
			}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double, double)"/>
		/// method with two arguments.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg1, double arg2 )
			{
			if( argsCount != 2 )
				throw WrongArgsCount(2);

			paramArray[0] = arg1;
			paramArray[1] = arg2;

			return RunInterp(stackArray, paramArray);
			}

		// TODO: Evaluate(,,)
		// TODO: Evaluate(,,,)

		/// <summary>
		/// Invokes the expression interpreter.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><paramref name="args"/> doesn't
		/// specify needed <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( params double[] args )
			{
			if( argsCount != args.Length )
				throw WrongArgsCount(args.Length);

			return RunInterp(stackArray, args);
			}

		#endregion
		#region EvaluateSync

		/// <summary>
		/// Synchronously invokes the expression interpreter
		/// with giving no arguments.</summary>
		/// <overloads>Synchronously invokes
		/// the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate()"/> method
		/// with no arguments.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double EvaluateSync( )
			{
			if( argsCount != 0 )
				throw WrongArgsCount(0);

			if( Monitor.TryEnter(syncRoot) )
				{
				try		{ return RunInterp(stackArray, paramArray); }
				finally	{ Monitor.Exit(syncRoot); }
				}

			// no need for allocate zero-lenght array
			return RunInterp(new double[stackMax], paramArray);
			}

		/// <summary>
		/// Synchronously invokes the expression interpreter
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double)"/> method
		/// with one argument.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double EvaluateSync( double arg )
			{
			if( argsCount != 1 )
				throw WrongArgsCount(1);

			if( Monitor.TryEnter(syncRoot) )
				{
				paramArray[0] = arg;

				try		{ return RunInterp(stackArray, paramArray); }
				finally	{ Monitor.Exit(syncRoot); }
				}

			return RunInterp(new double[stackMax], new[] { arg });
			}

		/// <summary>
		/// Synchronously invokes the expression interpreter
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double, double)"/>
		/// method with two arguments.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		//[DebuggerHidden]
		public double EvaluateSync( double arg1, double arg2 )
			{
			if( argsCount != 2 )
				throw WrongArgsCount(2);

			if( Monitor.TryEnter(syncRoot) )
				{
				paramArray[0] = arg1;
				paramArray[1] = arg2;

				try		{ return RunInterp(stackArray, paramArray); }
				finally	{ Monitor.Exit(syncRoot); }
				}

			return RunInterp(new double[stackMax], new[] { arg1, arg2 });
			}

		/// <summary>
		/// Synchronously invokes the expression interpreter.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><paramref name="args"/> doesn't
		/// specify needed <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double EvaluateSync( params double[] args )
			{
			if( argsCount != args.Length )
				throw WrongArgsCount(args.Length);
			
			if( Monitor.TryEnter(syncRoot) )
				{
				try		{ return RunInterp(stackArray, args); }
				finally	{ Monitor.Exit(syncRoot); }
				}

			return RunInterp(new double[stackMax], args);
			}

		#endregion
		#region Helpers

		private Exception WrongArgsCount( int actual )
			{
			return new InvalidOperationException(
				string.Format(Resources.errWrongArgsCount, actual, argsCount)
				);
			}

		#endregion
		#region Methods

		private double RunInterp( double[] stackArr, double[] args )
			{
			int cPos = 0, // code position
			    nPos = 0; // number position

			double[] stack = stackArr; // prepared stack array
			int pos = -1;

			while( true )
				{
				int op = code[cPos++];

				if( Code.IsOperator(op) ) //////////////////////// OPERATORS //
					{
					double value = stack[pos--];
					if( op != Code.Neg )
						{
						if( op == Code.Add ) stack[pos] += value; else
						if( op == Code.Mul ) stack[pos] *= value; else
						if( op == Code.Sub ) stack[pos] -= value; else
						if( op == Code.Div ) stack[pos] /= value; else
						if( op == Code.Rem ) stack[pos] %= value; else
							stack[pos] = Math.Pow(stack[pos], value);
						}
					else stack[++pos] = -value;
					}

				else if( op == Code.Number ) /////////////////////// NUMBERS //
					{
					stack[++pos] = numbers[nPos++];
					}

				else //////////////////////////////////////////////// OTHERS //
					{
					int id = code[cPos++];

					if( op == Code.Argument  ) stack[++pos] = args[id]; else
					if( op == Code.Delegate0 ) stack[++pos] = ((EvalFunc0) delegates[id])( ); else
					if( op == Code.Delegate1 ) stack[  pos] = ((EvalFunc1) delegates[id])(stack[pos]); else
					if( op == Code.Delegate2 ) stack[--pos] = ((EvalFunc2) delegates[id])(stack[pos], stack[pos+1]); else
					if( op == Code.Function  ) funcs[id].Invoke(stack, ref pos);
					else
						{
						if( checkedMode ) Check(stack[0]);
						return stack[0];
						}
					}
				}
			}

		private static void Check( double res )
			{
			if( double.IsInfinity(res)
			 || double.IsNaN(res) )
				{
				throw new NotFiniteNumberException(res.ToString( ));
				}
			}

		#endregion
		}
	}