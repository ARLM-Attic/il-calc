using System;
using ILCalc.Custom;

namespace ILCalc
{
  /// <summary>
  /// Provides the static methods for creating
  /// <see cref="ValueRange{T}"/> instances.
  /// </summary>
  /// <remarks>This static class helps to create
  /// <see cref="ValueRange{T}"/> instances without
  /// specifying type parameter.</remarks>
  public static class ValueRange
  {
    #region Generics

    static readonly SupportCollection<object> Support;

    static ValueRange()
    {
      Support = new SupportCollection<object>(3);

      Support.Add<Double>(new DoubleRangeSupport());
      Support.Add<Int32>(new Int32RangeSupport());
      Support.Add<Decimal>(new DecimalRangeSupport());
    }

    internal static IRangeSupport<T> Resolve<T>()
    {
      object support = Support.Find<T>();
      if(support == null)
      {
        throw new NotSupportedException(
          "Type " + typeof(T) + " not supported " +
          "as ValueRange<T> type parameter.");
      }

      return (IRangeSupport<T>) support;
    }

    #endregion
    #region Methods

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ValueRange{T}"/> structure with the
    /// specified begin, end and count values.</summary>
    /// <param name="begin">Range begin value.</param>
    /// <param name="end">Range end value.</param>
    /// <param name="count">Range iterations count value.</param>
    /// <typeparam name="T">Range values type.</typeparam>
    /// <remarks>Is not guaranteed that by using this method
    /// you will get range with <paramref name="count"/> iterations -
    /// because of floating-point numbers precision,
    /// needed range step cannot be rightly evaluated
    /// for any <paramref name="count"/> value.</remarks>
    /// <summary>Returnds a new instance of the
    /// <see cref="ValueRange{T}"/> structure with the
    /// specified begin, end and count values.</summary>
    public static ValueRange<T> FromCount<T>(T begin, T end, int count)
    {
      if (count <= 0)
        throw new ArgumentOutOfRangeException("count");

      return new ValueRange<T>(begin, end, count);
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ValueRange{T}"/> structure with the
    /// specified begin, end and step values.</summary>
    /// <param name="begin">Range begin value.</param>
    /// <param name="end">Range end value.</param>
    /// <param name="step">Range step value.</param>
    /// <typeparam name="T">Range values type.</typeparam>
    /// <returns>Returns a new instance of the
    /// <see cref="ValueRange{T}"/> structure with the
    /// specified begin, end and step values.</returns>
    public static ValueRange<T> Create<T>(T begin, T end, T step)
    {
      return new ValueRange<T>(begin, end, step);
    }

    #endregion
    #region Supports

    sealed class DoubleRangeSupport : IRangeSupport<Double>
    {
      public double StepFromCount(
        ValueRange<double> r, int count)
      {
        return (r.End - r.Begin) / count;
      }

      public int GetCount(ValueRange<double> r)
      {
        double len = r.End - r.Begin;
        int rcount = (int) (len / r.Step) + 1;

        if (len % r.Step == 0) rcount--;
        return rcount;
      }

      public ValueRangeValidness Validate(ValueRange<double> r)
      {
        if (double.IsInfinity(r.Begin) || double.IsNaN(r.Begin)
         || double.IsInfinity(r.Step)  || double.IsNaN(r.Step)
         || double.IsInfinity(r.End)   || double.IsNaN(r.End))
        {
          return ValueRangeValidness.NotFiniteRange;
        }

        if (r.Begin + r.Step == r.Begin)
        {
          return ValueRangeValidness.EndlessRange;
        }

        if (r.Begin > r.End != r.Step < 0)
        {
          return ValueRangeValidness.WrongStepSign;
        }

        if ((r.End - r.Begin) / r.Step >= int.MaxValue)
        {
          return ValueRangeValidness.RangeTooLoong;
        }

        return ValueRangeValidness.Correct;
      }
    }

    sealed class Int32RangeSupport : IRangeSupport<Int32>
    {
      public int StepFromCount(
        ValueRange<int> r, int count)
      {
        return (r.End - r.Begin) / count;
      }

      public int GetCount(ValueRange<int> r)
      {
        // TODO: think!
        return (r.End - r.Begin) / r.Step;
      }

      public ValueRangeValidness Validate(ValueRange<int> r)
      {
        // TODO: think!!!!

        if (r.Step == 0)
        {
          return ValueRangeValidness.EndlessRange;
        }

        if (r.Begin > r.End != r.Step < 0)
        {
          return ValueRangeValidness.WrongStepSign;
        }

        return ValueRangeValidness.Correct;
      }
    }

    sealed class DecimalRangeSupport : IRangeSupport<Decimal>
    {
      public decimal StepFromCount(
        ValueRange<decimal> r, int count)
      {
        return (r.End - r.Begin) / count;
      }

      public int GetCount(ValueRange<decimal> r)
      {
        // TODO: is this correct for decimal?

        decimal len = r.End - r.Begin;
        int rcount = (int) (len / r.Step) + 1;

        if (len % r.Step == 0) rcount--;
        return rcount;
      }

      public ValueRangeValidness Validate(ValueRange<decimal> r)
      {
        if (r.Begin + r.Step == r.Begin)
        {
          return ValueRangeValidness.EndlessRange;
        }

        if (r.Begin > r.End != r.Step < 0)
        {
          return ValueRangeValidness.WrongStepSign;
        }

        if ((r.End - r.Begin) / r.Step >= int.MaxValue)
        {
          return ValueRangeValidness.RangeTooLoong;
        }

        return ValueRangeValidness.Correct;
      }
    }

    #endregion
  }
}
