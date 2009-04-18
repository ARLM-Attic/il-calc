using System;
using System.Diagnostics;

namespace ILCalc
	{
	static class StringExtension
		{
		public static string ToLowerInvariant( this String value )
			{
			return value.ToLower( );
			}
		}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Delegate | AttributeTargets.Enum |
		AttributeTargets.Struct   | AttributeTargets.Class,
		Inherited = false)]
	sealed class SerializableAttribute : Attribute
		{
		}

	[Conditional("NEVER")]
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	sealed class NonSerializedAttribute : Attribute
		{
		}
	}
