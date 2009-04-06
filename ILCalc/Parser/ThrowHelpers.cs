using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;

namespace ILCalc
	{
	sealed partial class Parser
		{
		[DebuggerHidden]
		private SyntaxException IncorrectConstr( Item prev, Item next, int i )
			{
			int len = i - _prePos;
			var buf = new StringBuilder(Resources.errIncorrectConstr);

			buf.Append(" ("); buf.Append(prev.ToString( ).ToLowerInvariant( ));
			buf.Append(")("); buf.Append(next.ToString( ).ToLowerInvariant( ));
			buf.Append("): \"");
			buf.Append(_expr, _prePos, len);
			buf.Append("\".");

			return new SyntaxException(buf.ToString( ), _expr, _prePos, len);
			}

		[DebuggerHidden]
		private SyntaxException BraceDisbalance( int pos, bool mode )
			{
			return new SyntaxException(mode?
				Resources.errDisbalanceOpen:
				Resources.errDisbalanceClose,
				_expr, pos, 1);
			}

		[DebuggerHidden]
		private SyntaxException IncorrectIden( int i )
			{
			for(i++; i < _len; i++)
				{
				char c = _expr[i];
				if(!Char.IsLetterOrDigit(c) && c != '_') break;
				}

			return IncorrectConstr(Item.Identifier, Item.Identifier, i);
			}

		[DebuggerHidden]
		private SyntaxException NumberFormat( string str, Exception inner )
			{
			var buf = new StringBuilder(Resources.errNumberFormat);

			buf.Append(" \"");
			buf.Append(str);
			buf.Append('\"');

			return new SyntaxException(
				buf.ToString( ),
				_expr, _curPos, str.Length, inner
				);
			}

		[DebuggerHidden]
		private SyntaxException NumberOverflow( string str, Exception inner )
			{
			var buf = new StringBuilder(Resources.errNumberOverflow);

			buf.Append(" \"");
			buf.Append(str);
			buf.Append('\"');

			return new SyntaxException(
				buf.ToString( ),
				_expr, _curPos, str.Length, inner
				);
			}

		[DebuggerHidden]
		private SyntaxException NoOpenBrace( int pos, int len )
			{
			var buf = new StringBuilder(Resources.errFunctionNoBrace);

			buf.Append(" \"");
			buf.Append(_expr, pos, len);
			buf.Append("\".");

			return new SyntaxException(buf.ToString(), _expr, pos, len);
			}

		[DebuggerHidden]
		private SyntaxException InvalidSeparator( )
			{
			return new SyntaxException(
				Resources.errInvalidSeparator,
				_expr, _curPos, 1);
			}

		[DebuggerHidden]
		private SyntaxException UnresolvedIdentifier( int shift )
			{
			int end = _curPos;
			for(end += shift; end < _len; end++)
				{
				char c = _expr[end];
				if(!Char.IsLetterOrDigit(c) && c != '_') break;
				}

			var buf = new StringBuilder(Resources.errUnresolvedIdentifier);
			int len = end - _curPos;
				
			buf.Append(" \"");
			buf.Append(_expr, _curPos, len);
			buf.Append("\".");

			return new SyntaxException(buf.ToString(), _expr, _curPos, len);
			}

		[DebuggerHidden]
		private SyntaxException UnresolvedSymbol( int i )
			{
			var buf = new StringBuilder(Resources.errUnresolvedSymbol);

			buf.Append(" '");
			buf.Append(_expr[i]);
			buf.Append("'.");

			return new SyntaxException(buf.ToString( ), _expr, i, 1);
			}

		[DebuggerHidden]
		private SyntaxException WrongArgsCount( int args, int pos, int len,
												MethodGroup method )
			{
			var buf = new StringBuilder(Resources.sFunction);

			buf.Append(" \"");
			buf.Append(_expr, pos, len);
			buf.Append("\" ");

			buf.AppendFormat(Resources.errWrongOverload, args);
			
			//TODO: always false?
			if( method != null )
				{
				buf.Append(' ');
				buf.AppendFormat(
					Resources.errExistOverload,
					method.OverloadsList( )
					);
				}

			return new SyntaxException(buf.ToString(), _expr, pos, len);
			}

		[DebuggerHidden]
		private SyntaxException AmbiguousMatch( int pos, ICollection<Capture> matches )
			{
			var names = new List<string>(matches.Count);

			foreach( Capture match in matches )
			foreach( SearchItem list in _idens )
				{
				if( list.Type != match.Type ) continue;

				int i = 0, id = match.Index;
				foreach( string name in list.Names )
					{
					if( i++ == id )
						{
						names.Add(name);
						break;
						}
					}
				}

			var buf = new StringBuilder(Resources.errAmbiguousMatch);
			int idx = 0, count = names.Count;

			foreach( Capture match in matches )
				{
				string type = String.Empty;
				switch( match.Type )
					{
					case Iden.Argument: type = Resources.sArgument; break;
					case Iden.Constant: type = Resources.sConstant; break;
					case Iden.Function: type = Resources.sFunction; break;
					}

				buf.Append(' ');
				buf.Append(type.ToLowerInvariant( ));
				buf.Append(" \"");
				buf.Append(names[idx++]);
				buf.Append('\"');

				if(idx + 1 == count)
					{
					buf.Append(' ');
					buf.Append(Resources.sAnd);
					}
				else buf.Append(idx == count? '.': ',');
				}

			int len = names[0].Length;
			return new SyntaxException(buf.ToString(), _expr, pos, len);
			}
		}
	}
