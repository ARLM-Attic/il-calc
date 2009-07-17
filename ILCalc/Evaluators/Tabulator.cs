using System;
using System.Diagnostics;

namespace ILCalc
{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	/// <summary>
	/// Represents the object for evaluating compiled expression
	/// in specified range of argument values.<br/>
	/// Instance of this class can be get from the
	/// <see cref="CalcContext.CreateTabulator"/> method.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <remarks>Not available in the .NET CF / Silverlight versions.</remarks>
	/// <threadsafety instance="true" static="true"/>
	[DebuggerDisplay("{ToString()} ({RangesCount} range(s))")]

	public sealed class Tabulator
	{
		#region Fields

		[Browsable(State.Never)] private readonly TabFunc1 tabulator1;
		[Browsable(State.Never)] private readonly TabFunc2 tabulator2;
		[Browsable(State.Never)] private readonly TabFuncN tabulatorN;
		[Browsable(State.Never)] private readonly Allocator allocator;
		[Browsable(State.Never)] private readonly Delegate asyncTab;
		[Browsable(State.Never)] private readonly string exprString;
		[Browsable(State.Never)] private readonly int argsCount;

		#endregion
		#region Constructors

		private Tabulator(string expression, int argsCount)
		{
			Debug.Assert(expression != null);
			Debug.Assert(argsCount > 0);

			this.exprString = expression;
			this.argsCount = argsCount;
		}

		internal Tabulator(
			string expression, Delegate method, int argsCount, Allocator alloc)
			: this(expression, argsCount)
		{
			Debug.Assert(method != null);
			Debug.Assert(alloc  != null);
			Debug.Assert(argsCount > 2);

			this.tabulator1 = ThrowMethod1;
			this.tabulator2 = ThrowMethod2;
			this.tabulatorN = (TabFuncN) method;
			this.asyncTab   = (TabFuncN) this.tabulatorN.Invoke;

			this.allocator = alloc;
		}

		internal Tabulator(
			string expression, Delegate method, int argsCount)
			: this(expression, argsCount)
		{
			Debug.Assert(method != null);
			Debug.Assert(argsCount <= 2);

			if (argsCount == 1)
			{
				this.tabulator2 = ThrowMethod2;
				this.tabulator1 = (TabFunc1) method;
				this.asyncTab   = (TabFunc1) this.tabulator1.Invoke; // nice :)
				this.tabulatorN = (a,d) => this.tabulator1((double[]) a, d[0], d[1]);
			}
			else
			{
				this.tabulator1 = ThrowMethod1;
				this.tabulator2 = (TabFunc2) method;
				this.asyncTab   = (TabFunc2) this.tabulator2.Invoke;
				this.tabulatorN = (a,d) => this.tabulator2((double[][]) a, d[0], d[1], d[2], d[3]);
			}

			this.allocator = ThrowAlloc;
		}

		#endregion
		#region Delegates

		internal delegate double[] TabFunc1(double[] array, double step, double begin);

		internal delegate double[][] TabFunc2(double[][] array, double step1, double step2, double begin1, double begin2);

		internal delegate Array TabFuncN(Array array, params double[] data);

		internal delegate Array Allocator(params int[] lengths);

		#endregion
		#region Members

		/// <summary>
		/// Gets the argument ranges count, that
		/// this Tabulator implemented for.</summary>
		[Browsable(State.Never)]
		public int RangesCount
		{
			get { return this.argsCount; }
		}

		/// <summary>
		/// Returns the expression string, that
		/// this Tabulator represents.</summary>
		/// <returns>Expression string.</returns>
		public override string ToString()
		{
			return this.exprString;
		}

		#endregion
		#region Tabulate

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving one argument range.</summary>
		/// <overloads>Invokes the compiled expression tabulation.</overloads>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="end">Argument range end value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <exception cref="InvalidRangeException">
		/// Argument range from <paramref name="begin"/>, <paramref name="end"/>
		/// and <paramref name="step"/> is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 1.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>One-dimensional array of the evaluated values.</returns>
		public double[] Tabulate(double begin, double end, double step)
		{
			return Tabulate(
				new TabRange(begin, end, step));
		}

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving one argument range.</summary>
		/// <param name="range">Argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range"/> is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 1.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>One-dimensional array of the evaluated values.</returns>
		public double[] Tabulate(TabRange range)
		{
			return this.tabulator1(
				new double[range.ValidCount],
				range.Step,
				range.Begin);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving two argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range1"/> or <paramref name="range2"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 2.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Two-dimensional jagged array of the evaluated values.</returns>
		public double[][] Tabulate(TabRange range1, TabRange range2)
		{
			var array = new double[range1.ValidCount][];
			int count = range2.ValidCount;

			for(int i = 0; i < array.Length; i++)
			{
				array[i] = new double[count];
			}

			return this.tabulator2(
				array,
				range1.Step,
				range2.Step,
				range1.Begin,
				range2.Begin);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving three argument ranges.</summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <param name="range3">Third argument range.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>,
		/// <paramref name="range2"/> or <paramref name="range3"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 3.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Three-dimensional jagged array of the evaluated values
		/// casted to <see cref="Array"/> type.</returns>
		public Array Tabulate(TabRange range1, TabRange range2, TabRange range3)
		{
			if (this.argsCount != 3)
				throw WrongRanges(3);

			return this.tabulatorN(
				this.allocator(
					range1.ValidCount,
					range2.ValidCount,
					range3.ValidCount),
				range1.Step,
				range2.Step,
				range3.Step,
				range1.Begin,
				range2.Begin,
				range3.Begin);
		}

		//TODO: remarks & example!
		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with the specified argument <paramref name="ranges"/>.</summary>
		/// <param name="ranges">Argument ranges.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="ranges"/> is null.</exception>
		/// <exception cref="InvalidRangeException">
		/// Some instance of <paramref name="ranges"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal
		/// to specified <paramref name="ranges"/> count.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns><see cref="RangesCount">N</see>-dimensional jagged array
		/// of the evaluated values casted to <see cref="Array"/> type.</returns>
		public Array Tabulate(params TabRange[] ranges)
		{
			if (ranges == null || ranges.Length != this.argsCount)
			{
				throw WrongRanges(ranges);
			}

			var lengths = new int[ranges.Length];
			var data = new double[ranges.Length * 2];

			for(int i = 0; i < ranges.Length; i++)
			{
				TabRange range = ranges[i];

				lengths[i] = range.ValidCount;
				data[i] = range.Step;
				data[ranges.Length + i] = range.Begin;
			}

			return this.tabulatorN(this.allocator(lengths), data);
		}

		#endregion
		#region TabulateToArray

		/// <summary>
		/// Invokes the compiled expression tabulation with giving one
		/// argument range and output to the specified array.</summary>
		/// <overloads>Invokes the compiled expression tabulation
		/// with output to the pre-allocated array.</overloads>
		/// <param name="array">One-dimensional allocated
		/// array for evaluated values.</param>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="InvalidRangeException">Argument range
		/// from <paramref name="begin"/> and <paramref name="step"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 1.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public void TabulateToArray(double[] array, double begin, double step)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			this.tabulator1(array, step, begin);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation with giving one
		/// argument range and output to the specified array.</summary>
		/// <param name="array">One-dimensional allocated
		/// array for the evaluated values.</param>
		/// <param name="range">Argument range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="InvalidRangeException"><paramref name="range"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 1.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		public void TabulateToArray(double[] array, TabRange range)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			this.tabulator1(array, range.Step, range.Begin);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation with giving two
		/// argument ranges and output to the specified array.</summary>
		/// <param name="array">Two-dimensional jagged
		/// pre-allocated array for the evaluated values.</param>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>
		/// or <paramref name="range2"/> is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 2.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly evaluated (by the
		/// attached <see cref="Tabulator.Allocate(TabRange, TabRange)"/> method),
		/// or tabulator may throw <see cref="NullReferenceException"/>.</remarks>
		public void TabulateToArray(double[][] array, TabRange range1, TabRange range2)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			this.tabulator2(
				array,
				range1.Step,
				range2.Step,
				range1.Begin,
				range2.Begin);
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
		/// <exception cref="InvalidRangeException"><paramref name="range1"/>,
		/// <paramref name="range2"/> or <paramref name="range3"/> is not valid
		/// for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 3.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly allocated by using the attached
		/// <see cref="Tabulator.Allocate(TabRange, TabRange, TabRange)"/> method.<br/>
		/// Otherwise this tabulator may throw <see cref="NullReferenceException"/>
		/// or <see cref="InvalidCastException"/>.</remarks>
		public void TabulateToArray(Array array, TabRange range1, TabRange range2, TabRange range3)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			this.tabulatorN(
				array,
				range1.Step,
				range2.Step,
				range3.Step,
				range1.Begin,
				range2.Begin,
				range3.Begin);
		}

		/// <summary>
		/// Invokes the compiled expression tabulation with the specified argument
		/// <paramref name="ranges"/> and output to the specified array.</summary>
		/// <param name="array"><see cref="RangesCount">N</see>-dimensional
		/// jagged pre-allocated array of the evaluated values
		/// casted to <see cref="Array"/> type.</param>
		/// <param name="ranges">Argument ranges.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.</exception>
		/// <exception cref="InvalidRangeException">Some instance
		/// of <paramref name="ranges"/> is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">Expression's <see cref="RangesCount"/>
		/// is not equal to the specified <paramref name="ranges"/> count.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <remarks>Pre-allocated array should be correctly allocated by using
		/// the attached <see cref="Tabulator.Allocate(TabRange[])"/> method.<br/>
		/// Otherwise this tabulator may throw <see cref="NullReferenceException"/>
		/// or <see cref="InvalidCastException"/>.</remarks>
		public void TabulateToArray(Array array, params TabRange[] ranges)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (ranges == null || ranges.Length != this.argsCount)
			{
				throw WrongRanges(ranges);
			}

			var data = new double[ranges.Length * 2];
			for(int i = 0; i < ranges.Length; i++)
			{
				TabRange range = ranges[i];
				range.Validate();

				data[i] = range.Step;
				data[ranges.Length + i] = range.Begin;
			}

			this.tabulatorN(array, data);
		}

		#endregion
		#region AsyncTabulate

		/// <summary>
		/// Begins an asynchronous tabulation of the compiled
		/// expression with giving one argument range.</summary>
		/// <overloads>Begins an asynchronous tabulation
		/// of the compiled expression.</overloads>
		/// <param name="range">Argument range.</param>
		/// <param name="callback">
		/// The <see cref="AsyncCallback"/> delegate.</param>
		/// <param name="state">An object that contains
		/// state information for this tabulation.</param>
		/// <exception cref="InvalidRangeException"><paramref name="range"/>
		/// is not valid for iteration over it.</exception>
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 1.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			TabRange range, AsyncCallback callback, object state)
		{
			if (this.argsCount != 1)
				throw InvalidArgs(1);

			return ((TabFunc1) this.asyncTab).BeginInvoke(
				new double[range.ValidCount],
				range.Step,
				range.Begin,
				callback,
				state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the compiled
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
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 2.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			TabRange range1, TabRange range2, AsyncCallback callback, object state)
		{
			if( this.argsCount != 2 )
				throw InvalidArgs(2);

			var array = new double[range1.ValidCount][];
			int count = range2.ValidCount;

			for(int i = 0; i < array.Length; i++)
			{
				array[i] = new double[count];
			}

			return ((TabFunc2) this.asyncTab).BeginInvoke(
				array,
				range1.Step,
				range2.Step,
				range1.Begin,
				range2.Begin,
				callback,
				state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the compiled
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
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal 3.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>An <see cref="IAsyncResult"/> that references
		/// the asynchronous tabulation result.</returns>
		public IAsyncResult BeginTabulate(
			TabRange range1, TabRange range2, TabRange range3, AsyncCallback callback, object state)
		{
			if (this.argsCount != 3)
				throw WrongRanges(3);

			return ((TabFuncN) this.asyncTab).BeginInvoke(
				this.allocator(
					range1.ValidCount,
					range2.ValidCount,
					range3.ValidCount),
				new[]
				{
					range1.Step,  range2.Step,  range3.Step,
					range1.Begin, range2.Begin, range3.Begin
				}, 
				callback,
				state);
		}

		/// <summary>
		/// Begins an asynchronous tabulation of the compiled expression
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
		/// <exception cref="InvalidOperationException">
		/// Expression's <see cref="RangesCount"/> is not equal
		/// to specified <paramref name="ranges"/> count.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns><see cref="RangesCount">N</see>-dimensional jagged array
		/// of the evaluated values casted to <see cref="Array"/> type.</returns>
		public IAsyncResult BeginTabulate(
			TabRange[] ranges, AsyncCallback callback, object state)
		{
			if (ranges == null || ranges.Length != this.argsCount)
			{
				throw WrongRanges(ranges);
			}

			var lengths = new int[ranges.Length];
			var data = new double[ranges.Length*2];

			for(int i = 0; i < ranges.Length; i++)
			{
				TabRange range = ranges[i];

				lengths[i] = range.ValidCount;
				data[i] = range.Step;
				data[ranges.Length+i] = range.Begin;
			}

			return ((TabFuncN) this.asyncTab).BeginInvoke(
				this.allocator(lengths), data, callback, state);
		}


		/// <summary>
		/// Ends a pending asynchronous tabulation task.</summary>
		/// <param name="result">An <see cref="IAsyncResult"/>
		/// that stores state information and any user defined
		/// data for this asynchronous operation.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="result"/> is null.</exception>
		/// <exception cref="InvalidOperationException"><see cref="EndTabulate"/>
		/// was previously called for the asynchronous tabulation.</exception>
		/// <returns><see cref="RangesCount">N</see>-dimensional jagged array
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
		/// Argument range from <paramref name="begin"/>, <paramref name="end"/> and
		/// <paramref name="step"/> is not valid for iteration over it.</exception>
		/// <returns>Allocated one-dimensional array.</returns>
		public static double[] Allocate(double begin, double end, double step)
		{
			return new double[
				new TabRange(begin, end, step).ValidCount];
		}

		/// <summary>
		/// Allocates the array with length, that needed to tabulate
		/// some expression in the specified argument range.</summary>
		/// <param name="range">Argument range.</param>
		/// <exception cref="InvalidRangeException">
		/// <paramref name="range"/> is not valid for iteration over it.</exception>
		/// <returns>Allocated one-dimensional array.</returns>
		public static double[] Allocate(TabRange range)
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
		public static double[][] Allocate(TabRange range1, TabRange range2)
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
		public static Array Allocate(TabRange range1, TabRange range2, TabRange range3)
		{
			Allocator alloc = TabulatorCompiler.AllocCompiler.Resolve(3);

			return alloc(
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
		public static Array Allocate(params TabRange[] ranges)
		{
			if (ranges == null)
				throw new ArgumentNullException("ranges");

			if (ranges.Length == 1) return new double[ranges[0].ValidCount];
			if (ranges.Length == 2) return Allocate(ranges[0], ranges[1]);

			var lenghts = new int[ranges.Length];
			for (int i = 0; i < ranges.Length; i++)
			{
				lenghts[i] = ranges[i].ValidCount;
			}

			Allocator alloc = TabulatorCompiler.AllocCompiler.Resolve(ranges.Length);
			return alloc(lenghts);
		}

		#endregion
		#region Throw Methods

		private double[] ThrowMethod1(
			double[] array, double step, double begin)
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongRangesCount, 1, this.argsCount));
		}

		private double[][] ThrowMethod2(
			double[][] array,
			double step1,  double step2,
			double begin1, double begin2)
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongRangesCount, 2, this.argsCount));
		}

		// NOTE: unused?
		private Array ThrowAlloc(int[] length)
		{
			throw new InvalidOperationException(string.Format(
				Resource.errWrongRangesCount, length.Length, this.argsCount));
		}

		private Exception InvalidArgs(int actualCount)
		{
			return new InvalidOperationException(string.Format(
				Resource.errWrongRangesCount, actualCount, this.argsCount));
		}

		private Exception WrongRanges(TabRange[] ranges)
		{
			if (ranges == null)
				return new ArgumentNullException("ranges");

			return new ArgumentException(string.Format(
				Resource.errWrongRangesCount, ranges.Length, this.argsCount));
		}

		private Exception WrongRanges(int actualCount)
		{
			return new ArgumentException(string.Format(
				Resource.errWrongRangesCount, actualCount, this.argsCount));
		}

		#endregion
	}
}