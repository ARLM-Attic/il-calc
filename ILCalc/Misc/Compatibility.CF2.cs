using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ILCalc
{
	internal static class BuilderExtensions
	{
		private static readonly CultureInfo Current = CultureInfo.CurrentCulture;

		public static void AppendFormat(this StringBuilder builder, string format, object arg)
		{
			builder.AppendFormat(Current, format, arg);
		}
	}
}

namespace System.Runtime.CompilerServices
{
	[Conditional("NEVER")]
	[AttributeUsage(AttributeTargets.All, Inherited = true)]
	internal sealed class CompilerGeneratedAttribute : Attribute
	{
	}
}