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

	enum DebuggerBrowsableState
		{
		Collapsed = 2,
		Never = 0,
		RootHidden = 3
		}

	// ReSharper disable UnusedParameter.Local

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Property,
		AllowMultiple = false)]
	sealed class DebuggerBrowsableAttribute : Attribute
		{
		public DebuggerBrowsableAttribute( DebuggerBrowsableState state ) { }
		}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Delegate | AttributeTargets.Field |
		AttributeTargets.Property | AttributeTargets.Enum  |
		AttributeTargets.Struct   | AttributeTargets.Class |
		AttributeTargets.Assembly, AllowMultiple = true)]
	sealed class DebuggerDisplayAttribute : Attribute
		{
		public DebuggerDisplayAttribute( string value ) { }
		public string Name { get; set; }
		}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Struct | AttributeTargets.Class |
		AttributeTargets.Assembly, AllowMultiple = true)]
	sealed class DebuggerTypeProxyAttribute : Attribute
		{
		public DebuggerTypeProxyAttribute( string typeName ) { }
		public DebuggerTypeProxyAttribute( Type type ) { }
		}

	[Conditional("NEVER")]
	[AttributeUsage(
		AttributeTargets.Delegate | AttributeTargets.Enum |
		AttributeTargets.Struct | AttributeTargets.Class,
		Inherited = false)]
	sealed class SerializableAttribute : Attribute
		{
		}

	// ReSharper restore UnusedParameter.Local
	}