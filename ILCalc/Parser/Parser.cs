using System;
using System.Collections.Generic;

namespace ILCalc
{
  sealed partial class Parser<T>
  {
    #region Fields

    int exprDepth;
    int curPos;
    int prePos;
    T value;

    #endregion

    int Parse(ref int i, bool func)
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

        //this.curPos = i++;
        this.curPos = i;
        int val;

        // ============================================= NUMBER ==
        if ((val = Literal.TryParse(i, this)) != -1)
        {
          i += val;

          // [ )123 ], [ 123 456 ] or [ pi123 ]
          if (prev >= Item.Number)
          {
            //ScanNumber(c, ref i);
            throw IncorrectConstr(prev, Item.Number, i);
          }

          Output.PutConstant(value);
          prev = Item.Number;
        }

        // =========================================== OPERATOR ==
        else if ((val = Operators.IndexOf(c)) != -1)
        {
          // BINARY ============
          // [ )+ ], [ 123+ ] or [ pi+ ]
          if (prev >= Item.Number)
          {
            Flush(operators, Priority[val]);
            operators.Push(val);
          }

          // UNARY [-] =========
          else if (val == Code.Sub)
          {
            // prev == [+-], [,] or [(]
            operators.Push(Code.Neg);
          }

          // UNARY [+] =========
          else
          {
            //throw IncorrectConstr(prev, Item.Operator, i);
            throw IncorrectConstr(prev, Item.Operator, i+1);
          }

          i++; // <===
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
            throw IncorrectConstr(prev, Item.Separator, i+1);
            //throw IncorrectConstr(prev, Item.Separator, i);
          }

          Flush(operators);
          Output.PutSeparator();
          separators++;

          i++; // <====
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
              throw IncorrectConstr(prev, Item.Begin, i+1);
              //throw IncorrectConstr(prev, Item.Begin, i);
            }

            Flush(operators, 1);
            operators.Push(Code.Mul); // Insert [*]
          }

          i++; // <======
          ParseNested(ref i, false);
          prev = Item.End;
        }

        // ======================================== BRACE CLOSE ==
        else if (c == ')')
        {
          // [ +) ], [ ,) ] or [ () ]
          if (prev <= Item.Separator ||
            (!func && prev == Item.Begin))
          {
            throw IncorrectConstr(prev, Item.End, i+1);
            //throw IncorrectConstr(prev, Item.End, i);
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

          i++; // <=====
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
              //TODO: test if "sin z" (1 char unresolved!)
              throw IncorrectIden(i);
            }

            if (!Context.ImplicitMul)
            {
              //throw IncorrectConstr(prev, Item.Identifier, i);
              throw IncorrectConstr(prev, Item.Identifier, i+1);
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

        this.prePos = this.curPos;
      }

      // ====================================== END OF EXPRESSION ==
      // [ +) ], [ ,) ] or [ () ]
      if (prev <= Item.Begin)
      {
        //throw IncorrectConstr(prev, Item.End, i);
        throw IncorrectConstr(prev, Item.End, i+1);
      }

      Flush(operators);
      Output.PutExprEnd();

      return -1;
    }

    #region Stack Operations

    void Flush(Stack<int> stack)
    {
      while (stack.Count > 0)
      {
        Output.PutOperator(stack.Pop());
      }
    }

    void Flush(Stack<int> stack, int priority)
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