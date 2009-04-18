using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Defines a set of (begin, end, step) values,
	/// that represents the range of values.</summary>
	/// <remarks>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	/// <threadsafety instance="false"/>
	
	[DebuggerDisplay("[{Begin} - {End}] step {Step}")]
	[Serializable]

	public struct TabRange : IEquatable<TabRange>,
							 IEnumerable<double>
		{
		#region Fields

		// range values:
		[DebuggerBrowsable(State.Never)] private double begin;
		[DebuggerBrowsable(State.Never)] private double step;
		[DebuggerBrowsable(State.Never)] private double end;

		// cached info:
		[DebuggerBrowsable(State.Never)] private bool isChecked;
		[DebuggerBrowsable(State.Never)] private int count;
		
		#endregion
		#region Properties
		
		/// <summary>Gets or sets the begining value of the range.</summary>
		public double Begin
			{
			[DebuggerHidden] get { return begin; }
			[DebuggerHidden] set { begin = value; OnChange( ); }
			}

		/// <summary>Gets or sets the ending value of the range.</summary>
		public double End
			{
			[DebuggerHidden] get { return end; }
			[DebuggerHidden] set { end = value; OnChange( ); }
			}

		/// <summary>Gets or sets the step value of the range.</summary>
		public double Step
			{
			[DebuggerHidden] get { return step; }
			[DebuggerHidden] set { step = value; OnChange( ); }
			}

		/// <summary>
		/// Gets or sets the count of the steps, that would
		/// be taken while iteration over the range.</summary>
		/// <remarks>
		/// Is not guaranteed that by setting this property you
		/// will get range with <see cref="Count"/> iterations.<br/>
		/// Because of floating-point numbers precision, range step
		/// cannot be rightly evaluated for any <see cref="Count"/> value.
		/// </remarks>
		public int Count
			{
			get
				{
				if( count > 0 ) return count;
				return IsValid( )? InternalGetCount( ): 0;
				}
			set
				{
				InternalSetCount(value);
				OnChange( );
				}
			}

		#endregion
		#region Methods

		/// <summary>
		/// Converts the values of this range
		/// to its equivalent string representation.</summary>
		/// <returns>Expression string.</returns>
		public override string ToString( )
			{
			var buf = new StringBuilder( );
			buf.Append(begin); buf.Append(" - ");
			buf.Append(end);   buf.Append(" : ");
			buf.Append(step);
			return buf.ToString( );
			}

		private void OnChange( ) { isChecked = false; count = 0; }

		#endregion
		#region Validate

		private enum InvalidateReason
			{
			None,
			NotFiniteRange,
			EndlessRange,
			WrongStepSign,
			RangeTooLoong
			}

		/// <summary>
		/// Throws an <see cref="InvalidRangeException"/> if this
		/// range instance is not valid for iteration over it.</summary>
		/// <exception cref="InvalidRangeException">
		/// Range is not valid for iteration over it.
		/// </exception>
		public void Validate()
			{
			if( isChecked ) return;

			string msg = string.Empty;
			InvalidateReason reason = InternalValidate( );

			switch( reason )
				{
				case InvalidateReason.None: return;

				case InvalidateReason.NotFiniteRange:	msg = Resources.errRangeNotFinite;	break;
				case InvalidateReason.EndlessRange:		msg = Resources.errEndlessLoop;		break;
				case InvalidateReason.WrongStepSign:	msg = Resources.errWrongStepSign;	break;
				case InvalidateReason.RangeTooLoong:	msg = Resources.errTooLongRange;	break;
				}

			throw new InvalidRangeException(msg);
			}

		/// <summary>
		/// Returns <c>true</c> if this range instance
		/// is valid for iteration over it.</summary>
		/// <returns><b>true</b> if range is valid,
		/// otherwise <b>false</b></returns>
		public bool IsValid()
			{
			return isChecked
				|| InternalValidate( ) == InvalidateReason.None;
			}

		private InvalidateReason InternalValidate( )
			{
			isChecked = false;

			if(	double.IsInfinity(begin)	|| double.IsNaN(begin)
			||	double.IsInfinity(step)		|| double.IsNaN(step)
			||	double.IsInfinity(end)		|| double.IsNaN(end) )
				{
				return InvalidateReason.NotFiniteRange;
				}

			if( begin + step == begin )
				return InvalidateReason.EndlessRange;

			if( begin > end != step < 0 )
				return InvalidateReason.WrongStepSign;

			if( (end - begin) / step >= int.MaxValue )
				return InvalidateReason.RangeTooLoong;

			isChecked = true;
			return InvalidateReason.None;
			}

		#endregion
		#region Internals

		private bool MoveNext( ref double value )
			{
			if( value < end )
				{
				value += step;
				return true;
				}

			return false;
			}

		private void InternalSetCount( int value )
			{
			step = (end - begin) / value;
			}

		private int InternalGetCount( )
			{
			double len = end - begin;
			var iCount = (int) (len / step);

			if( len % step == 0 ) iCount++;
			return iCount;
			}

		#endregion
		#region Overrides

		/// <summary>Returns the hash code of this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
			{
			return	begin.GetHashCode( ) ^
					  end.GetHashCode( ) ^
					 step.GetHashCode( );
			}

		/// <summary>
		/// Indicates whether the current <see cref="TabRange"/> is
		/// equal to another <see cref="TabRange"/> structure.</summary>
		/// <overloads>
		/// Returns a value indicating whether two instances of
		/// <see cref="TabRange"/> is equal.</overloads>
		/// <param name="other">An another <see cref="TabRange"/>
		/// to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="TabRange"/> is equal to
		/// the other <see cref="TabRange"/>; otherwise, <b>false</b>.</returns>
		public bool Equals( TabRange other )
			{
			return	begin.Equals(other.begin)
				&&	step.Equals(other.step)
				&&	end.Equals(other.end);
			}

		/// <summary>
		/// Indicates whether the current <see cref="TabRange"/>
		/// is equal to another object.</summary>
		/// <param name="obj">
		/// An another <see cref="object"/> to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="TabRange"/> is equal to
		/// the other <see cref="TabRange"/>; otherwise, <b>false</b>.</returns>
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
		/// <param name="a">A <see cref="TabRange"/></param>
		/// <param name="b">A <see cref="TabRange"/></param>
		/// <returns><b>true</b> if <paramref name="a"/> and <paramref name="b"/>
		/// are equal; otherwise, <b>false</b>.</returns>
		public static bool operator==( TabRange a, TabRange b )
			{
			return a.Equals(b);
			}

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> are not equal.
		/// </summary>
		/// <param name="a">A <see cref="TabRange"/></param>
		/// <param name="b">A <see cref="TabRange"/></param>
		/// <returns><b>true</b> if <paramref name="a"/> and  <paramref name="b"/>
		/// are not equal; otherwise, <b>false</b>.</returns>
		public static bool operator!=( TabRange a, TabRange b )
			{
			return !a.Equals(b);
			}

		#endregion
		#region Enumerator

		
		/// <summary>
		/// Enumerates all values that will be taken while iterating
		/// over numeric range that this <see cref="TabRange"/> represents.
		/// </summary>
		public struct Enumerator : IEnumerator<double>
			{
			#region Members

			private readonly TabRange range;
			private double current;

			/// <summary>Gets the value at the current position of the enumerator.</summary>
			public double Current		{ get { return current; } }
			object IEnumerator.Current	{ get { return current; } }

			/// <summary>Advances the enumerator to the next element of the range.</summary>
			/// <returns><b>true</b> if the enumerator was successfully advanced to the next value;
			/// <b>false</b> if the enumerator has passed the end of the range.</returns>
			public bool MoveNext( )	{ return range.MoveNext(ref current); }

			/// <summary>Sets the enumerator to begin position of the range.</summary>
			public void Reset( )	{ current = range.Begin; }

			/// <summary>Releases all resources
			/// used by the <see cref="TabRange.Enumerator"/>.</summary>
			public void Dispose( )	{ }

			#endregion
			#region Constructor

			internal Enumerator( TabRange range )
				{
				range.Validate( );
				this.range = range;
				current = range.Begin;
				}

			#endregion
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through the values
		/// that will be taken while iterating over numeric range that
		/// this <see cref="TabRange"/> represents.</summary>
		/// <returns>An enumerator for values of the numeric range
		/// from this <see cref="TabRange"/>.</returns>
		public Enumerator GetEnumerator( )
			{
			Validate( );
			return new Enumerator(this);
			}

		IEnumerator<double> IEnumerable<double>.GetEnumerator( )
			{
			Validate( );
			return new Enumerator(this);
			}

		IEnumerator IEnumerable.GetEnumerator( )
			{
			throw new NotImplementedException( );
			}

		#endregion
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="TabRange"/>
		/// structure with the specified begin, end and step values.
		/// </summary>
		/// <param name="begin">Range begin value.</param>
		/// <param name="end">Range end value.</param>
		/// <param name="step">Range step value.</param>
		public TabRange( double begin, double end, double step )
			{
			this.begin = begin;
			this.step = step;
			this.end = end;

			isChecked = false;
			count = 0;
			}

		#endregion
		}
	}