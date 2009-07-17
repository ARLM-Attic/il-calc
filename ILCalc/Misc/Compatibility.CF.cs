using System;
using System.Diagnostics;

namespace System.Runtime.CompilerServices
{
	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Method |
		AttributeTargets.Class  |
		AttributeTargets.Assembly)]
	internal sealed class ExtensionAttribute : Attribute
	{
	}
}

namespace ILCalc
{
	internal enum DebuggerBrowsableState
	{
		Never = 0,
		Collapsed = 2,
		RootHidden = 3
	}

	internal static class StringExtensions
	{
		public static string ToLowerInvariant(this string value)
		{
			return value.ToLower();
		}
	}

	// ReSharper disable UnusedParameter.Local
	// ReSharper disable UnusedParameter.Global
	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Field |
		AttributeTargets.Property,
		AllowMultiple = false)]
	internal sealed class DebuggerBrowsableAttribute : Attribute
	{
		public DebuggerBrowsableAttribute(DebuggerBrowsableState state)
		{
		}
	}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Delegate | AttributeTargets.Field |
		AttributeTargets.Property | AttributeTargets.Enum  |
		AttributeTargets.Struct   | AttributeTargets.Class |
		AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class DebuggerDisplayAttribute : Attribute
	{
		public DebuggerDisplayAttribute(string value)
		{
		}

		public string Name { get; set; }
	}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Struct | AttributeTargets.Class |
		AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class DebuggerTypeProxyAttribute : Attribute
	{
		public DebuggerTypeProxyAttribute(Type type)
		{
		}
	}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Delegate | AttributeTargets.Enum |
		AttributeTargets.Struct   | AttributeTargets.Class,
		Inherited = false)]
	internal sealed class SerializableAttribute : Attribute
	{
	}

	// ReSharper restore UnusedParameter.Global
	// ReSharper restore UnusedParameter.Local
}