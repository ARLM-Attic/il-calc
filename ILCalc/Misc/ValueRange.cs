using System;
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

	public struct ValueRange : IEquatable<ValueRange>
	{
		#region Fields

		[DebuggerBrowsable(State.Never)] private readonly double begin;
		[DebuggerBrowsable(State.Never)] private readonly double step;
		[DebuggerBrowsable(State.Never)] private readonly double end;
		[DebuggerBrowsable(State.Never)] private readonly int count;
		[DebuggerBrowsable(State.Never)]
		private readonly InvalidateReason valid;

		#endregion
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ILCalc.ValueRange"/>
		/// structure with the specified begin, end and step values.</summary>
		/// <param name="begin">Range begin value.</param>
		/// <param name="end">Range end value.</param>
		/// <param name="step">Range step value.</param>
		public ValueRange(double begin, double end, double step)
		{
			this.begin = begin;
			this.step = step;
			this.end = end;
			this.valid = 0;
			this.count = 0;

			if ((this.valid = InternalValidate()) == 0)
			{
				this.count = InternalGetCount();
			}
		}

		private ValueRange(
			double begin, double end, int count, bool fictive)
		{
			this.begin = begin;
			this.end = end;
			this.step = default(double);
			this.valid = 0;
			this.count = 0;

			this.step  = InternalSetCount(count);
			this.valid = InternalValidate();
			this.count = count;
		}

		#endregion
		#region Properties
		
		/// <summary>
		/// Gets the begining value of the range.</summary>
		public double Begin { get { return this.begin; } }

		/// <summary>
		/// Gets the ending value of the range.</summary>
		public double End { get { return this.end; } }

		/// <summary>
		/// Gets the step value of the range.</summary>
		public double Step { get { return this.step; } }

		/// <summary>
		/// Gets or sets the count of the steps, that would
		/// be taken while iteration over the range.</summary>
		/// <remarks>
		/// Is not guaranteed that by setting this property you will get
		/// range with <see cref="Count"/> iterations - because of floating-point
		/// numbers precision, needed range step cannot be rightly evaluated
		/// for any setted <see cref="Count"/> value.</remarks>
		public int Count { get { return this.count; } }

		/// <summary>
		/// Gets the value indicating when this instance
		/// is valid for iteration over it.</summary>
		/// <returns><b>true</b> if range is valid,
		/// otherwise <b>false</b></returns>
		public bool IsValid { get { return this.valid == 0; } }

		internal int ValidCount
		{
			get
			{
				if (this.valid != 0) Validate();
				return this.count;
			}
		}

		#endregion
		#region Methods

		/// <summary>
		/// Sets the begin value of the current range instance.</summary>
		/// <param name="value">New beginning value.</param>
		/// <returns>A new <see cref="ILCalc.ValueRange"/> whose begin
		/// equals <paramref name="value"/>.</returns>
		public ValueRange SetBegin(double value)
		{
			return new ValueRange(
				value, this.end, this.step);
		}

		public ValueRange SetEnd(double value)
		{
			return new ValueRange(
				this.begin, value, this.step);
		}

		public ValueRange SetStep(double value)
		{
			return new ValueRange(
				this.begin, this.end, value);
		}

		public ValueRange SetCount(int value)
		{
			return new ValueRange(
				this.begin,
				this.end,
				InternalSetCount(value));
		}

		public static ValueRange FromCount(double begin, double end, int count)
		{
			if (count <= 0)
				throw new ArgumentOutOfRangeException("count");

			return new ValueRange(begin, end, count, true);
		}

		/// <summary>
		/// Converts the values of this range to its
		/// equivalent string representation.</summary>
		/// <returns>Expression string.</returns>
		public override string ToString()
		{
			var buf = new StringBuilder();
			buf
				.Append(this.begin)
				.Append(" - ").Append(this.end)
				.Append(" : ").Append(this.step);

			return buf.ToString();
		}

		#endregion
		#region Count

		private double InternalSetCount(int newCount)
		{
			return (this.end - this.begin) / newCount;
		}

		private int InternalGetCount()
		{
			double len = this.end - this.begin;
			int rcount = (int) (len / this.step) + 1;

			if (len % this.step == 0)
			{
				rcount--;
			}

			return rcount;
		}

		#endregion
		#region Validate

		private InvalidateReason InternalValidate()
		{
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

			return InvalidateReason.None;
		}

		private enum InvalidateReason : byte
		{
			None = 0,
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
			string msg = null;
			switch (this.valid)
			{
				case InvalidateReason.None: return;
				case InvalidateReason.NotFiniteRange: msg = Resource.errRangeNotFinite; break;
				case InvalidateReason.EndlessRange:   msg = Resource.errEndlessLoop;    break;
				case InvalidateReason.WrongStepSign:  msg = Resource.errWrongStepSign;  break;
				case InvalidateReason.RangeTooLoong:  msg = Resource.errTooLongRange;   break;
			}

			throw new InvalidRangeException(msg);
		}

		#endregion
		#region Equality

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
		/// Indicates whether the current <see cref="ILCalc.ValueRange"/> is
		/// equal to another <see cref="ILCalc.ValueRange"/> structure.</summary>
		/// <overloads>Returns a value indicating whether two instances
		/// of the <see cref="ILCalc.ValueRange"/> is equal.</overloads>
		/// <param name="other">An another <see cref="ILCalc.ValueRange"/> to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="ILCalc.ValueRange"/> is equal to
		/// the other <see cref="ILCalc.ValueRange"/>; otherwise, <b>false</b>.</returns>
		public bool Equals(ValueRange other)
		{
			return this.begin.Equals(other.begin)
			    && this.step.Equals(other.step)
			    && this.end.Equals(other.end);
		}

		/// <summary>
		/// Indicates whether the current <see cref="ILCalc.ValueRange"/>
		/// is equal to another object.</summary>
		/// <param name="obj">An another <see cref="object"/> to compare with.</param>
		/// <returns><b>true</b> if the current <see cref="ILCalc.ValueRange"/> is equal to
		/// the other <see cref="ILCalc.ValueRange"/>; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			return obj is ValueRange
				&& Equals((ValueRange) obj);
		}

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="ILCalc.ValueRange"/> are equal.</summary>
		/// <param name="a">First <see cref="ILCalc.ValueRange"/>.</param>
		/// <param name="b">Second <see cref="ILCalc.ValueRange"/>.</param>
		/// <returns><b>true</b> if <paramref name="a"/> and <paramref name="b"/>
		/// are equal; otherwise, <b>false</b>.</returns>
		public static bool operator ==(ValueRange a, ValueRange b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Returns a value indicating whether two instances
		/// of <see cref="ILCalc.ValueRange"/> are not equal.</summary>
		/// <param name="a">First <see cref="ILCalc.ValueRange"/>.</param>
		/// <param name="b">Second <see cref="ILCalc.ValueRange"/>.</param>
		/// <returns><b>true</b> if <paramref name="a"/> and  <paramref name="b"/>
		/// are not equal; otherwise, <b>false</b>.</returns>
		public static bool operator !=(ValueRange a, ValueRange b)
		{
			return !a.Equals(b);
		}

		#endregion
	}
}