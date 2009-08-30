using System;
using System.Text;

namespace ILCalc
{
  static class Validator
  {
    public static void CheckName(string name)
    {
      // NOTE: perform nullness check in callers?

      if (string.IsNullOrEmpty(name))
        throw new ArgumentException(Resource.errIdentifierEmpty);

      char first = name[0];
      if (!char.IsLetter(first) && first != '_')
      {
        throw InvalidFirstSymbol(name, first);
      }

      for (int i = 1; i < name.Length; i++)
      {
        char ch = name[i];
        if (!char.IsLetterOrDigit(ch) && ch != '_')
        {
          throw new ArgumentException(string.Format(
            Resource.errIdentifierSymbol, ch, name));
        }
      }
    }

    static ArgumentException InvalidFirstSymbol(string name, char first)
    {
      var buf = new StringBuilder();
      buf.AppendFormat(Resource.errIdentifierStartsWith, name);

      if (first == '<')
      {
        buf.Append(' ')
           .Append(Resource.errIdentifierFromLambda);
      }

      return new ArgumentException(buf.ToString());
    }
  }
}