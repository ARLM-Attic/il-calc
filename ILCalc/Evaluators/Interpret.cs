using System;
using System.Diagnostics;
using System.Threading;

// NOTE: what faster - if check / virtual call?

namespace ILCalc
{
	using Browsable = DebuggerBrowsableAttribute;
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Represents the object for evaluating expression by interpreter.<br/>
	/// Instance of this class can be get from
	/// the <see cref="CalcContext.CreateInterpret"/> method.<br/>
	/// This class cannot be inherited.</summary>
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
		[Browsable(State.Never)] private readonly string expression;
		[Browsable(State.Never)] private readonly int argsCount;

		// interpretation data:
		[Browsable(State.Never)] private readonly int[] code;
		[Browsable(State.Never)] private readonly double[] numbers;
		[Browsable(State.Never)] private readonly FuncCall[] funcs;
#if !CF2
		[Browsable(State.Never)] private readonly Delegate[] delegates;
#endif

		[Browsable(State.Never)] private readonly bool checkedMode;
		[Browsable(State.Never)] private readonly int stackMax;

		// stack & params array, sync object:
		[NonSerialized, Browsable(State.Never)] private double[] stackArray;
		[NonSerialized, Browsable(State.Never)] private double[] paramArray;
		[NonSerialized, Browsable(State.Never)] private object syncRoot;

		#endregion
		#region Constructor

		internal Interpret(string expression, int argsCount, bool check, InterpretCreator creator)
		{
			this.code = creator.Codes;
			this.funcs = creator.Functions;
			this.numbers = creator.Numbers;
#if !CF2
			this.delegates = creator.Delegates;
#endif

			this.stackMax = creator.StackMax;
			this.expression = expression;
			this.argsCount = argsCount;
			this.checkedMode = check;

			this.stackArray = new double[creator.StackMax];
			this.paramArray = new double[argsCount];

			this.syncRoot = new object();
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Interpret"/> implemented for.
		/// </summary>
		public int ArgumentsCount
		{
			[DebuggerHidden]
			get { return this.argsCount; }
		}

		/// <summary>
		/// Gets a value indicating whether arithmetic
		/// checks causes during the expression evaluation.</summary>
		/// <value>Inherited from <see cref="CalcContext.OverflowCheck"/>
		/// property of parent context value.</value>
		public bool OverflowCheck
		{
			[DebuggerHidden]
			get { return this.checkedMode; }
		}

		/// <summary>
		/// Returns the expression string, that this
		/// <see cref="Interpret"/> represents.
		/// </summary>
		/// <returns>
		/// Expression string.
		/// </returns>
		public override string ToString()
		{
			return this.expression;
		}

		#endregion
		#region Evaluate

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving no arguments.</summary>
		/// <overloads>Invokes the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated by <see cref="Interpret.Evaluate()"/> method
		/// with no arguments.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate()
		{
			if (this.argsCount != 0)
			{
				throw this.WrongArgsCount(0);
			}

			return this.RunInterp(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double)"/> method
		/// with one argument.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(double arg)
		{
			if (this.argsCount != 1)
			{
				throw this.WrongArgsCount(1);
			}

			this.paramArray[0] = arg;

			return this.RunInterp(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <see cref="Interpret"/> can't be evaluated by 
		/// <see cref="Interpret.Evaluate(double, double)"/>
		/// method with two arguments.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double Evaluate(double arg1, double arg2)
		{
			if (this.argsCount != 2)
			{
				throw this.WrongArgsCount(2);
			}

			this.paramArray[0] = arg1;
			this.paramArray[1] = arg2;

			return this.RunInterp(this.stackArray, this.paramArray);
		}

		// TODO: Evaluate(,,)
		// TODO: Evaluate(,,,)

		/// <summary>
		/// Invokes the expression interpreter.</summary>
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
			if (this.argsCount != args.Length)
			{
				throw this.WrongArgsCount(args.Length);
			}

			return this.RunInterp(this.stackArray, args);
		}

		#endregion
		#region EvaluateSync

		/// <summary>
		/// Synchronously invokes the expression
		/// interpreter with giving no arguments.</summary>
		/// <overloads>Synchronously invokes
		/// the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Interpret"/> can't be evaluated
		/// by <see cref="Interpret.Evaluate()"/>
		/// method with no arguments.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double EvaluateSync()
		{
			if (this.argsCount != 0)
			{
				throw this.WrongArgsCount(0);
			}

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					return this.RunInterp(this.stackArray, this.paramArray);
				}
				finally
				{
					Monitor.Exit(this.syncRoot);
				}
			}

			// no need for allocate zero-lenght array
			return this.RunInterp(new double[this.stackMax], this.paramArray);
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
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double EvaluateSync(double arg)
		{
			if (this.argsCount != 1)
			{
				throw this.WrongArgsCount(1);
			}

			if (Monitor.TryEnter(this.syncRoot))
			{
				this.paramArray[0] = arg;

				try
				{
					return this.RunInterp(this.stackArray, this.paramArray);
				}
				finally
				{
					Monitor.Exit(this.syncRoot);
				}
			}

			return this.RunInterp(new double[this.stackMax], new[] { arg });
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
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double EvaluateSync(double arg1, double arg2)
		{
			if (this.argsCount != 2)
			{
				throw this.WrongArgsCount(2);
			}

			if (Monitor.TryEnter(this.syncRoot))
			{
				this.paramArray[0] = arg1;
				this.paramArray[1] = arg2;

				try
				{
					return this.RunInterp(this.stackArray, this.paramArray);
				}
				finally
				{
					Monitor.Exit(this.syncRoot);
				}
			}

			return this.RunInterp(new double[this.stackMax], new[] { arg1, arg2 });
		}

		/// <summary>
		/// Synchronously invokes the expression interpreter.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="args"/> doesn't specify needed
		/// <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		[DebuggerHidden]
		public double EvaluateSync(params double[] args)
		{
			if (this.argsCount != args.Length)
			{
				throw this.WrongArgsCount(args.Length);
			}

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					return this.RunInterp(this.stackArray, args);
				}
				finally
				{
					Monitor.Exit(this.syncRoot);
				}
			}

			return this.RunInterp(new double[this.stackMax], args);
		}

		#endregion
		#region Privates

		private static void Check(double res)
		{
			if (double.IsInfinity(res) || double.IsNaN(res))
			{
				throw new NotFiniteNumberException(res.ToString());
			}
		}

		private Exception WrongArgsCount(int actualCount)
		{
			return new ArgumentException(string.Format(
				Resource.errWrongArgsCount, actualCount, this.argsCount));
		}

		private double RunInterp(double[] stackArr, double[] args)
		{
			int c = 0, // code position
			    n = 0; // number position

			double[] stack = stackArr;
			int i = -1;

			while (true)
			{
				int op = this.code[c++];

				if (Code.IsOperator(op))
				{
					double value = stack[i--];
					if (op != Code.Neg)
					{
						if (op == Code.Add)
						{
							stack[i] += value;
						}
						else if (op == Code.Mul)
						{
							stack[i] *= value;
						}
						else if (op == Code.Sub)
						{
							stack[i] -= value;
						}
						else if (op == Code.Div)
						{
							stack[i] /= value;
						}
						else if (op == Code.Rem)
						{
							stack[i] %= value;
						}
						else
						{
							stack[i] = Math.Pow(stack[i], value);
						}
					}
					else
					{
						stack[++i] = -value;
					}
				}
				else if (op == Code.Number)
				{
					stack[++i] = this.numbers[n++];
				}
				else
				{
					int id = this.code[c++];

					if (op == Code.Argument)
					{
						stack[++i] = args[id];
					}
#if !CF2
					else if (op == Code.Delegate0)
					{
						stack[++i] = ((EvalFunc0) this.delegates[id])();
					}
					else if (op == Code.Delegate1)
					{
						stack[i] = ((EvalFunc1) this.delegates[id])(stack[i]);
					}
					else if (op == Code.Delegate2)
					{
						stack[--i] = ((EvalFunc2) this.delegates[id])(stack[i], stack[i + 1]);
					}
#endif
					else if (op == Code.Function)
					{
						this.funcs[id].Invoke(stack, ref i);
					}
					else
					{
						if (this.checkedMode)
						{
							Check(stack[0]);
						}

						return stack[0];
					}
				}
			}
		}

		#endregion
	}
}