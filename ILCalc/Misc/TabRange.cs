using System;
using System.Diagnostics;
using System.Text;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Defines a set of (begin, end, step) values,
	/// that represents the range of values.
	/// </summary>
	/// <remarks>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	[DebuggerDisplay("[{Begin} - {End}] step {Step}")]
	public struct TabRange : IEquatable<TabRange>
		{
		#region Fields

		[DebuggerBrowsable(State.Never)] private double _begin;
		[DebuggerBrowsable(State.Never)] private double _step;
		[DebuggerBrowsable(State.Never)] private double _end;
		[DebuggerBrowsable(State.Never)] private bool _checked;
		
		#endregion
		#region Properties
		
		/// <summary>Gets or sets the begining value of the range.</summary>
		public double Begin
			{
			[DebuggerHidden] get { return  _begin; }
			[DebuggerHidden]
			set
				{
				_begin = value;
				_checked = false;
				}
			}

		/// <summary>Gets or sets the ending value of the range.</summary>
		public double End
			{
			[DebuggerHidden] get { return  _end; }
			[DebuggerHidden]
			set
				{
				_end = value;
				_checked = false;
				}
			}

		/// <summary>Gets or sets the step value of the range.</summary>
		public double Step
			{
			[DebuggerHidden] get { return  _step; }
			[DebuggerHidden]
			set
				{
				_step = value;
				_checked = false;
				}
			}

		/// <summary>Calculates the lenght of the range.</summary>
		public double Length
			{
			[DebuggerHidden]
			get
				{
				return _end - _begin;
				}
			}

		/// <summary>Gets or sets the count of the steps, that would
		/// be taken while iteration over the range.</summary>
		/// <remarks>
		/// Is not guaranteed that by setting this property you
		/// will get range with <see cref="Count"/> iterations.
		/// Because of floating-point numbers precision, range
		/// step cannot be rightly evaluated for any 
		/// <see cref="Count"/> value.
		/// </remarks>
		public int Count
			{
			get 
				{
				if( _begin > _end != _step < 0.0 ) return 0;

				double len = _end - _begin;
				double dCount = len /_step;
				
				// too long range iterations count
				if( dCount >= int.MaxValue ) return int.MaxValue;
				
				int count = (int) dCount;

				if( len % _step == 0 ) count++;
				return count;
				}
			set
				{
				_step = (_end - _begin) / value;
				_checked = false;
				}
			}

		#endregion
		#region Members

		/// <summary>
		/// Returns the expression string, that this <see cref="Tabulator"/> represents.
		/// </summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			var buf = new StringBuilder( );
			buf.Append(_begin); buf.Append(" - ");
			buf.Append(_end);   buf.Append(" : ");
			buf.Append(_step);
			return buf.ToString( );
			}

		/// <summary>Throws an <see cref="InvalidRangeException"/> if this
		/// range instance is not valid for iteration over it.</summary>
		/// <exception cref="InvalidRangeException">
		/// Range is not valid for iteration over it.
		/// </exception>
		public void Validate()
			{
			if( _checked ) return;

			string msg = string.Empty;
			InvalidateReason reason = InternalValidate( );

			switch( reason )
				{
				case InvalidateReason.None: return;

				case InvalidateReason.NotFiniteRange:
					msg = Resources.errRangeNotFinite; break;

				case InvalidateReason.EndlessRange:
					msg = Resources.errEndlessLoop; break;

				case InvalidateReason.WrongStepSign:
					msg = Resources.errWrongStepSign; break;

				case InvalidateReason.RangeTooLoong:
					msg = Resources.errTooLongRange; break;
				}

			throw new InvalidRangeException(msg);
			}

		/// <summary>Returns <c>true</c> if this range instance
		/// is valid for iteration over it.</summary>
		/// <returns><b>true</b> if range is valid,
		/// otherwise <b>false</b></returns>
		public bool IsValid()
			{
			return _checked
				|| InternalValidate( ) == InvalidateReason.None;
			}

		private InvalidateReason InternalValidate( )
			{
			_checked = false;

			if(	double.IsInfinity(_begin)	|| double.IsNaN(_begin)
			||	double.IsInfinity(_step)	|| double.IsNaN(_step)
			||	double.IsInfinity(_end)		|| double.IsNaN(_end) )
				{
				return InvalidateReason.NotFiniteRange;
				}

			if( _begin + _step == _begin )
				{
				return InvalidateReason.EndlessRange;
				}

			if( _begin > _end != _step < 0 )
				{
				return InvalidateReason.WrongStepSign;
				}

			if( (_end - _begin) / _step >= int.MaxValue )
				{
				return InvalidateReason.RangeTooLoong;
				}

			_checked = true;
			return InvalidateReason.None;
			}

		private enum InvalidateReason
			{
			None,
			NotFiniteRange,
			EndlessRange,
			WrongStepSign,
			RangeTooLoong
			}

		#endregion
		#region Constructor

		// NOTE: constructor without step?
		// new TabRange(1, 10) { Count = 100 };

		/// <summary>
		/// Initializes a new instance of the <see cref="TabRange"/>
		/// structure with the specified begin, end and step values.
		/// </summary>
		/// <param name="begin">Range begin value.</param>
		/// <param name="end">Range end value.</param>
		/// <param name="step">Range step value.</param>
		public TabRange( double begin, double end, double step )
			{
			_checked = false;
			_begin = begin;
			_step = step;
			_end = end;
			}

		#endregion
		#region Overrides

		/// <summary>Returns the hash code of this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
			{
			return	_begin.GetHashCode( ) ^
					  _end.GetHashCode( ) ^
					 _step.GetHashCode( );
			}

		/// <summary>Indicates whether the current <see cref="TabRange"/>
		/// is equal to another <see cref="TabRange"/> structure.</summary>
		/// <overloads>Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> is equal.</overloads>
		/// <param name="other">An another <see cref="TabRange"/>
		/// to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="TabRange"/>
		/// is equal to the other <see cref="TabRange"/>;
		/// otherwise, <b>false</b>.</returns>
		public bool Equals( TabRange other )
			{
			return	_begin.Equals(other._begin)
				&&	_step.Equals(other._step)
				&&	_end.Equals(other._end);
			}

		/// <summary>
		/// Indicates whether the current <see cref="TabRange"/>
		/// is equal to another object.
		/// </summary>
		/// <param name="obj">An another <see cref="object"/> to compare with.</param>
		/// <returns>
		/// <b>true</b> if the current <see cref="TabRange"/> is equal 
		/// to the other <see cref="TabRange"/>;
		/// otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals( object obj )
			{
			return obj is TabRange
				&& Equals((TabRange) obj);
			}

		#endregion
		#region Operators

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> are equal.
		/// </summary>
		/// <param name="r1">A <see cref="TabRange"/></param>
		/// <param name="r2">A <see cref="TabRange"/></param>
		/// <returns>
		/// <b>true</b> if <paramref name="r1"/> and <paramref name="r2"/> are equal;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool operator==( TabRange r1, TabRange r2 )
			{
			return r1.Equals(r2);
			}

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> are not equal.
		/// </summary>
		/// <param name="r1">A <see cref="TabRange"/></param>
		/// <param name="r2">A <see cref="TabRange"/></param>
		/// <returns>
		/// <b>true</b> if <paramref name="r1"/> and <paramref name="r2"/> are not equal;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool operator!=( TabRange r1, TabRange r2 )
			{
			return !r1.Equals(r2);
			}

		#endregion
		}
	}