using System;
using System.Globalization;

namespace ILCalc.Custom
{
  interface IParserSupport<T>
  {
    string Expression { get; }
    int BeginPos { get; }
    char DecimalDot { get; }
    NumberFormatInfo NumberFormat { get; }

    T ParsedValue { set; }

    Exception InvalidNumberFormat(
      string message,
      string badLiteral,
      Exception innerException);
  }

  interface ILiteralParser<T>
  {
    int TryParse(int i, IParserSupport<T> p);
  }
}
