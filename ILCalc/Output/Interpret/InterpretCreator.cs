using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	sealed class InterpretCreator : IExpressionOutput
		{
		#region Fields

		private readonly Stack<int> calls;

		internal readonly List<int> code;
		internal readonly List<double> numbers;
		internal readonly List<FuncCall> funcs;
		internal readonly List<Delegate> delegates;

		internal int stMax;
		private int stSize;

		#endregion
		#region Constructor

		public InterpretCreator( )
			{
			code = new List<int>(8);
			funcs = new List<FuncCall>( );
			numbers = new List<double>(4);
			delegates = new List<Delegate>( );

			calls = new Stack<int>( );
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			numbers.Add(value);
			code.Add(Code.Number);

			if( ++stSize > stMax ) stMax = stSize;
			}

		public void PutArgument( int id )
			{
			code.Add(Code.Argument);
			code.Add(id);

			if( ++stSize > stMax ) stMax = stSize;
			}

		public void PutOperator( int oper )
			{
			code.Add(oper);

			if( oper != Code.Neg )
				stSize--;
			}

		public void PutSeparator( ) { }
		public void PutBeginCall( ) { }

		public void PutBeginParams( int fixCount, int varCount )
			{
			calls.Push(varCount);
			calls.Push(fixCount);
			}

		public void PutMethod( MethodInfo method, int fixCount )
			{
			if( fixCount < 0 ) // params method
				{
				    fixCount = calls.Pop( );
				int varCount = calls.Pop( );

				code.Add(Code.Function);
				code.Add(PutMethod(method, fixCount, varCount));
				RecalcStackSize(fixCount + varCount);
				return;
				}

			switch( fixCount )
				{
				case 0:
					code.Add(Code.Delegate0);
					code.Add(PutDelegate(method, evalType0));
					if( ++stSize > stMax ) stMax = stSize;
					break;

				case 1:
					code.Add(Code.Delegate1);
					code.Add(PutDelegate(method, evalType1));
					break;

				case 2:
					code.Add(Code.Delegate2);
					code.Add(PutDelegate(method, evalType2));
					stSize--;
					break;

				default:
					code.Add(Code.Function);
					code.Add(PutMethod(method, fixCount, -1));
					RecalcStackSize(fixCount);
					break;
				}
			}

		public void PutExprEnd( )
			{
			code.Add(Code.Return);
			code.Add(0); // fictive code
			}

		#endregion
		#region Helpers

		private void RecalcStackSize( int argsCount )
			{
			if( argsCount == 0 )
				{
				if( ++stSize > stMax ) stMax = stSize;
				}

			else stSize -= argsCount - 1;
			}

		private int PutDelegate( MethodInfo method, Type delegateType )
			{
			for(int i = 0; i < delegates.Count; i++)
				{
				if( delegates[i].Method == method )
					{
					return i;
					}
				}

			delegates.Add(
				Delegate.CreateDelegate(delegateType, null, method)
				);

			return delegates.Count - 1;
			}

		private int PutMethod( MethodInfo method, int argc, int argv )
			{
			for( int i = 0; i < funcs.Count; i++ )
				{
				if( funcs[i].Method == method
				 && funcs[i].IsReusable(argc, argv))
					{
					return i;
					}
				}

			funcs.Add(new FuncCall(method, argc, argv));
			return funcs.Count - 1;
			}

		#endregion
		#region Static Data

		// Types ==================================================

		private static readonly Type evalType0 = typeof(EvalFunc0);
		private static readonly Type evalType1 = typeof(EvalFunc1);
		private static readonly Type evalType2 = typeof(EvalFunc2);

		#endregion
		}
	}
