using System;
using System.Diagnostics;

namespace System.Runtime.CompilerServices
{
  [Conditional("NEVER")]
  [AttributeUsage(
    AttributeTargets.Method |
    AttributeTargets.Class |
    AttributeTargets.Assembly)]
  sealed class ExtensionAttribute : Attribute { }
}

namespace ILCalc
{
  enum DebuggerBrowsableState
  {
    Never = 0,
    Collapsed = 2,
    RootHidden = 3
  }

  static class StringExtensions
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
    AttributeTargets.Property, AllowMultiple = false)]
  sealed class DebuggerBrowsableAttribute : Attribute
  {
    public DebuggerBrowsableAttribute(
      DebuggerBrowsableState state) { }
  }

  [Conditional("NEVER")]
  [AttributeUsage(
    AttributeTargets.Delegate | AttributeTargets.Field |
    AttributeTargets.Property | AttributeTargets.Enum  |
    AttributeTargets.Struct   | AttributeTargets.Class |
    AttributeTargets.Assembly, AllowMultiple = true)]
  sealed class DebuggerDisplayAttribute : Attribute
  {
    public DebuggerDisplayAttribute(string value) { }

    public string Name { get; set; }
  }

  [Conditional("NEVER")]
  [AttributeUsage(
    AttributeTargets.Struct |
    AttributeTargets.Class  |
    AttributeTargets.Assembly, AllowMultiple = true)]
  sealed class DebuggerTypeProxyAttribute : Attribute
  {
    public DebuggerTypeProxyAttribute(Type type) { }
  }

  [Conditional("NEVER")]
  [AttributeUsage(
    AttributeTargets.Delegate | AttributeTargets.Enum |
    AttributeTargets.Struct   | AttributeTargets.Class,
    Inherited = false)]
  sealed class SerializableAttribute : Attribute {}

  // ReSharper restore UnusedParameter.Global
  // ReSharper restore UnusedParameter.Local
}