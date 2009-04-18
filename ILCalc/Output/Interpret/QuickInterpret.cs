using System;
using System.Reflection;

namespace ILCalc
	{
	sealed class QuickInterpret : IExpressionOutput
		{
		#region Fields

		private readonly double[] argList;
		private readonly bool checkedMode;

		private double[] stack = new double[4];
		private int[] calls = new int[6];
		private int cpos, pos = -1;
		
		#endregion
		#region Methods

		public double Result
			{
			get { return stack[0]; }
			}

		public void Clear( )
			{
			pos = -1;
			cpos = 0;
			}

		public QuickInterpret( double[] arguments, bool check )
			{
			checkedMode = check;
			argList = arguments;
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			if( ++pos == stack.Length )
				{
				var newStack = new double[pos * 2];
				Array.Copy(stack, 0, newStack, 0, pos);
				stack = newStack;
				}

			stack[pos] = value;
			}

		public void PutArgument( int id )
			{
			if( ++pos == stack.Length )
				{
				var newStack = new double[pos * 2];
				Array.Copy(stack, 0, newStack, 0, pos);
				stack = newStack;
				}

			stack[pos] = argList[id];
			}

		public void PutSeparator( ) { }

		public void PutOperator( int oper )
			{
			double value = stack[pos--];
			if( oper != Code.Neg )
				{
				if( oper == Code.Add ) stack[pos] += value; else
				if( oper == Code.Mul ) stack[pos] *= value; else
				if( oper == Code.Sub ) stack[pos] -= value; else
				if( oper == Code.Div ) stack[pos] /= value; else
				if( oper == Code.Rem ) stack[pos] %= value; else
					stack[pos] = Math.Pow(stack[pos], value);
				}
			else stack[++pos] = -value;
			}

		public void PutBeginCall( ) { }

		public void PutBeginParams( int fixCount, int varCount )
			{
			if( cpos == calls.Length )
				{
				var newCalls = new int[cpos * 2];
				Array.Copy(calls, 0, newCalls, 0, cpos);
				calls = newCalls;
				}

			calls[cpos++] = varCount;
			calls[cpos++] = fixCount;
			}

		public void PutMethod( MethodInfo method, int fixCount )
			{
			object[] fixArgs;

			if( fixCount < 0 ) // params call
				{
				fixCount = calls[--cpos];
				
				int varCount = calls[--cpos];
				var varArgs = new double[varCount];

				// fill params args array
				for( int i = varCount - 1; i >= 0; i-- )
					{
					varArgs[i] = stack[pos--];
					}

				fixArgs = new object[fixCount + 1];
				fixArgs[fixCount] = varArgs;
				}
			else
				fixArgs = new object[fixCount];

			// fill std args array
			for( int i = fixCount - 1; i >= 0; i-- )
				{
				fixArgs[i] = stack[pos--];
				}

			// invoke via reflection
			PutNumber(( double ) method.Invoke(null, fixArgs));
			}

		public void PutExprEnd( )
			{
			if( !checkedMode ) return;

			double res = stack[0];

			if(	double.IsInfinity(res)
			||	double.IsNaN(res) )
				{
				throw new NotFiniteNumberException(res.ToString());
				}
			}

		#endregion
		}
	}
