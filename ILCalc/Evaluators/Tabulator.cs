using System;
using System.Diagnostics;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Represents the object for evaluating compiled expression
	/// in specfied range of arguments values.<br/>
	/// Instance of this class can be get from the <see cref="CalcContext.CreateTabulator"/>
	/// method.<br/>This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// This class absolutely atomic from parent <see cref="CalcContext"/> class.<br/>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	/// <threadsafety instance="true"/>
	
	[DebuggerDisplay("{ToString()} ({RangesCount} range(s))")]

	public sealed class Tabulator
		{
		#region Delegates

		internal delegate double[] TabFunc1( double x, int count, double step );

		internal delegate double[][] TabFunc2(
			double x, int count1,
			double y, int count2,
			double step1, double step2 );

		#endregion
		#region Fields

		[DebuggerBrowsable(State.Never)] private readonly TabFunc1 _tab1;
		[DebuggerBrowsable(State.Never)] private readonly TabFunc2 _tab2;
		[DebuggerBrowsable(State.Never)] private readonly string _expr;
		[DebuggerBrowsable(State.Never)] private readonly bool _oneArg;
		
		#endregion
		#region Members

		/// <summary>
		/// Returns the expression string, that this Tabulator represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		[DebuggerHidden]
		public override string ToString( )
			{
			return _expr;
			}

		/// <summary>
		/// Gets the argument ranges count, that this Tabulator implemented for.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public int RangesCount
			{
			[DebuggerHidden]
			get { return _oneArg? 1: 2; }
			}

		#endregion
		#region Tabulate

		// TODO: more exception info

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving one argument range.
		/// </summary>
		/// <overloads>Invokes the compiled expression tabulation.</overloads>
		/// <param name="begin">Argument range begin value.</param>
		/// <param name="end">Argument range end value.</param>
		/// <param name="step">Argument range step value.</param>
		/// <returns>Array of evaluated values.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Tabulator"/> with one argument range is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		public double[] Tabulate(double begin, double end, double step)
			{
			var range = new TabRange(begin, end, step);
			range.Validate( );
			
			return _tab1(begin, range.Count, step);
			}

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving one argument range.
		/// </summary>
		/// <param name="range">Argument range.</param>
		/// <returns>Array of evaluated values.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Tabulator"/> with one argument range is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		public double[] Tabulate( TabRange range )
			{
			range.Validate();
			
			return _tab1(range.Begin, range.Count, range.Step);
			}

		/// <summary>
		/// Invokes the compiled expression tabulation
		/// with giving two arguments ranges.
		/// </summary>
		/// <param name="range1">First argument range.</param>
		/// <param name="range2">Second argument range.</param>
		/// <returns>Array of arrays of evaluated values.</returns>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Tabulator"/> with two arguments ranges is not compiled.
		/// </exception>
		/// <exception cref="ArithmeticException">
		/// Expression evaluation thrown the <see cref="ArithmeticException"/>.
		/// </exception>
		public double[][] Tabulate( TabRange range1, TabRange range2 )
			{
			range1.Validate( );
			range2.Validate( );

			return _tab2( range1.Begin, range1.Count,
						  range2.Begin, range2.Count,
						  range1.Step,  range2.Step );
			}

		#endregion
		#region Constructor

		internal Tabulator( string expr, Delegate method, bool oneArg )
			{
			if(oneArg)
				{
				_tab1 = (TabFunc1) method;
				_tab2 = ThrowFunc2;
				}
			else
				{
				_tab1 = ThrowFunc1;
				_tab2 = (TabFunc2) method;
				}

			_oneArg = oneArg;
			_expr = expr;
			}

		#endregion
		#region Throw Funcs

		private double[] ThrowFunc1( double x, int count, double step)
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongRangesCount, 1, RangesCount)
				);
			}

		private double[][] ThrowFunc2( double x, int count1,
									   double y, int count2,
									   double step1, double step2 )
			{
			throw new InvalidOperationException(
				string.Format(Resources.errWrongRangesCount, 2, RangesCount)
				);
			}

		#endregion
		}
	}
