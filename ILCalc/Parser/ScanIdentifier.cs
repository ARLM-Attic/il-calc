using System;
using System.Collections.Generic;
using System.Globalization;

namespace ILCalc
	{
	using ListOfItems = List< MethodGroup.Item >;

	sealed partial class Parser
		{
		private Item ScanIdenifier( ref int i )
			{
			int len = 0;

			var matches = ( _context._culture != null )?
				GetMatchesCulture(ref len):
				GetMatchesOrdinal(ref len);

			if( len == 0 ) throw UnresolvedIdentifier(1);

			if( _curPos + len < _len )
				{
				char c = _expr[_curPos + len];
				if( Char.IsLetterOrDigit(c) || c == '_' )
					{
					throw UnresolvedIdentifier(len + 1);
					}
				}

			i += len - 1;

			return (matches.Count == 1)?
				SimpleMatch(matches[0], ref i, len):
				AmbiguousMatch(matches, ref i, len);
			}

		#region Matches

		private Item SimpleMatch( Capture match, ref int i, int len )
			{
			switch( match.Type )
				{
				case Iden.Argument: // ===============================
					{
					_output.PutArgument(match.Index);
					return Item.Identifier;
					}

				case Iden.Constant: // ===============================
					{
					_output.PutNumber(_context._consts[match.Index]);
					return Item.Identifier;
					}

				case Iden.Function: // ===============================
					{
					int fPos = _curPos;
					if( !SkipBrace(ref i) )
						throw NoOpenBrace(fPos, len);

					MethodGroup methods = GetMethod(match);
					if( methods.HasParams )
						{
						IExpressionOutput old = _output;
						
						var buf = new BufferOutput( );
						
						_output = buf; int args = ParseNested(ref i, true);
						_output = old;

						var func = methods.GetParamsFunc(args);
						
						if( func != null )
							OutputBufferCall(buf, func, args);

						else throw WrongArgsCount(args, fPos, len, methods);
						}
					else
						{
						_output.BeginCall(-1, 0); // std call

						int args = ParseNested(ref i, true);
						var func = methods.GetStdFunc(args);

						if( func != null )
							_output.PutFunction(func.Func);

						else throw WrongArgsCount(args, fPos, len, methods);
						}

					return Item.End;
					}

				default: throw new NotSupportedException();
				}
			} 

		private Item AmbiguousMatch( ICollection<Capture> matches,
									 ref int i, int len )
			{
			#region Count Matches

			var matchFn = new List<Capture>( );
			var matchId = new List<Capture>( );

			foreach( Capture match in matches )
				{
				if( IsFunc(match) )
					 matchFn.Add(match);
				else matchId.Add(match);
				}
				
			#endregion
			#region Locals

			ListOfItems funcs;
			BufferOutput buf;
			int args, fPos = _curPos;

			#endregion
			
			//===================================== > 0 Identifiers ==
			if( matchFn.Count == 0 )
				{
				throw AmbiguousMatch(_curPos, matchId);
				}
			
			//======================================= > 0 Functions ==
			if( matchId.Count == 0 )
				{
				if( !SkipBrace(ref i) )
					throw NoOpenBrace(fPos, len);

				funcs = GetSuitableFunctions(matchFn, ref i, out buf, out args);

				if( funcs.Count == 1 )
					OutputBufferCall(buf, funcs[0], args);

				else if( funcs.Count == 0 )
					 throw WrongArgsCount(args, fPos, len, null);
				else throw AmbiguousMatch(fPos, matchFn);

				return Item.End;
				}
			
			//==================== > 0 Functions and > 0 Identifiers ==
			int prevPos = i;
			if( !SkipBrace(ref i) ) // if no brace ahead
				{
				i = prevPos;
				if( matchId.Count != 1 )
					throw AmbiguousMatch(fPos, matchId);

				return SimpleMatch(matchId[0], ref i, len);
				}
			
			funcs = GetSuitableFunctions(matchFn, ref i, out buf, out args);

			if( args == 1 ) // one arg: maybe iden
				{
				// can't deduce if exist func with 1 arg
				if( funcs.Count != 0 )
					throw AmbiguousMatch(fPos, matches);

				// not more than one iden candidate
				if( matchId.Count != 1 )
					throw AmbiguousMatch(fPos, matchId);

				return SimpleMatch(matchId[0], ref i, len);
				}

			if( funcs.Count == 1 )
				OutputBufferCall(buf, funcs[0], args);

			else if( funcs.Count == 0 )
				 throw WrongArgsCount(args, fPos, len, null);
			else throw AmbiguousMatch(fPos, matchFn);
			
			return Item.End;
			}

		#endregion
		#region Helpers

		private MethodGroup GetMethod( Capture match )
			{
			return _context._funcs[match.Index];
			}

		private ListOfItems GetSuitableFunctions(
						IEnumerable<Capture> matches, ref int i,
						out BufferOutput buf, out int args )
			{
			var methods = new List<MethodGroup>( );
			foreach( Capture match in matches )
				{
				methods.Add(GetMethod(match));
				}

			IExpressionOutput old = _output;

			buf = new BufferOutput();
			
			_output = buf; args = ParseNested(ref i, true);
			_output = old;

			int fixCount = -1;
			bool isParams = false;

			var funcs = new ListOfItems( );
			foreach( MethodGroup method in methods )
				{
				var item = method.GetFunc(args);
				if( item == null ) continue;

				// some kind of overload resolution :)
				// TODO: completely rewrite here

				if( item.ArgCount > fixCount )
					{
					funcs.Clear( );
					funcs.Add(item);
					fixCount = item.ArgCount;
					isParams = item.IsParams;
					}
				else if( item.ArgCount == fixCount )
					{
					if( item.IsParams )
						{
						if( isParams )
							funcs.Add(item);
						}
					else
						{
						if( isParams )
							{
							isParams = false;
							funcs.Clear( );
							}

						funcs.Add(item);
						}
					}
				}

			return funcs;
			}

		private void OutputBufferCall( BufferOutput buf, MethodGroup.Item func, int args )
			{
			if( func.IsParams )
				 _output.BeginCall(func.ArgCount, args - func.ArgCount);
			else _output.BeginCall(-1, 0);
			
			buf.WriteTo(_output);

			_output.PutFunction(func.Func);
			}

		private bool SkipBrace( ref int i )
			{
			while( i < _len && Char.IsWhiteSpace(_expr[i]) ) i++;

			if( i >= _len ) return false;

			_curPos = i;
			_prePos = _curPos;
			char c = _expr[i++];

			return ( c == '(' );
			}

		private int ParseNested( ref int i, bool func )
			{
			int bPos = _curPos;
			_prePos = _curPos;
			_depth++;

			int args = Parse(ref i, func);
			
			if( args == -1 && _depth > 0 )
				{
				throw BraceDisbalance(bPos, false);
				}
			
			_depth--;
			return args;
			}

		#endregion
		#region Search

		private enum Iden { Argument, Constant, Function }
		
		private static bool IsFunc( Capture match )
			{
			return match.Type == Iden.Function;
			}

		private struct SearchItem
			{
			private readonly IEnumerable<string> _names;
			private readonly Iden _type;

			public IEnumerable< string > Names
							 { get { return _names; } }
			public Iden Type { get { return _type;  } }

			public SearchItem( Iden type, IEnumerable<string> names )
				{
				_names = names;
				_type  = type;
				}
			}

		private struct Capture
			{
			private readonly Iden _type;
			private readonly int _index;

			public Iden Type { get { return  _type; } }
			public int Index { get { return _index; } }

			public Capture( Iden type, int index )
				{
				_index = index;
				_type  = type;
				}
			}

#if SILVERLIGHT

		private List<Capture> GetMatchesCulture( ref int max )
			{
			var match = new List<Capture>( );

			CultureInfo culture = _context._culture;
			bool ignoreCase = _context._ignoreCase;

			var compare = ignoreCase?
				CompareOptions.IgnoreCase:
				CompareOptions.None;

			foreach( SearchItem list in _idens )
				{
				int id = 0;
				foreach( string name in list.Names )
					{
					int ln = name.Length;
					if( ln >= max &&
						String.Compare(_expr, _curPos, name, 0, ln, culture, compare) == 0)
						{
						if( ln != max ) match.Clear();
						match.Add(new Capture(list.Type, id));
						max = ln;
						}
					id++;
					}
				}

			return match;
			}

#else

		private List<Capture> GetMatchesCulture( ref int max )
			{
			var match = new List<Capture>( );
			
			CultureInfo culture = _context._culture;
			bool ignoreCase = _context._ignoreCase;

			foreach( SearchItem list in _idens )
				{
				int id = 0;
				foreach( string name in list.Names )
					{
					int ln = name.Length;
					if(	ln >= max &&
						String.Compare(_expr, _curPos, name, 0, ln, ignoreCase, culture) == 0 )
						{
						if(ln != max) match.Clear( );
						match.Add(new Capture(list.Type, id));
						max = ln;
						}
					id++;
					}
				}

			return match;
			}

#endif

		private List<Capture> GetMatchesOrdinal( ref int max )
			{
			var match = new List<Capture>( );
			
			var strCmp = ( _context._ignoreCase )?
				StringComparison.OrdinalIgnoreCase:
				StringComparison.Ordinal;

			foreach( SearchItem list in _idens )
				{
				int id = 0;
				foreach( string name in list.Names )
					{
					int ln = name.Length;
					if(	ln >= max &&
						String.Compare(_expr, _curPos, name, 0, ln, strCmp) == 0 )
						{
						if(ln != max) match.Clear( );
						match.Add(new Capture(list.Type, id));
						max = ln;
						}
					id++;
					}
				}

			return match;
			}

		#endregion
		}
	}
