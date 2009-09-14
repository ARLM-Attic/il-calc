using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILCalc
{
  sealed partial class Parser<T>
  {
    Exception IncorrectConstr(Item prev, Item next, int i)
    {
      int len = i - this.prePos;
      var buf = new StringBuilder(Resource.errIncorrectConstr);

      Debug.Assert(len >= 0);

      buf.Append(" (");
      buf.Append(prev);
      buf.Append(")(");
      buf.Append(next);
      buf.Append("): \"");
      buf.Append(this.expr, this.prePos, len);
      buf.Append("\".");

      return new SyntaxException(
        buf.ToString(), this.expr, this.prePos, len);
    }

    Exception BraceDisbalance(int pos, bool mode)
    {
      string message = mode ?
        Resource.errDisbalanceOpen :
        Resource.errDisbalanceClose;

      return new SyntaxException(message, this.expr, pos, 1);
    }

    Exception IncorrectIden(int i)
    {
      for (i++; i < this.xlen; i++)
      {
        char c = this.expr[i];
        if (!char.IsLetterOrDigit(c) && c != '_') break;
      }

      return IncorrectConstr(
        Item.Identifier, Item.Identifier, i);
    }

    //TODO: unused?
    Exception NumberFormatException(
      string message, string text, Exception inner)
    {
      var buf = new StringBuilder(message);

      buf.Append(" \"");
      buf.Append(text);
      buf.Append("\".");

      return new SyntaxException(
        buf.ToString(), this.expr, this.curPos, text.Length, inner);
    }

    Exception NoOpenBrace(int pos, int len)
    {
      var buf = new StringBuilder(Resource.errFunctionNoBrace);

      buf.Append(" \"");
      buf.Append(this.expr, pos, len);
      buf.Append("\".");

      return new SyntaxException(
        buf.ToString(), this.expr, pos, len);
    }

    Exception InvalidSeparator()
    {
      return new SyntaxException(
        Resource.errInvalidSeparator, this.expr, this.curPos, 1);
    }

    Exception UnresolvedIdentifier(int shift)
    {
      int end = this.curPos;
      for (end += shift; end < this.xlen; end++)
      {
        char c = this.expr[end];
        if (!Char.IsLetterOrDigit(c) && c != '_') break;
      }

      var buf = new StringBuilder(Resource.errUnresolvedIdentifier);
      int len = end - this.curPos;

      buf.Append(" \"");
      buf.Append(this.expr, this.curPos, len);
      buf.Append("\".");

      return new SyntaxException(
        buf.ToString(), this.expr, this.curPos, len);
    }

    Exception UnresolvedSymbol(int i)
    {
      var buf = new StringBuilder(Resource.errUnresolvedSymbol);

      buf.Append(" '");
      buf.Append(this.expr[i]);
      buf.Append("'.");

      if (LiteralParser.IsUnknown(Literal))
      {
        buf.Append(string.Format(
          Resource.errParseLiteralHint, typeof(T)));
      }

      return new SyntaxException(
        buf.ToString(), this.expr, i, 1);
    }

    Exception WrongArgsCount(
      int pos, int len, int args, FunctionGroup<T> group)
    {
      var buf = new StringBuilder(Resource.sFunction);

      buf.Append(" \"");
      buf.Append(this.expr, pos, len);
      buf.Append("\" ");
      buf.AppendFormat(Resource.errWrongOverload, args);

      // NOTE: improve this?
      // NOTE: may be empty FunctionGroup! Show actual message
      // NOTE: FunctionGroup => IEnumerable<FunctionInfo>
      if (group != null)
      {
        buf.Append(' ');
        buf.AppendFormat(
          Resource.errExistOverload,
          group.MakeMethodsArgsList());
      }

      return new SyntaxException(
        buf.ToString(), this.expr, pos, len);
    }

    Exception AmbiguousMatchException(int pos, List<Capture> matches)
    {
      Debug.Assert(matches != null);
      Debug.Assert(matches.Count > 0);

      var names = new List<string>(matches.Count);

      foreach (Capture match in matches)
      {
        IdenType idenType = IdenType.Argument;
        foreach (var list in Context.Literals)
        {
          if (idenType == match.Type)
          {
            int i = 0, id = match.Index;
            foreach (string name in list)
            {
              if (i++ == id) { names.Add(name); break; }
            }
          }

          idenType++;
        }
      }

      Debug.Assert(matches.Count == names.Count);

      var buf = new StringBuilder(Resource.errAmbiguousMatch);
      for (int i = 0; i < matches.Count; i++)
      {
        string type = string.Empty;

        switch (matches[i].Type)
        {
          case IdenType.Argument: type = Resource.sArgument; break;
          case IdenType.Constant: type = Resource.sConstant; break;
          case IdenType.Function: type = Resource.sFunction; break;
        }

        buf.Append(' ');
        buf.Append(type.ToLowerInvariant());
        buf.Append(" \"");
        buf.Append(names[i]);
        buf.Append('\"');

        if (i + 1 == matches.Count)
        {
          buf.Append(' ');
          buf.Append(Resource.sAnd);
        }
        else buf.Append(i == matches.Count ? '.' : ',');
      }

      int len = names[0].Length;
      return new SyntaxException(
        buf.ToString(), this.expr, pos, len);
    }
  }
}