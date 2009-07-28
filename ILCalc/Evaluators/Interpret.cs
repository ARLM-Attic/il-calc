using System;
using System.Diagnostics;
using System.Threading;

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
		[Browsable(State.Never)] private readonly bool check;

		// interpretation data:
		[Browsable(State.Never)] private readonly int stackMax;
		[Browsable(State.Never)] private readonly int[] code;
		[Browsable(State.Never)] private readonly double[] numbers;
		[Browsable(State.Never)] private readonly FuncCall[] funcs;
#if !CF2
		[Browsable(State.Never)] private readonly Delegate[] delegates;
#endif

		// stack & params array, sync object:
		[Browsable(State.Never), NonSerialized] private double[] stackArray;
		[Browsable(State.Never), NonSerialized] private double[] paramArray;
		[Browsable(State.Never), NonSerialized] private object syncRoot;

		// async tabulator:
		[Browsable(State.Never), NonSerialized]
		private Delegate asyncTab;

		#endregion
		#region Constructor

		internal Interpret(
			string expression, int argsCount, bool check, InterpretCreator creator)
		{
			this.code = creator.Codes;
			this.funcs = creator.Functions;
			this.numbers = creator.Numbers;
#if !CF2
			this.delegates = creator.Delegates;
#endif

			this.expression = expression;
			this.stackMax = creator.StackMax;
			this.argsCount = argsCount;
			this.check = check;

			this.stackArray = new double[creator.StackMax];
			this.paramArray = new double[argsCount];

			this.syncRoot = new object();

			switch(argsCount)
			{
				case 1:  this.asyncTab = (TabFunc1) Tab1Impl; break;
				case 2:  this.asyncTab = (TabFunc2) Tab2Impl; break;
				default: this.asyncTab = (TabFuncN) TabNImpl; break;
			}
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets the arguments count, that this
		/// <see cref="Interpret"/> implemented for.
		/// </summary>
		public int ArgumentsCount
		{
			get { return this.argsCount; }
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
		/// can't be evaluated with no arguments given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double Evaluate()
		{
			if (this.argsCount != 0)
				throw WrongArgsCount(0);

			return Run(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with one arguments given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double Evaluate(double arg)
		{
			if (this.argsCount != 1)
				throw WrongArgsCount(1);

			this.paramArray[0] = arg;

			return Run(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with two argument given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double Evaluate(double arg1, double arg2)
		{
			if (this.argsCount != 2)
				throw WrongArgsCount(2);

			this.paramArray[0] = arg1;
			this.paramArray[1] = arg2;

			return Run(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter
		/// with giving three arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <param name="arg3">Third expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with three arguments given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double Evaluate(double arg1, double arg2, double arg3)
		{
			if (this.argsCount != 2)
				throw WrongArgsCount(2);

			this.paramArray[0] = arg1;
			this.paramArray[1] = arg2;
			this.paramArray[2] = arg3;

			return Run(this.stackArray, this.paramArray);
		}

		/// <summary>
		/// Invokes the expression interpreter.</summary>
		/// <param name="args">Expression arguments.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="args"/> doesn't specify needed
		/// <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double Evaluate(params double[] args)
		{
			if (args == null || args.Length != this.argsCount)
				throw WrongArgsCount(args);

			return Run(this.stackArray, args);
		}

		#endregion
		#region EvaluateSync

		/// <summary>
		/// Synchronously invokes the expression
		/// interpreter with giving no arguments.</summary>
		/// <overloads>Synchronously invokes
		/// the expression interpreter.</overloads>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with no arguments given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double EvaluateSync()
		{
			if (this.argsCount != 0)
				throw WrongArgsCount(0);

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					return Run(this.stackArray, this.paramArray);
				}
				finally { Monitor.Exit(this.syncRoot); }
			}

			// no need for allocate zero-lenght array
			return Run(new double[this.stackMax], this.paramArray);
		}

		/// <summary>
		/// Synchronously invokes the expression interpreter
		/// with giving one argument.</summary>
		/// <param name="arg">Expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with one argument given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double EvaluateSync(double arg)
		{
			if (this.argsCount != 1)
				throw WrongArgsCount(1);

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					this.paramArray[0] = arg;

					return Run(this.stackArray, this.paramArray);
				}
				finally { Monitor.Exit(this.syncRoot); }
			}

			return Run(new double[this.stackMax], new[] { arg });
		}

		/// <summary>
		/// Synchronously invokes the expression interpreter
		/// with giving two arguments.</summary>
		/// <param name="arg1">First expression argument.</param>
		/// <param name="arg2">Second expression argument.</param>
		/// <returns>Evaluated value.</returns>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be evaluated with two arguments given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public double EvaluateSync(double arg1, double arg2)
		{
			if (this.argsCount != 2)
				throw WrongArgsCount(2);

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					this.paramArray[0] = arg1;
					this.paramArray[1] = arg2;

					return Run(this.stackArray, this.paramArray);
				}
				finally { Monitor.Exit(this.syncRoot); }
			}

			return Run(new double[this.stackMax], new[] { arg1, arg2 });
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
		public double EvaluateSync(params double[] args)
		{
			if (args == null || args.Length != this.argsCount)
				throw WrongArgsCount(args);

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					return Run(this.stackArray, args);
				}
				finally { Monitor.Exit(this.syncRoot); }
			}

			return Run(new double[this.stackMax], args);
		}

		#endregion
		#region Tabulate

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving one argument range.</summary>
		/// <overloads>Invokes interpreter to tabulate the expression.</overloads>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="end">Argument range end value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <exception cref="InvalidRangeException">
		/// Argument range from <paramref name="begin"/>, <paramref name="end"/>
		/// and <paramref name="step"/> is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with one range given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>One-dimensional array of the evaluated values.</returns>
		public double[] Tabulate(double begin, double end, double step)
		{
			if (this.argsCount != 1)
				throw WrongRangesCount(1);

			var array = new double[
				new ValueRange(begin, end, step).ValidCount];
			return Tab1Impl(array, begin, step);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving one argument range.</summary>
		/// <param name="range">Argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range"/> is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with one range given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>One-dimensional array of the evaluated values.</returns>
		public double[] Tabulate(ValueRange range)
		{
			if (this.argsCount != 1)
				throw WrongRangesCount(1);

			var array = new double[range.ValidCount];
			return Tab1Impl(array, range.Begin, range.Step);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving two argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range1"/> or <paramref name="range2"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with two ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Two-dimensional jagged array of the evaluated values.</returns>
		public double[][] Tabulate(ValueRange range1, ValueRange range2)
		{
			if (this.argsCount != 2)
				throw WrongRangesCount(2);

			var array = new double[range1.ValidCount][];
			int count = range2.ValidCount;

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new double[count];
			}

			return Tab2Impl(array, range1, range2);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving two argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="range3">Third argument range.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>,
		/// <paramref name="range2"/> or <paramref name="range3"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with two ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Three-dimensional jagged array of the evaluated values
		/// casted to <see cref="Array"/> type.</returns>
		public Array Tabulate(ValueRange range1, ValueRange range2, ValueRange range3)
		{
			if (this.argsCount != 3)
				throw WrongRangesCount(3);

			Array array = Allocate(range1, range2, range3);
			return TabNImpl(array, range1, range2, range3);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with the specified argument <paramref name="ranges"/>.</summary>
		/// <param name="ranges">Argument ranges.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="ranges"/> is null.</exception>
		/// <exception cref="InvalidRangeException">
		/// Some instance of <paramref name="ranges"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="ranges"/> doesn't specify needed
		/// <see cref="ArgumentsCount">ranges count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns><see cref="ArgumentsCount">N</see>-dimensional jagged array
		/// of the evaluated values casted to <see cref="Array"/> type.</returns>
		public Array Tabulate(params ValueRange[] ranges)
		{
			if (ranges == null
			 || ranges.Length != this.argsCount)
			{
				throw WrongRangesCount(ranges);
			}

			Array array = Allocate(ranges);
			TabNImpl(array, ranges);

			return array;
		}

		#endregion
		#region TabulateToArray

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving one argument range and output
		/// to the pre-allocated array.</summary>
		/// <overloads>Invokes interpreter to tabulate the expression
		/// with output to the pre-allocated array.</overloads>
		/// <param name="array">One-dimensional allocated
		/// array for evaluated values.</param>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with one range given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>One-dimensional array of the evaluated values.</returns>
		public void TabulateToArray(double[] array, double begin, double step)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (this.argsCount != 1)
				throw WrongRangesCount(1);

			Tab1Impl(array, begin, step);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving one argument range and output
		/// to the pre-allocated array.</summary>
		/// <param name="array">One-dimensional allocated
		/// array for the evaluated values.</param>
		/// <param name="range">Argument range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="InvalidRangeException"><paramref name="range"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with one range given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public void TabulateToArray(double[] array, ValueRange range)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (this.argsCount != 1)
				throw WrongRangesCount(1);

			Tab1Impl(array, range.Begin, range.Step);
		}

		/// <summary>
		/// Invokes interpreter to tabulate the expression
		/// with giving one argument range and output
		/// to the pre-allocated array.</summary>
		/// <param name="array">Two-dimensional jagged
		/// pre-allocated array for the evaluated values.</param>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with two ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly evaluated (by the
		/// attached <see cref="Interpret.Allocate(ILCalc.ValueRange, ILCalc.ValueRange)"/> method),
		/// or interpret may throw <see cref="NullReferenceException"/>.</remarks>
		public void TabulateToArray(
			double[][] array, ValueRange range1, ValueRange range2)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (this.argsCount != 2)
				throw WrongRangesCount(2);

			Tab2Impl(array, range1, range2);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation with giving three
		/// argument ranges and output to the specified array.</summary>
		/// <param name="array">Three-dimensional jagged pre-allocated array
		/// of the evaluated values casted to <see cref="Array"/> type.</param>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="range3">Third argument range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with two ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly allocated by using the attached
		/// <see cref="Interpret.Allocate(ILCalc.ValueRange, ILCalc.ValueRange, ILCalc.ValueRange)"/> method.<br/>
		/// Otherwise this interpret may throw <see cref="NullReferenceException"/>
		/// or <see cref="InvalidCastException"/>.</remarks>
		public void TabulateToArray(
			Array array, ValueRange range1, ValueRange range2, ValueRange range3)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (this.argsCount != 3)
				throw WrongRangesCount(3);

			TabNImpl(array, range1, range2, range3);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation with the specified argument
		/// <paramref name="ranges"/> and output to the specified array.</summary>
		/// <param name="array"><see cref="ArgumentsCount">N</see>-dimensional
		/// jagged pre-allocated array of the evaluated values
		/// casted to <see cref="Array"/> type.</param>
		/// <param name="ranges">Argument ranges.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="ranges"/> doesn't specify needed
		/// <see cref="ArgumentsCount">ranges count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly allocated by using
		/// the attached <see cref="Interpret.Allocate(ILCalc.ValueRange[])"/> method.<br/>
		/// Otherwise this interpret may throw <see cref="NullReferenceException"/>
		/// or <see cref="InvalidCastException"/>.</remarks>
		public void TabulateToArray(Array array, params ValueRange[] ranges)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (ranges == null
			 || ranges.Length != this.argsCount)
			{
				throw WrongRangesCount(ranges);
			}

			TabNImpl(array, ranges);
		}

		#endregion
		#region AsyncTabulate

		/// <summary>
		/// Begins an asynchronous tabulation of the
		/// expression with giving one argument range.</summary>
		/// <overloads>Begins an asynchronous
		/// tabulation of the expression.</overloads>
		/// <param name="range">Argument range.</param>
		/// <param name="callback">
		/// The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains
		/// state information for this tabulation.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with one range given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			ValueRange range, AsyncCallback callback, object state)
		{
			if (this.argsCount != 1)
				throw WrongRangesCount(1);

			var array = new double[range.ValidCount];

			return ((TabFunc1) this.asyncTab).BeginInvoke(
				array, range.Begin, range.Step, callback, state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the
		/// expression with giving two argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="callback">
		/// The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains
		/// state information for this tabulation.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range1"/> or <paramref name="range2"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with two ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			ValueRange range1,
			ValueRange range2,
			AsyncCallback callback,
			object state)
		{
			if (this.argsCount != 2)
				throw WrongRangesCount(2);

			var array = new double[range1.ValidCount][];
			int count = range2.ValidCount;

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new double[count];
			}

			return ((TabFunc2) this.asyncTab).BeginInvoke(
				array, range1, range2, callback, state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the
		/// expression with giving three argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="range3">Third argument range.</param>
		/// <param name="callback">
		/// The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains
		/// state information for this tabulation.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>,
		/// <paramref name="range2"/> or <paramref name="range3"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException"><see cref="Interpret"/>
		/// can't be tabulated with three ranges given, you should specify
		/// valid <see cref="ArgumentsCount">arguments count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			ValueRange range1,
			ValueRange range2,
			ValueRange range3,
			AsyncCallback callback,
			object state)
		{
			if (this.argsCount != 3)
				throw WrongRangesCount(3);

			Array array = Allocate(range1, range2, range3);
			return ((TabFuncN) this.asyncTab).BeginInvoke(
				array, new[] { range1, range2, range3 }, callback, state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the expression
		/// with specified argument <paramref name="ranges"/>.</summary>
		/// <param name="ranges">Argument ranges.</param>
		/// <param name="callback">
		/// The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains
		/// state information for this tabulation.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="ranges"/> is null.</exception>
		/// <exception cref="InvalidRangeException">
		/// Some instance of <paramref name="ranges"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="ranges"/> doesn't specify needed
		/// <see cref="ArgumentsCount">ranges count</see>.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns><see cref="ArgumentsCount">N</see>-dimensional jagged array
		/// of the evaluated values casted to <see cref="Array"/> type.</returns>
		public IAsyncResult BeginTabulate(
			ValueRange[] ranges, AsyncCallback callback, object state)
		{
			if (ranges == null
			 || ranges.Length != this.argsCount)
			{
				throw WrongRangesCount(ranges);
			}

			Array array = Allocate(ranges);
			return ((TabFuncN) this.asyncTab).BeginInvoke(
				array, ranges, callback, state);
		}

		/// <summary>
		/// Ends a pending asynchronous tabulation task.</summary>
		/// <param name="result">An <see cref="IAsyncResult"/>
		/// that stores state information and any user defined
		/// data for this asynchronous operation.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="result"/> is null.</exception>
		/// <exception cref="ArgumentException"><see cref="EndTabulate"/>
		/// was previously called for the asynchronous tabulation.</exception>
		/// <returns><see cref="ArgumentsCount">N</see>-dimensional jagged array
		/// of the evaluated values casted to <see cref="Array"/> type.</returns>
		public Array EndTabulate(IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException("result");

			switch (this.argsCount)
			{
				case 1:  return ((TabFunc1) this.asyncTab).EndInvoke(result);
				case 2:  return ((TabFunc2) this.asyncTab).EndInvoke(result);
				default: return ((TabFuncN) this.asyncTab).EndInvoke(result);
			}
		}

		#endregion
		#region Delegates

		internal delegate double[] TabFunc1(
			double[] array, double begin, double step);

		internal delegate double[][] TabFunc2(
			double[][] array, ValueRange range1, ValueRange range2);

		internal delegate Array TabFuncN(
			Array array, params ValueRange[] data);

		#endregion
		#region Allocate

		/// <summary>
		/// Allocates the array with length, that needed to tabulate
		/// some expression in the specified argument range.</summary>
		/// <overloads>
		/// Allocates the array with length(s), that needed to tabulate
		/// some expression in specified argument range(s).</overloads>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="end">Argument range end value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <exception cref="InvalidRangeException">
		/// Argument range from <paramref name="begin"/>,
		/// <paramref name="end"/> and <paramref name="step"/>
		/// is not valid for iteration over it.</exception>
		/// <returns>Allocated one-dimensional array.</returns>
		public static double[] Allocate(double begin, double end, double step)
		{
			return new double[
				new ValueRange(begin, end, step).ValidCount];
		}

		/// <summary>
		/// Allocates the array with length, that needed to tabulate
		/// some expression in the specified argument range.</summary>
		/// <param name="range">Argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range"/> is not valid for iteration over it.</exception>
		/// <returns>Allocated one-dimensional array.</returns>
		public static double[] Allocate(ValueRange range)
		{
			return new double[range.ValidCount];
		}

		/// <summary>
		/// Allocates the array with lengths, that needed to tabulate
		/// some expression in the two specified argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range1"/> or <paramref name="range2"/>
		/// is not valid for iteration over it.</exception>
		/// <returns>Allocated two-dimensional jagged array.</returns>
		public static double[][] Allocate(ValueRange range1, ValueRange range2)
		{
			var array = new double[range1.ValidCount][];
			int count = range2.ValidCount;

			for(int i = 0; i < array.Length; i++)
			{
				array[i] = new double[count];
			}

			return array;
		}

		/// <summary>
		/// Allocates the array with lengths, that needed to tabulate
		/// some expression in the three specified argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="range3">Third argument range.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>,
		/// <paramref name="range2"/> or <paramref name="range3"/>
		/// is not valid for iteration over it.</exception>
		/// <returns>Allocated three-dimensional jagged array
		/// casted to <see cref="Array"/> type.</returns>
		public static Array Allocate(
			ValueRange range1, ValueRange range2, ValueRange range3)
		{
			return AllocImpl(
				0,
				range1.ValidCount,
				range2.ValidCount,
				range3.ValidCount);
		}

		/// <summary>
		/// Allocates the array with lengths, that needed
		/// to tabulate some expression in the specified
		/// argument <paramref name="ranges"/>.</summary>
		/// <param name="ranges">Argument ranges.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="ranges"/> is null.</exception>
		/// <exception cref="InvalidRangeException">
		/// Some instance of <paramref name="ranges"/>
		/// is not valid for iteration over it.</exception>
		/// <returns>Allocated <paramref name="ranges"/>-dimensional
		/// jagged array casted to <see cref="Array"/> type.</returns>
		public static Array Allocate(params ValueRange[] ranges)
		{
			if (ranges == null)
				throw new ArgumentNullException("ranges");

			if (ranges.Length == 1) return new double[ranges[0].ValidCount];
			if (ranges.Length == 2) return Allocate(ranges[0], ranges[1]);

			var sizes = new int[ranges.Length];
			for (int i = 0; i < ranges.Length; i++)
			{
				sizes[i] = ranges[i].ValidCount;
			}

			return AllocImpl(0, sizes);
		}

		#endregion
		#region Internals

		private double Run(double[] stackArr, double[] args)
		{
			int c = 0, // code position
			    n = 0, // number position
			    i =-1; // stack marker

			double[] stack = stackArr;
			while (true)
			{
				int op = this.code[c++];
				if (Code.IsOperator(op))
				{
					double value = stack[i--];
					if (op != Code.Neg)
					{
						if      (op == Code.Add) stack[i] += value;
						else if (op == Code.Mul) stack[i] *= value;
						else if (op == Code.Sub) stack[i] -= value;
						else if (op == Code.Div) stack[i] /= value;
						else if (op == Code.Rem) stack[i] %= value;
						else stack[i] = Math.Pow(stack[i], value);
					}
					else stack[++i] = -value;
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
					else if (op == Code.Delegate0) stack[++i] = ((EvalFunc0) this.delegates[id])();
					else if (op == Code.Delegate1) stack[  i] = ((EvalFunc1) this.delegates[id])(stack[i]);
					else if (op == Code.Delegate2) stack[--i] = ((EvalFunc2) this.delegates[id])(stack[i], stack[i+1]);
#endif
					else if (op == Code.Function)
					{
						this.funcs[id].Invoke(stack, ref i);
					}
					else
					{
						if (this.check) Check(stack[0]);
						return stack[0];
					}
				}
			}
		}

		private static void Check(double res)
		{
			if (double.IsInfinity(res) || double.IsNaN(res))
			{
				throw new NotFiniteNumberException(res.ToString());
			}
		}

		private double[] Tab1Impl(double[] array, double begin, double step)
		{
			var stack = new double[this.stackMax];
			var args = new[] { begin };

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Run(stack, args);
				args[0] += step;
			}

			return array;
		}

		private double[][] Tab2Impl(double[][] array, ValueRange range1, ValueRange range2)
		{
			var stack = new double[this.stackMax];
			var args = new[] { range1.Begin, range2.Begin };

			for (int i = 0; i < array.Length; i++)
			{
				double[] row = array[i];

				args[1] = range2.Begin;
				for (int j = 0; j < row.Length; j++)
				{
					row[j] = Run(stack, args);
					args[1] += range2.Step;
				}

				args[0] += range1.Step;
			}

			return array;
		}

		private Array TabNImpl(Array array, params ValueRange[] ranges)
		{
			var stack = new double[this.stackMax];
			var args = new double[ranges.Length];

			return TabNImpl(args, stack, array, 0, ranges);
		}

		private Array TabNImpl(
			double[] args, double[] stack,
			Array xarray, int pos, params ValueRange[] ranges)
		{
			int next = pos + 1;
			if (ranges.Length - pos == 2)
			{
				var array = (double[][]) xarray;
				double step = ranges[next].Step;

				args[pos] = ranges[pos].Begin;
				for (int i = 0; i < array.Length; i++)
				{
					double[] row = array[i];

					args[next] = ranges[next].Begin;
					for (int j = 0; j < row.Length; j++)
					{
						row[j] = Run(stack, args);
						args[next] += step;
					}

					args[pos] += ranges[pos].Step;
				}
			}
			else
			{
				args[pos] = ranges[pos].Begin;
				for (int i = 0; i < xarray.Length; i++)
				{
					var array = (Array) xarray.GetValue(i);
					TabNImpl(args, stack, array, next, ranges);

					args[pos] += ranges[pos].Step;
				}
			}

			return xarray;
		}

		private static Array AllocImpl(int pos, params int[] sizes)
		{
			if (sizes.Length - pos == 2)
			{
				var array = new double[sizes[pos]][];
				int count = sizes[pos + 1];

				for (int i = 0; i < array.Length; i++)
					array[i] = new double[count];

				return array;
			}
			else
			{
				var array = Array.CreateInstance(
					TypeHelper.GetArrayType(sizes.Length - pos - 1),
					sizes[pos++]);

				for (int i = 0; i < array.Length; i++)
					array.SetValue(AllocImpl(pos, sizes), i);

				return array;
			}
		}

		#endregion
		#region Throw Helpers

		private Exception WrongArgsCount(int actualCount)
		{
			return new ArgumentException(string.Format(
			                             	Resource.errWrongArgsCount,
			                             	actualCount,
			                             	this.argsCount));
		}

		private Exception WrongArgsCount(double[] args)
		{
			if (args == null)
				return new ArgumentNullException("args");

			return new ArgumentException(string.Format(
			                             	Resource.errWrongArgsCount,
			                             	args.Length,
			                             	this.argsCount));
		}

		private Exception WrongRangesCount(ValueRange[] ranges)
		{
			if (ranges == null)
				return new ArgumentNullException("ranges");

			return new ArgumentException(string.Format(
			                             	Resource.errWrongRangesCount,
			                             	ranges.Length,
			                             	this.argsCount));
		}

		private Exception WrongRangesCount(int actualCount)
		{
			return new ArgumentException(string.Format(
			                             	Resource.errWrongRangesCount,
			                             	actualCount,
			                             	this.argsCount));
		}

		#endregion
	}
}