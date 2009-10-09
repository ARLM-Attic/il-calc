using System;
using System.Globalization;
using System.Collections.Generic;

namespace ILCalc.Tests
{
  static class CultureHelper
  {
#if SILVERLIGHT

    public static IEnumerable<CultureInfo> GetCultures()
    {
      yield return CultureInfo.CurrentCulture;
      yield return CultureInfo.CurrentUICulture;
      yield return CultureInfo.InvariantCulture;
    }

#else

    public static IEnumerable<CultureInfo> GetCultures()
    {
      CultureInfo c;
      for (int id = 1000; id < 22000; id++)
      {
        try
        {
          c = CultureInfo.GetCultureInfo(id);
        }
        catch(ArgumentException) { continue; }
        if (c.IsNeutralCulture)  { continue; }

        yield return c;
      }
    }

#endif
  }
}