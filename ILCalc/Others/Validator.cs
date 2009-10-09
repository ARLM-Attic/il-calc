using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILCalc
{
  static class Validator
  {
    public static void CheckName(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException(
          Resource.errIdentifierEmpty);
      }

      char first = name[0];
      if (!char.IsLetter(first) && first != '_')
      {
        throw InvalidFirstSymbol(name, first);
      }

      for (int i = 1; i < name.Length; i++)
      {
        char c = name[i];
        if (!char.IsLetterOrDigit(c) && c != '_')
        {
          throw new ArgumentException(
            string.Format(
              Resource.errIdentifierSymbol, c, name));
        }
      }
    }

    [DebuggerHidden]
    static Exception InvalidFirstSymbol(string name, char first)
    {
      var buf = new StringBuilder();
      buf.AppendFormat(
        Resource.errIdentifierStartsWith, name);

      if (first == '<')
      {
        buf.Append(' ')
           .Append(Resource.errIdentifierFromLambda);
      }

      return new ArgumentException(buf.ToString());
    }

    [Conditional("SILVERLIGHT")]
    public static void CheckVisible(Type type)
    {
      Debug.Assert(type != null);

      if (!type.IsVisible)
      {
        throw new ArgumentException(string.Format(
          Resource.errTypeNonPublic, type.FullName));
      }
    }

    [Conditional("SILVERLIGHT")]
    public static void CheckVisible(
      System.Reflection.MethodInfo method)
    {
      Debug.Assert(method != null);

      if (!method.IsPublic)
      {
        throw new ArgumentException(string.Format(
          Resource.errMethodNonPublic, method));
      }

      if (method.DeclaringType != null)
      {
        CheckVisible(method.DeclaringType);
      }
    }
  }

  interface IListEnumerable
  {
    List<string>.Enumerator GetEnumerator();
  }
}