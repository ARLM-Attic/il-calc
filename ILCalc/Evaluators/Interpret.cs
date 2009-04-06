using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	/// <summary>
	/// Represents the object for evaluating expression by interpreter.<br/>
	/// Instance of this class can be get from the <see cref="CalcContext.CreateInterpret"/> method.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// This class absolutely atomic from parent <see cref="CalcContext"/> class.
	/// </remarks>
	/// <threadsafety instance="false"/>
	
	[DebuggerDisplay("{ToString()} ({ArgumentsCount} argument(s))")]
	[Serializable]

	public sealed partial class Interpret : IEvaluator
		{
		#region Fields

		// expression info
		[Browsable(State.Never)] private readonly string _exprText;
		[Browsable(State.Never)] private readonly int _argCount;

		// interpretation data
		[Browsable(State.Never)] private readonly List<int> _code;
		[Browsable(State.Never)] private readonly List<double> _numbers;
		[Browsable(State.Never)] private readonly List<InterpCall> _funcs;
		[Browsable(State.Never)] private readonly List<Delegate> _delegates;

		// mutable fields
		[Browsable(State.Never)] private readonly int _stackMax;
		[Browsable(State.Never)] private bool _checkOvf;

		// evaluation stack
		[NonSerialized]
		[Browsable(State.Never)] private double[] _stack;

		// arguments array
		[NonSerialized]
		[Browsable(State.Never)] private double[] _param;

		#endregion
		#region Properties

		/// <summary>
		/// Returns the expression string, that this
		/// <see cref="Interpret"/> represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			return _exprText;
			}

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Interpret"/> implemented for.
		/// </summary>
		public int ArgumentsCount
			{
			[DebuggerHidden] get { return _argCount; }
			}

		/// <summary>Gets or sets checking mode for the expression evaluation.</summary>
		/// <remarks>Using this option will reduce perfomance of evaluation.</remarks>
		/// <value>
		/// Inherited from <see cref="CalcContext.OverflowCheck"/> property value.
		/// </value>
		public bool OverflowCheck
			{
			[DebuggerHidden] get { return _checkOvf; }
			[DebuggerHidden] set { _checkOvf = value; }
			}

		#endregion
		#region Constructor

		internal Interpret(string expr, int args, bool check,
						   InterpretCreator creator )
			{
			_exprText = expr;
			_argCount = args;
			_checkOvf = check;

			_code = creator._code;
			_funcs = creator._funcs;
			_numbers = creator._numbers;
			_delegates = creator._delegates;

			_stackMax = creator._stackMax;

			_stack = new double[_stackMax];
			_param = new double[_argCount];
			}

		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the expression interpreter with giving no arguments.
		/// </summary>
		/// <overloads>Invokes the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate()"/> method with no arguments.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( )
			{
			if(_argCount != 0)
				{
				throw new InvalidOperationException(
					string.Format(Resources.errWrongArgsCount, 0, _argCount)
					);
				}

			return RunInterp(_param);
			}
		
		/// <summary>
		/// Invokes the expression interpreter with giving one argument.
		/// </summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double)"/> method with one argument.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg )
			{
			if(_argCount != 1)
				{
				throw new InvalidOperationException(
					string.Format(Resources.errWrongArgsCount, 1, _argCount)
					);
				}

			_param[0] = arg;

			return RunInterp(_param);
			}

		/// <summary>
		/// Invokes the expression interpreter with giving two arguments.
		/// </summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double, double)"/> method with two arguments.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		[DebuggerHidden]
		public double Evaluate( double arg1, double arg2 )
			{
			if(_argCount != 2)
				{
				throw new InvalidOperationException(
					string.Format(Resources.errWrongArgsCount, 2, _argCount)
					);
				}

			_param[0] = arg1;
			_param[1] = arg2;

			return RunInterp(_param);
			}
		
		// TODO: Evaluate(,,)
		// TODO: Evaluate(,,,)

		/// <summary>
		/// Invokes the expression interpreter with giving three or more arguments.
		/// </summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
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
			if( _argCount != args.Length )
				{
				throw new ArgumentException(
					string.Format(Resources.errWrongArgsCount,
								  args.Length, _argCount)
					);
				}

			return RunInterp(args);
			}

		#endregion
		#region Members

		private double RunInterp( params double[] args )
			{
			int cPos = 0, // code position
				nPos = 0; // number position

			double[] stack = _stack; // prepared stack array
			int pos = -1;

			while(true)
				{
				int code = _code[cPos++];

				if( Code.IsOperator(code) ) //////////////////////// OPERATORS //
					{
					double value = stack[pos--];
					if(code != Code.Neg)
						{
						if( code == Code.Add ) stack[pos] += value; else
						if( code == Code.Mul ) stack[pos] *= value; else
						if( code == Code.Sub ) stack[pos] -= value; else
						if( code == Code.Div ) stack[pos] /= value; else
						if( code == Code.Rem ) stack[pos] %= value;
						else stack[pos] = Math.Pow(stack[pos], value);
						}
					else stack[++pos] = -value;
					}

				else if( code == Code.Number ) /////////////////////// NUMBERS //
					{
					stack[++pos] = _numbers[nPos++];
					}

				else ////////////////////////////////////////////////// OTHERS //
					{
					int id = _code[cPos++];

					if     ( code == Code.Argument  ) stack[++pos] = args[id];
					else if( code == Code.Delegate0 ) stack[++pos] = ((EvalFunc0)_delegates[id])( );
					else if( code == Code.Delegate1 ) stack[  pos] = ((EvalFunc1)_delegates[id])(stack[pos]);
					else if( code == Code.Delegate2 ) stack[--pos] = ((EvalFunc2)_delegates[id])(stack[pos], stack[pos+1]);
					else if( code == Code.Function  ) _funcs[id].InvokeFunc(stack, ref pos);
					else
						{
						if(_checkOvf) Check(stack[0]);

						return stack[0];
						}
					}
				}
			}

		private static void Check( double res )
			{
			if(double.IsInfinity(res) || double.IsNaN(res))
				{
				throw new NotFiniteNumberException(res.ToString());
				}
			}

		#endregion
		}
	}