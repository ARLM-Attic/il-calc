using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ILCalc
{
  static class StringBuilderExtensions
  {
    static readonly CultureInfo Current = CultureInfo.CurrentCulture;

    public static void AppendFormat(
      this StringBuilder builder, string format, object arg)
    {
      builder.AppendFormat(Current, format, arg);
    }
  }
}

namespace System.Runtime.CompilerServices
{
  [Conditional("NEVER")]
  [AttributeUsage(AttributeTargets.All, Inherited = true)]
  sealed class CompilerGeneratedAttribute : Attribute { }
}