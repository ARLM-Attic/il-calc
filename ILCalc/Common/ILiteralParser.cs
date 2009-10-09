using System;
using System.Globalization;

namespace ILCalc.Custom
{
  interface IParserSupport<T>
  {
    string Expression { get; }

    int BeginPos { get; }

    char DecimalDot { get; } // => DecimalSeparator
    NumberFormatInfo NumberFormat { get; }

    T ParsedValue { set; }
    bool DiscardNegate();

    Exception InvalidNumberFormat(
      string message, string literal, Exception exc);
  }

  interface ILiteralParser<T>
  {
    int TryParse(int i, IParserSupport<T> p);
  }
}
