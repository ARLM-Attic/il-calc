using System;
using System.Collections.Generic;

namespace ILCalc
{
	internal sealed partial class Parser
	{
		#region Fields

		private int exprDepth;
		private int curPos;
		private int prePos;

		#endregion

		private int Parse(ref int i, bool func)
		{
			Item prev = Item.Begin;
			int separators = 0;
			var operators = new Stack<int>();

			while (i < this.xlen)
			{
				char c = this.expr[i];

				// NOTE: maybe put in last else?
				if (Char.IsWhiteSpace(c))
				{
					i++;
					continue;
				}

				this.curPos = i++;

				// ============================================= NUMBER ==
				if ((c <= '9' && '0' <= c) || c == dotSymbol)
				{
					// [ )123 ], [ 123 456 ] or [ pi123 ]
					if (prev >= Item.Number)
					{
						ScanNumber(c, ref i);
						throw IncorrectConstr(prev, Item.Number, i);
					}

					Output.PutNumber(ScanNumber(c, ref i));
					prev = Item.Number;
				}

				// =========================================== OPERATOR ==
				else
				{
					int oper;
					if ((oper = Operators.IndexOf(c)) != -1)
					{
						// BINARY ============
						// [ )+ ], [ 123+ ] or [ pi+ ]
						if (prev >= Item.Number)
						{
							Flush(operators, Priority[oper]);
							operators.Push(oper);
						}

						// UNARY [-] =========
						else if (oper == Code.Sub)
						{
							// prev == [+-], [,] or [(]
							operators.Push(Code.Neg);
						}
					
						// UNARY [+] =========
						else
						{
							throw IncorrectConstr(prev, Item.Operator, i);
						}

						prev = Item.Operator;
					}

					// ========================================== SEPARATOR ==
					else if (c == this.sepSymbol)
					{
						if (!func)
						{
							throw InvalidSeparator();
						}

						// [ (, ], [ +, ] or [ ,, ]
						if (prev <= Item.Begin)
						{
							throw IncorrectConstr(prev, Item.Separator, i);
						}

						Flush(operators);
						Output.PutSeparator();
						separators++;

						prev = Item.Separator;
					}

					// ========================================= BRACE OPEN ==
					else if (c == '(')
					{
						// [ )( ], [ 123( ] or [ pi( ]
						if (prev >= Item.Number)
						{
							if (!Context.ImplicitMul)
							{
								throw IncorrectConstr(prev, Item.Begin, i);
							}
						
							Flush(operators, 1);
							operators.Push(Code.Mul); // Insert [*]
						}

						ParseNested(ref i, false);
						prev = Item.End;
					}

					// ======================================== BRACE CLOSE ==
					else if (c == ')')
					{
						// [ +) ], [ ,) ] or [ () ]
						if (prev <= Item.Separator || (!func
						 && prev == Item.Begin))
						{
							throw IncorrectConstr(prev, Item.End, i);
						}

						Flush(operators);
						if (this.exprDepth == 0)
						{
							throw BraceDisbalance(this.curPos, true);
						}

						if (prev != Item.Begin)
						{
							separators++;
						}

						return separators;
					}

					// ========================================= IDENTIFIER ==
					else if (Char.IsLetterOrDigit(c) || c == '_')
					{
						if (prev >= Item.Number)
						{
							// [ pi sin ]
							if (prev == Item.Identifier)
							{
								throw IncorrectIden(i);
							}

							if (!Context.ImplicitMul)
							{
								throw IncorrectConstr(prev, Item.Identifier, i);
							}

							// [ )pi ] or [ 123pi ]
							Flush(operators, 1);
							operators.Push(Code.Mul); // Insert [*]
						}

						prev = ScanIdenifier(ref i);
					}

					// ========================================= UNRESOLVED ==
					else
					{
						throw UnresolvedSymbol(this.curPos);
					}
				}

				this.prePos = this.curPos;
			}

			// ====================================== END OF EXPRESSION ==
			// [ +) ], [ ,) ] or [ () ]
			if (prev <= Item.Begin)
			{
				throw IncorrectConstr(prev, Item.End, i);
			}

			Flush(operators);
			Output.PutExprEnd();

			return -1;
		}

		#region Stack Operations

		private void Flush(Stack<int> stack)
		{
			while (stack.Count > 0)
			{
				Output.PutOperator(stack.Pop());
			}
		}

		private void Flush(Stack<int> stack, int priority)
		{
			while (stack.Count > 0 &&
			       priority <= Priority[stack.Peek()])
			{
				Output.PutOperator(stack.Pop());
			}
		}

		#endregion
	}
}