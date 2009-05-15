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

			while (i < this.exprLen)
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
						this.ScanNumber(c, ref i);
						throw this.IncorrectConstr(prev, Item.Number, i);
					}

					this.output.PutNumber(ScanNumber(c, ref i));
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
							this.Flush(operators, Priority[oper]);
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
							throw this.IncorrectConstr(prev, Item.Operator, i);
						}

						prev = Item.Operator;
					}

					// ========================================== SEPARATOR ==
					else if (c == this.sepSymbol)
					{
						if (!func)
						{
							throw this.InvalidSeparator();
						}

						// [ (, ], [ +, ] or [ ,, ]
						if (prev <= Item.Begin)
						{
							throw this.IncorrectConstr(prev, Item.Separator, i);
						}

						this.Flush(operators);
						this.output.PutSeparator();
						separators++;

						prev = Item.Separator;
					}

					// ========================================= BRACE OPEN ==
					else if (c == '(')
					{
						// [ )( ], [ 123( ] or [ pi( ]
						if (prev >= Item.Number)
						{
							if (!this.context.implicitMul)
							{
								throw this.IncorrectConstr(prev, Item.Begin, i);
							}
						
							this.Flush(operators, 1);
							operators.Push(Code.Mul); // Insert [*]
						}

						this.ParseNested(ref i, false);
						prev = Item.End;
					}

					// ======================================== BRACE CLOSE ==
					else if (c == ')')
					{
						// [ +) ], [ ,) ] or [ () ]
						if (prev <= Item.Separator || (!func
						 && prev == Item.Begin))
						{
							throw this.IncorrectConstr(prev, Item.End, i);
						}

						this.Flush(operators);
						if (this.exprDepth == 0)
						{
							throw this.BraceDisbalance(this.curPos, true);
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
								throw this.IncorrectIden(i);
							}

							if (!this.context.implicitMul)
							{
								throw this.IncorrectConstr(prev, Item.Identifier, i);
							}

							// [ )pi ] or [ 123pi ]
							this.Flush(operators, 1);
							operators.Push(Code.Mul); // Insert [*]
						}

						prev = this.ScanIdenifier(ref i);
					}

					// ========================================= UNRESOLVED ==
					else
					{
						throw this.UnresolvedSymbol(this.curPos);
					}
				}

				this.prePos = this.curPos;
			}

			// ====================================== END OF EXPRESSION ==
			// [ +) ], [ ,) ] or [ () ]
			if (prev <= Item.Begin)
			{
				throw this.IncorrectConstr(prev, Item.End, i);
			}

			this.Flush(operators);
			this.output.PutExprEnd();

			return -1;
		}

		#region Stack Operations

		private void Flush(Stack<int> stack)
		{
			while (stack.Count > 0)
			{
				this.output.PutOperator(stack.Pop());
			}
		}

		private void Flush(Stack<int> stack, int priority)
		{
			while (stack.Count > 0 && priority <= Priority[stack.Peek()])
			{
				this.output.PutOperator(stack.Pop());
			}
		}

		#endregion
	}
}