using System.Collections.Generic;
using System;

namespace ILCalc
	{
	sealed partial class Parser
		{
		#region Fields

		private int _depth;
		private int _curPos;
		private int _prePos;
		private int _nextOp;

		#endregion

		private int Parse( ref int i, bool func )
			{
			Item prev = Item.Begin;
			int separators = 0;
			var ops = new Stack<int>( );

			while( i < _len )
				{
				char c = _expr[i];
				// NOTE: maybe put in last else?
				if( Char.IsWhiteSpace(c) ) { i++; continue; }

				_curPos = i++;

				//============================================= NUMBER ==
				if( (c <= '9' && '0' <= c) || c == _dot )
					{
					// [ )123 ], [ 123 456 ] or [ pi123 ]
					if( prev >= Item.Number )
						{
						ScanNumber(c, ref i);
						throw IncorrectConstr(prev, Item.Number, i);
						}

					_output.PutNumber(ScanNumber(c, ref i));

					prev = Item.Number;
					}

				//=========================================== OPERATOR ==
				else if( (_nextOp = _operators.IndexOf(c)) != -1 )
					{
					// BINARY ============
					// [ )+ ], [ 123+ ] or [ pi+ ]
					if( prev >= Item.Number )
						{
						Flush(ops, _prior[_nextOp]);
						ops.Push(_nextOp);
						}

					// UNARY [-] =========
					else if( _nextOp == Code.Sub )
						{
						// prev == [+-], [,] or [(]
						ops.Push(Code.Neg);
						}
					
					// UNARY [+] =========
					else 
						throw IncorrectConstr(prev, Item.Operator, i);

					prev = Item.Operator;
					}

				//========================================== SEPARATOR ==
				else if( c == _sep )
					{
					if( !func ) 
						throw InvalidSeparator( );

					// [ (, ], [ +, ] or [ ,, ]
					if( prev <= Item.Begin )
						{
						throw IncorrectConstr(prev, Item.Separator, i);
						}

					Flush(ops);
					separators++;
					_output.PutSeparator( );

					prev = Item.Separator;
					}

				//========================================= BRACE OPEN ==
				else if( c == '(' )
					{
					// [ )( ], [ 123( ] or [ pi( ]
					if( prev >= Item.Number )
						{
						Flush(ops, 1);
						ops.Push(Code.Mul);	 // Insert (*)
						}

					ParseNested(ref i, false);
					prev = Item.End;
					}

				//======================================== BRACE CLOSE ==
				else if( c == ')' )
					{
					// [ +) ], [ ,) ] or [ () ]
					if( prev <= Item.Separator
					||	(!func && prev == Item.Begin) )
						{
						throw IncorrectConstr(prev, Item.End, i);
						}

					Flush(ops);
					if( _depth == 0 )
						{
						throw BraceDisbalance(_curPos, true);
						}

					if( prev != Item.Begin ) separators++;
					return separators;
					}

				//========================================= IDENTIFIER ==
				else if( Char.IsLetterOrDigit(c) || c == '_' )
					{
					if( prev >= Item.Number )
						{
						// [ pi sin ]
						if( prev == Item.Identifier )
							{
							throw IncorrectIden(i);
							}

						// [ )pi ] or [ 123pi ]
						Flush(ops, 1);
						ops.Push(Code.Mul);		// Insert [*]
						}

					prev = ScanIdenifier(ref i);
					}
				//========================================= UNRESOLVED ==
				else
					{
					throw UnresolvedSymbol(_curPos);
					}

				_prePos = _curPos;
				}

			//====================================== END OF EXPRESSION ==
			// [ +) ], [ ,) ] or [ () ]
			if( prev <= Item.Begin )
				{
				throw IncorrectConstr(prev, Item.End, i);
				}

			Flush(ops);

			_output.PutExprEnd( );
			return -1;
			}

		#region Stack Operations

		private void Flush( Stack<int> stack )
			{
			while( stack.Count > 0 )
				{
				_output.PutOperator(stack.Pop( ));
				}
			}

		private void Flush( Stack<int> stack, int priority )
			{
			while( stack.Count > 0 )
				{
				if( priority <= _prior[stack.Peek( )] )
					{
					_output.PutOperator(stack.Pop( ));
					}
				else break;
				}
			}

		#endregion
		}
	}