﻿using System.Globalization;
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
	[Serializable]
	[AttributeUsage(AttributeTargets.All, Inherited = true)]
	public sealed class CompilerGeneratedAttribute : Attribute
	{
	}
}