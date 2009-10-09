using System;

namespace ILCalc.Custom
{
  #if SILVERLIGHT
  /// <summary>
  /// Static class for helper methods.<br/>
  /// This class only public in Silverlight build.
  /// </summary>
  public
#endif
  static class MathHelper
  {
    /// <summary>
    /// Returns a specified number raised to the specified power.</summary>
    /// <param name="x">A 32-bit signed integer to be raised to a power.</param>
    /// <param name="y">A 32-bit signed integer that specifies a power.</param>
    /// <returns>The number x raised to the power y.</returns>
    public static int Pow(int x, int y)
    {
      if (y < 0) return 0;

      int res = 1;
      while (y != 0)
      {
        if ((y & 1) == 1) res *= x;
        y >>= 1;
        x *= x;
      }

      return res;
    }

    /// <summary>
    /// Returns a specified number raised to the specified power.</summary>
    /// <param name="x">A 64-bit signed integer to be raised to a power.</param>
    /// <param name="y">A 64-bit signed integer that specifies a power.</param>
    /// <returns>The number x raised to the power y.</returns>
    public static long Pow(long x, long y)
    {
      if (y < 0) return 0;

      long res = 1;
      while (y != 0)
      {
        if ((y & 1) == 1) res *= x;
        y >>= 1;
        x *= x;
      }

      return res;
    }

    /// <summary>
    /// Returns a specified number raised to the specified power.</summary>
    /// <param name="x">A single-precision floating-
    /// point number to be raised to a power.</param>
    /// <param name="y">A single-precision floating-
    /// point number that specifies a power.</param>
    /// <returns>The number x raised to the power y.</returns>
    public static float Pow(float x, float y)
    {
      return (float) Math.Pow(x, y);
    }

    /// <summary>
    /// Returns a specified number raised to the specified power.</summary>
    /// <param name="x">A <see cref="Decimal"/> number to be raised to a power.</param>
    /// <param name="y">A <see cref="Decimal"/> number that specifies a power.</param>
    /// <returns>The number x raised to the power y.</returns>
    public static decimal Pow(decimal x, decimal y)
    {
      if (y == Decimal.Floor(y) && Math.Abs(y) < 1000m)
      {
        decimal res = 1m;
        bool sign = y < 0m;
        y = Math.Abs(y);

        while (y > 0)
        {
          res *= x;
          y--;
        }

        return sign ? 1m / res : res;
      }
      
      return (decimal)
        Math.Pow((double) x, (double) y);
    }

    /// <summary>
    /// Returns a specified number raised to the specified
    /// power with overflow checking.</summary>
    /// <param name="x">A 32-bit signed integer to be raised to a power.</param>
    /// <param name="y">A 32-bit signed integer that specifies a power.</param>
    /// <exception cref="OverflowException">
    /// Arithmetic operations results in an overflow.</exception>
    /// <returns>The number x raised to the power y.</returns>
    public static int PowChecked(int x, int y)
    {
      if (y < 0) return 0;

      int res = 1;
      while (y != 0)
      {
        if ((y & 1) == 1)
          checked { res *= x; }

        y >>= 1;
        if (y != 0)
          checked { x *= x; }
      }

      return res;
    }

    /// <summary>
    /// Returns a specified number raised to the specified power.</summary>
    /// <param name="x">A 64-bit signed integer to be raised to a power.</param>
    /// <param name="y">A 64-bit signed integer that specifies a power.</param>
    /// <exception cref="OverflowException">
    /// Arithmetic operations results in an overflow.</exception>
    /// <returns>The number x raised to the power y.</returns>
    public static long PowChecked(long x, long y)
    {
      if (y < 0) return 0;

      long res = 1;
      while (y != 0)
      {
        if ((y & 1) == 1)
          checked { res *= x; }

        y >>= 1;
        if (y != 0)
          checked { x *= x; }
      }

      return res;
    }

    /// <summary>
    /// Returns a specified number raised to the specified
    /// power with overflow checking.</summary>
    /// <param name="x">A single-precision floating-
    /// point number to be raised to a power.</param>
    /// <param name="y">A single-precision floating-
    /// point number that specifies a power.</param>
    /// <exception cref="OverflowException">
    /// Arithmetic operations results in an overflow.</exception>
    /// <returns>The number x raised to the power y.</returns>
    public static float PowChecked(float x, float y)
    {
      var temp = (float) Math.Pow(x, y);
      if (float.IsInfinity(temp) || float.IsNaN(temp))
      {
        throw new NotFiniteNumberException(temp.ToString());
      }

      return temp;
    }
  }
}
