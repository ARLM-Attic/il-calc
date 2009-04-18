using System;
using System.Collections.Generic;
using System.Globalization;

namespace ILCalc
	{
	sealed partial class Parser
		{
		private Item ScanIdenifier( ref int i )
			{
			int len = 0;

			var matches = ( context.parseCulture != null )?
				GetMatchesCulture(ref len):
				GetMatchesOrdinal(ref len);

			if( len == 0 ) throw UnresolvedIdentifier(1);

			if( curPos + len < exprLen )
				{
				char c = expr[curPos + len];
				if( Char.IsLetterOrDigit(c) || c == '_' )
					{
					throw UnresolvedIdentifier(len + 1);
					}
				}

			i += len - 1;

			return ( matches.Count == 1 )?
				SimpleMatch(matches[0], ref i, len):
				AmbiguousMatch(matches, ref i, len);
			}

		#region Matches

		private Item SimpleMatch( Capture match, ref int i, int len )
			{
			switch( match.Type )
				{
				case IdenType.Argument: // ===============================
					{
					output.PutArgument(match.Index);
					return Item.Identifier;
					}

				case IdenType.Constant: // ===============================
					{
					output.PutNumber(context.constDict[match.Index]);
					return Item.Identifier;
					}

				case IdenType.Function: // ===============================
					{
					int funcPos = curPos;
					if( !SkipBrace(ref i) )
						throw NoOpenBrace(funcPos, len);

					FunctionGroup group = GetGroup(match);

					if( group.HasParamsMethods )
						{
						IExpressionOutput old = output;
						
						var buf = new BufferOutput( );
						
						output = buf; int args = ParseNested(ref i, true);
						output = old;

						Function func = group.GetOverload(args);
						
						if( func != null )
							OutputBufferCall(buf, func, args);
						else 
							throw WrongArgsCount(funcPos, len, args, group);
						}
					else
						{
						output.PutBeginCall( );

						int args = ParseNested(ref i, true);
						var func = group.GetOverload(args);

						if( func != null )
							output.PutMethod(func.Method, func.ArgsCount);
						else
							throw WrongArgsCount(funcPos, len, args, group);
						}

					return Item.End;
					}

				default:
					throw new NotSupportedException();
				}
			} 

		private Item AmbiguousMatch( ICollection<Capture> matches,
									 ref int i, int len )
			{
			#region Count Matches

			var funcMatches = new List<Capture>( );
			var idenMatches = new List<Capture>( );

			foreach( Capture match in matches )
				{
				if( IsFunc(match) )
					 funcMatches.Add(match);
				else idenMatches.Add(match);
				}
				
			#endregion
			#region Locals

			List<Function> funcs;
			BufferOutput buf;
			int args, funcPos = curPos;

			#endregion
			
			//===================================== > 0 Identifiers ==
			if( funcMatches.Count == 0 )
				{
				throw AmbiguousMatch(curPos, idenMatches);
				}
			
			//======================================= > 0 Functions ==
			if( idenMatches.Count == 0 )
				{
				if( !SkipBrace(ref i) )
					{
					throw NoOpenBrace(funcPos, len);
					}

				funcs = GetSuitable(funcMatches, ref i, out buf, out args);

				if( funcs.Count == 1 )
					OutputBufferCall(buf, funcs[0], args);

				else if( funcs.Count == 0 )
					throw WrongArgsCount(funcPos, len, args, null);
				else
					throw AmbiguousMatch(funcPos, funcMatches);

				return Item.End;
				}
			
			//==================== > 0 Functions and > 0 Identifiers ==
			int prevPos = i;
			if( !SkipBrace(ref i) ) // if no brace ahead
				{
				i = prevPos;
				if( idenMatches.Count != 1 )
					throw AmbiguousMatch(funcPos, idenMatches);

				return SimpleMatch(idenMatches[0], ref i, len);
				}
			
			funcs = GetSuitable(funcMatches, ref i, out buf, out args);

			if( args == 1 ) // one arg: maybe iden
				{
				// can't deduce if exist func with 1 arg
				if( funcs.Count != 0 )
					throw AmbiguousMatch(funcPos, matches);

				// not more than one iden candidate
				if( idenMatches.Count != 1 )
					throw AmbiguousMatch(funcPos, idenMatches);

				return SimpleMatch(idenMatches[0], ref i, len);
				}

			if( funcs.Count == 1 )
				OutputBufferCall(buf, funcs[0], args);

			else if( funcs.Count == 0 )
				throw WrongArgsCount(funcPos, len, args, null);
			else
				throw AmbiguousMatch(funcPos, funcMatches);
			
			return Item.End;
			}

		#endregion
		#region Helpers

		private FunctionGroup GetGroup( Capture match )
			{
			return context.funcsDict[match.Index];
			}

		private List<Function> GetSuitable( IEnumerable<Capture> matches,
						ref int i, out BufferOutput buf, out int args )
			{
			var methods = new List<FunctionGroup>( );

			foreach( Capture match in matches )
				{
				methods.Add(GetGroup(match));
				}

			IExpressionOutput old = output;

			buf = new BufferOutput();
			
			output = buf; args = ParseNested(ref i, true);
			output = old;

			return FunctionGroup.GetOverloadsList(methods, args);
			}

		private void OutputBufferCall( BufferOutput buf, Function func, int args )
			{
			if( func.HasParamArray )
				 output.PutBeginParams(func.ArgsCount, args - func.ArgsCount);
			else output.PutBeginCall( );
			
			buf.WriteTo(output);

			output.PutMethod(func.Method, -1);
			}

		private bool SkipBrace( ref int i )
			{
			while( i < exprLen
				&& Char.IsWhiteSpace(expr[i]) ) i++;

			if( i >= exprLen ) return false;

			curPos = i;
			prePos = curPos;
			char c = expr[i++];

			return ( c == '(' );
			}

		private int ParseNested( ref int i, bool func )
			{
			int bPos = curPos;
			prePos = curPos;
			exprDepth++;

			int args = Parse(ref i, func);
			if( args == -1 && exprDepth > 0 )
				{
				throw BraceDisbalance(bPos, false);
				}
			
			exprDepth--;
			return args;
			}

		#endregion
		#region Search

		private enum IdenType { Argument, Constant, Function }
		
		private static bool IsFunc( Capture match )
			{
			return match.Type == IdenType.Function;
			}

		private struct SearchItem
			{
			private readonly IEnumerable<string> names;
			private readonly IdenType type;

			public IEnumerable< string > Names
								 { get { return names; } }
			public IdenType Type { get { return type;  } }

			public SearchItem( IdenType type, IEnumerable<string> names )
				{
				this.names = names;
				this.type  = type;
				}
			}

		private struct Capture
			{
			private readonly IdenType type;
			private readonly int index;

			public IdenType Type { get { return  type; } }
			public int Index     { get { return index; } }

			public Capture( IdenType type, int index )
				{
				this.index = index;
				this.type  = type;
				}
			}

		private List<Capture> GetMatchesCulture( ref int max )
			{
			var match = new List<Capture>( );

			CultureInfo culture = context.parseCulture;

#if SILVERLIGHT
			var compare = context.ignoreCase?
				CompareOptions.IgnoreCase:
				CompareOptions.None;
#else
			bool ignoreCase = context.ignoreCase;
#endif

			foreach( SearchItem list in idenList )
				{
				int id = 0;
				foreach( string name in list.Names )
					{
					int ln = name.Length;
					if( ln >= max &&
#if SILVERLIGHT
						String.Compare(expr, curPos, name, 0, ln, culture, compare) == 0 )
#else
						String.Compare(expr, curPos, name, 0, ln, ignoreCase, culture) == 0 )
#endif
						{
						if( ln != max ) match.Clear( );
						match.Add(new Capture(list.Type, id));
						max = ln;
						}
					id++;
					}
				}

			return match;
			}

		private List<Capture> GetMatchesOrdinal( ref int max )
			{
			var match = new List<Capture>( );
			
			var strCmp = ( context.ignoreCase )?
				StringComparison.OrdinalIgnoreCase:
				StringComparison.Ordinal;

			foreach( SearchItem list in idenList )
				{
				int id = 0;
				foreach( string name in list.Names )
					{
					int ln = name.Length;
					if(	ln >= max &&
						String.Compare(expr, curPos, name, 0, ln, strCmp) == 0 )
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
