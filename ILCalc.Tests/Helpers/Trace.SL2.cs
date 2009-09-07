namespace System.Diagnostics
{
  public static class Trace
  {
    [Conditional("TRACE")]
    public static void WriteLine(object value) { }

    [Conditional("TRACE")]
    public static void WriteLine(string message) { }

    [Conditional("TRACE")]
    public static void WriteLine(object value, string category) { }

    [Conditional("TRACE")]
    public static void WriteLine(string message, string category) { }

    [Conditional("TRACE")]
    public static void Indent() { }

    [Conditional("TRACE")]
    public static void Unindent() { }
  }
}
