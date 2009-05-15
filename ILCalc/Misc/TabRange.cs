using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILCalc
{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	// TODO: no laziness?
	// TODO: faster Count

	/// <summary>
	/// Defines a set of (begin, end, step) values,
	/// that represents the range of values.</summary>
	/// <remarks>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	/// <threadsafety instance="false"/>
	[DebuggerDisplay("[{Begin} - {End}] step {Step}")]
	[Serializable]

	public struct TabRange : IEquatable<TabRange>, IEnumerable<double>
	{
		#region Fields

		// range values:
		[Browsable(State.Never)] private double begin;
		[Browsable(State.Never)] private double step;
		[Browsable(State.Never)] private double end;

		// cached info:
		[Browsable(State.Never)] private bool isChecked;
		[Browsable(State.Never)] private int count;
		
		#endregion
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="TabRange"/>
		/// structure with the specified begin, end and step values.</summary>
		/// <param name="begin">Range begin value.</param>
		/// <param name="end">Range end value.</param>
		/// <param name="step">Range step value.</param>
		public TabRange(double begin, double end, double step)
		{
			this.begin = begin;
			this.step = step;
			this.end = end;

			this.isChecked = false;
			this.count = 0;
		}

		#endregion
		#region Properties
		
		/// <summary>Gets or sets the begining value of the range.</summary>
		public double Begin
		{
			[DebuggerHidden]
			get
			{
				return this.begin;
			}

			[DebuggerHidden]
			set
			{
				this.begin = value;
				this.RangeChanged();
			}
		}

		/// <summary>Gets or sets the ending value of the range.</summary>
		public double End
		{
			[DebuggerHidden]
			get
			{
				return this.end;
			}

			[DebuggerHidden]
			set
			{
				this.end = value;
				this.RangeChanged();
			}
		}

		/// <summary>Gets or sets the step value of the range.</summary>
		public double Step
		{
			[DebuggerHidden]
			get
			{
				return this.step;
			}

			[DebuggerHidden]
			set
			{
				this.step = value;
				this.RangeChanged();
			}
		}

		/// <summary>
		/// Gets or sets the count of the steps, that would
		/// be taken while iteration over the range.</summary>
		/// <remarks>
		/// Is not guaranteed that by setting this property you will get
		/// range with <see cref="Count"/> iterations - because of floating-point
		/// numbers precision, needed range step cannot be rightly evaluated
		/// for any setted <see cref="Count"/> value.</remarks>
		public int Count
		{
			[DebuggerHidden]
			get
			{
				return this.count > 0 ?
					this.count :
					this.InternalGetCount(false);
			}

			[DebuggerHidden]
			set
			{
				this.InternalSetCount(value);
				this.RangeChanged();
			}
		}

		[Browsable(State.Never)]
		internal int ValidCount
		{
			[DebuggerHidden]
			get
			{
				return this.count > 0 ?
					this.count :
					this.InternalGetCount(true);
			}
		}

		#endregion
		#region Methods

		/// <summary>
		/// Converts the values of this range to its
		/// equivalent string representation.</summary>
		/// <returns>Expression string.</returns>
		public override string ToString()
		{
			var buf = new StringBuilder();
			buf.Append(this.begin);
			buf.Append(" - ");
			buf.Append(this.end);
			buf.Append(" : ");
			buf.Append(this.step);
			return buf.ToString();
		}

		#endregion
		#region Overrides

		/// <summary>
		/// Returns the hash code of this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return
				this.begin.GetHashCode() ^
				this.end.GetHashCode() ^
				this.step.GetHashCode();
		}

		/// <summary>
		/// Indicates whether the current <see cref="TabRange"/> is
		/// equal to another <see cref="TabRange"/> structure.</summary>
		/// <overloads>Returns a value indicating whether two instances
		/// of the <see cref="TabRange"/> is equal.</overloads>
		/// <param name="other">An another <see cref="TabRange"/> to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="TabRange"/> is equal to
		/// the other <see cref="TabRange"/>; otherwise, <b>false</b>.</returns>
		public bool Equals(TabRange other)
		{
			return this.begin.Equals(other.begin)
				&& this.step.Equals(other.step)
				&& this.end.Equals(other.end);
		}

		/// <summary>
		/// Indicates whether the current <see cref="TabRange"/>
		/// is equal to another object.</summary>
		/// <param name="obj">An another <see cref="object"/> to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="TabRange"/> is equal to
		/// the other <see cref="TabRange"/>; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			return obj is TabRange
				&& this.Equals((TabRange) obj);
		}

		#endregion
		#region Operators

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> are equal.</summary>
		/// <param name="a">First <see cref="TabRange"/>.</param>
		/// <param name="b">Second <see cref="TabRange"/>.</param>
		/// <returns><b>true</b> if <paramref name="a"/> and <paramref name="b"/>
		/// are equal; otherwise, <b>false</b>.</returns>
		public static bool operator ==(TabRange a, TabRange b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="TabRange"/> are not equal.</summary>
		/// <param name="a">First <see cref="TabRange"/>.</param>
		/// <param name="b">Second <see cref="TabRange"/>.</param>
		/// <returns><b>true</b> if <paramref name="a"/> and  <paramref name="b"/>
		/// are not equal; otherwise, <b>false</b>.</returns>
		public static bool operator !=(TabRange a, TabRange b)
		{
			return !a.Equals(b);
		}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through the values
		/// that will be taken while iterating over numeric range that
		/// this <see cref="TabRange"/> represents.</summary>
		/// <returns>An enumerator for values of the numeric range
		/// from this <see cref="TabRange"/>.</returns>
		public Enumerator GetEnumerator()
		{
			this.Validate();
			return new Enumerator(this);
		}

		IEnumerator<double> IEnumerable<double>.GetEnumerator()
		{
			this.Validate();
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

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
			if (this.isChecked)
			{
				return;
			}

			string msg = string.Empty;
			InvalidateReason reason = this.InternalValidate();

			switch (reason)
			{
				case InvalidateReason.None:
					return;
				case InvalidateReason.NotFiniteRange:
					msg = Resource.errRangeNotFinite;
					break;
				case InvalidateReason.EndlessRange:
					msg = Resource.errEndlessLoop;
					break;
				case InvalidateReason.WrongStepSign:
					msg = Resource.errWrongStepSign;
					break;
				case InvalidateReason.RangeTooLoong:
					msg = Resource.errTooLongRange;
					break;
			}

			throw new InvalidRangeException(msg);
		}

		/// <summary>
		/// Returns <c>true</c> if this range instance
		/// is valid for iteration over it.</summary>
		/// <returns><b>true</b> if range is valid, otherwise <b>false</b></returns>
		public bool IsValid()
			{
			return this.isChecked
				|| this.InternalValidate() == InvalidateReason.None;
			}

		private InvalidateReason InternalValidate()
		{
			this.isChecked = false;

			if (double.IsInfinity(this.begin) || double.IsNaN(this.begin)
			 || double.IsInfinity(this.step)  || double.IsNaN(this.step)
			 || double.IsInfinity(this.end)   || double.IsNaN(this.end))
				{
				return InvalidateReason.NotFiniteRange;
				}

			if (this.begin + this.step == this.begin)
			{
				return InvalidateReason.EndlessRange;
			}

			if (this.begin > this.end != this.step < 0)
			{
				return InvalidateReason.WrongStepSign;
			}

			if ((this.end - this.begin) / this.step >= int.MaxValue)
			{
				return InvalidateReason.RangeTooLoong;
			}

			this.isChecked = true;
			return InvalidateReason.None;
			}

		#endregion
		#region Internals

		private void RangeChanged()
		{
			this.isChecked = false;
			this.count = 0;
		}

		private bool MoveNext(ref double value)
		{
			if (value < this.end)
			{
				value += this.step;
				return true;
			}

			return false;
		}

		private void InternalSetCount(int value)
		{
			this.step = (this.end - this.begin) / value;
		}

		private int InternalGetCount(bool needValid)
		{
			if (needValid)
			{
				this.Validate();
			}
			else if (!this.IsValid())
			{
				return 0;
			}

			double len = this.end - this.begin;
			var rangeCount = (int)(len / this.step) + 1;

			if (len % this.step == 0)
			{
				rangeCount--;
			}

			return rangeCount;
		}

		#endregion
		#region Enumerator

		/// <summary>
		/// Enumerates all values that will be taken while iterating over numeric range
		/// that this <see cref="TabRange"/> represents.</summary>
		public struct Enumerator : IEnumerator<double>
		{
			private readonly TabRange range;
			private double current;

			internal Enumerator(TabRange range)
			{
				range.Validate();
				this.range = range;
				this.current = range.Begin;
			}

			/// <summary>Gets the value at the current position of the enumerator.</summary>
			public double Current
			{
				get { return this.current; }
			}

			object IEnumerator.Current
			{
				get { return this.current; }
			}

			/// <summary>Advances the enumerator to the next element of the range.</summary>
			/// <returns><b>true</b> if the enumerator was successfully advanced to the next value;
			/// <b>false</b> if the enumerator has passed the end of the range.</returns>
			public bool MoveNext()
			{
				return this.range.MoveNext(ref this.current);
			}

			/// <summary>Sets the enumerator to begin position of the range.</summary>
			public void Reset()
			{
				this.current = this.range.Begin;
			}

			/// <summary>Releases all resources used
			/// by the <see cref="TabRange.Enumerator"/>.</summary>
			public void Dispose()
			{
			}
		}

		#endregion
	}
}