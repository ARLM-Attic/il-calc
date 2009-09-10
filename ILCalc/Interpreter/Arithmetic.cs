using System;
using System.Diagnostics;
using ILCalc.Custom;

namespace ILCalc
{
  static class Arithmetics
  {
    #region Resolve

    static readonly SupportCollection<Type> Support;
    static readonly SupportCollection<Type> Checked;

    static Arithmetics()
    {
      Support = new SupportCollection<Type>();
      Support.Add<Int32>(typeof(Int32Arithmetic));
      Support.Add<Int64>(typeof(Int64Arithmetics));
      Support.Add<Single>(typeof(SingleArithmetic));
      Support.Add<Double>(typeof(DoubleArithmetic));
      Support.Add<Decimal>(typeof(DecimalArithmetic));

      Checked = new SupportCollection<Type>();
      //Checked.Add<Int32>(typeof(Int32CheckedArithmetic));
    }

    public static Type Resolve<T>(bool useChecks)
    {
      Type arithmetic = useChecks ?
        Checked.Find<T>() :
        Support.Find<T>();

      if (arithmetic == null)
      {
        return typeof(UnknownArithmetic<>)
          .MakeGenericType(typeof(T));
      }

      return arithmetic;
    }

    #endregion
    #region Built-in

    struct Int32Arithmetic : IArithmetic<Int32>
    {
      public int Zero { get { return 0; } }
      public int One  { get { return 1; } }

      public int Neg(int x) { return -x; }
      public int Add(int x, int y) { return x + y; }
      public int Sub(int x, int y) { return x - y; }
      public int Mul(int x, int y) { return x * y; }
      public int Div(int x, int y) { return x / y; }
      public int Mod(int x, int y) { return x % y; }

      //TODO: is this right?
      //TODO: write test
      public int Pow(int x, int y)
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

      public bool IsEqual(int x, int y)     { return x == y; }
      public bool IsGreather(int x, int y)  { return x >  y; }
      public bool IsGrOrEqual(int x, int y) { return x >= y; }
      public int? IsIntergal(int value)     { return value;  }
    }

    struct Int32CheckedArithmetic : IArithmetic<Int32>
    {
      public int Zero { get { return 0; } }
      public int One  { get { return 1; } }

      public int Neg(int x) { return -x; }
      public int Add(int x, int y) { return checked(x + y); }
      public int Sub(int x, int y) { return checked(x - y); }
      public int Mul(int x, int y) { return checked(x * y); }
      public int Div(int x, int y) { return checked(x / y); }
      public int Mod(int x, int y) { return checked(x % y); }

      public int Pow(int x, int y)
      {
        if (y  < 0) return 0;
        if (y == 0) return 1;

        int t = x;
        checked //TODO: think
        {
          for(int i = 0; i < y; i++) t *= x;
        }

        return t;
      }

      public bool IsEqual(int x, int y)     { return x == y; }
      public bool IsGreather(int x, int y)  { return x >  y; }
      public bool IsGrOrEqual(int x, int y) { return x >= y; }
      public int? IsIntergal(int value)     { return value; }
    }

    struct Int64Arithmetics : IArithmetic<Int64>
    {
      public long Zero { get { return 0; } }
      public long One  { get { return 1; } }

      public long Neg(long x) { return -x; }
      public long Add(long x, long y) { return x + y; }
      public long Sub(long x, long y) { return x - y; }
      public long Mul(long x, long y) { return x * y; }
      public long Div(long x, long y) { return x / y; }
      public long Mod(long x, long y) { return x % y; }

      //TODO: test
      public long Pow(long x, long y)
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

      public bool IsEqual(long x, long y)     { return x == y; }
      public bool IsGreather(long x, long y)  { return x >  y; }
      public bool IsGrOrEqual(long x, long y) { return x >= y; }

      public int? IsIntergal(long value)
      {
        if (int.MinValue >= value && value <= int.MaxValue)
          return (int) value;
        return null;
      }
    }

    struct SingleArithmetic : IArithmetic<Single>
    {
      public float Zero { get { return 0.0f; } }
      public float One  { get { return 1.0f; } }

      public float Neg(float x) { return -x; }
      public float Add(float x, float y) { return x + y; }
      public float Sub(float x, float y) { return x - y; }
      public float Mul(float x, float y) { return x * y; }
      public float Div(float x, float y) { return x / y; }
      public float Mod(float x, float y) { return x % y; }
      public float Pow(float x, float y)
      {
        return (float) Math.Pow(x, y);
      }

      public bool IsEqual(float x, float y) { return x == y; }
      public bool IsGreather(float x, float y) { return x > y; }
      public bool IsGrOrEqual(float x, float y) { return x >= y; }

      public int? IsIntergal(float value)
      {
        var xint = (int) value;
        if (xint == value) return xint;
        return null;
      }
    }

    struct DoubleArithmetic : IArithmetic<Double>
    {
      public double Zero { get { return 0.0; } }
      public double One  { get { return 1.0; } }

      public double Neg(double x) { return -x; }
      public double Add(double x, double y) { return x + y; }
      public double Sub(double x, double y) { return x - y; }
      public double Mul(double x, double y) { return x * y; }
      public double Div(double x, double y) { return x / y; }
      public double Mod(double x, double y) { return x % y; }
      public double Pow(double x, double y) { return Math.Pow(x, y); }

      public bool IsEqual(double x, double y) { return x == y; }
      public bool IsGreather(double x, double y) { return x > y; }
      public bool IsGrOrEqual(double x, double y) { return x >= y; }

      public int? IsIntergal(double value)
      {
        var xint = (int) value;
        if (xint == value) return xint;
        return null;
      }
    }

    struct DecimalArithmetic : IArithmetic<Decimal>
    {
      public decimal Zero { get { return 0M; } }
      public decimal One  { get { return 1M; } }

      public decimal Neg(decimal x) { return -x; }
      public decimal Add(decimal x, decimal y) { return x + y; }
      public decimal Sub(decimal x, decimal y) { return x - y; }
      public decimal Mul(decimal x, decimal y) { return x * y; }
      public decimal Div(decimal x, decimal y) { return x / y; }
      public decimal Mod(decimal x, decimal y) { return x % y; }
      public decimal Pow(decimal x, decimal y)
      {
        return (decimal) Math.Pow((double) x, (double) y);
      }

      public bool IsEqual(decimal x, decimal y)     { return x == y; }
      public bool IsGreather(decimal x, decimal y)  { return x >  y; }
      public bool IsGrOrEqual(decimal x, decimal y) { return x >= y; }

      public int? IsIntergal(decimal value)
      {
        var integral = (int) value;
        if (value == integral) return integral;
        return null;
      }
    }

    sealed class UnknownArithmetic<T> : IArithmetic<T>
    {
      static Exception MakeException(string name)
      {
        Debug.Assert(name != null);

        return new NotSupportedException(string.Format(
          Resource.errNotSupported, name, typeof(T)));
      }

      public T Zero { get { throw MakeException("get_Zero"); } }
      public T One  { get { throw MakeException("get_One");  } }

      public T Neg(T x)      { throw MakeException("-"); }
      public T Add(T x, T y) { throw MakeException("+"); }
      public T Sub(T x, T y) { throw MakeException("-"); }
      public T Mul(T x, T y) { throw MakeException("*"); }
      public T Div(T x, T y) { throw MakeException("/"); }
      public T Mod(T x, T y) { throw MakeException("%"); }
      public T Pow(T x, T y) { throw MakeException("^"); }

      public bool IsEqual(T x, T y)     { throw MakeException("^"); }
      public bool IsGreather(T x, T y)  { throw MakeException(">"); }
      public bool IsGrOrEqual(T x, T y) { throw MakeException(">="); }

      public int? IsIntergal(T value) { return null; }
    }

    #endregion
  }
}
