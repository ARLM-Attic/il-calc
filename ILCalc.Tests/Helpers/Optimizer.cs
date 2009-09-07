using System.Collections;
using System.Collections.Generic;

namespace ILCalc.Tests
{
  public class Optimizer
  {
    public static Enumerator Modes
    {
      get { return new Enumerator(); }
    }

    public struct Enumerator
      : IEnumerable<OptimizeModes>,
        IEnumerator<OptimizeModes>
    {
      const int EnumLast = (int) OptimizeModes.PerformAll + 1;
      int i;

      public OptimizeModes Current
      {
        get { return (OptimizeModes) this.i; }
      }

      object IEnumerator.Current
      {
        get { return (OptimizeModes) this.i; }
      }

      public void Dispose() { }

      public void Reset() { this.i = 0; }

      public bool MoveNext()
      {
        if (this.i < EnumLast)
        {
          this.i++;
          return true;
        }

        return false;
      }

      public IEnumerator<OptimizeModes> GetEnumerator() { return this; }

      IEnumerator IEnumerable.GetEnumerator() { return this; }
    }
  }
}