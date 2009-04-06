using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	sealed class InterpretCreator : IExpressionOutput
		{
		#region Fields

		private readonly Stack<int> _calls;

		internal readonly List<int> _code;
		internal readonly List<double> _numbers;
		internal readonly List<InterpCall> _funcs;
		internal readonly List<Delegate> _delegates;

		internal int _stackMax;
		private int _stackSize;

		#endregion
		#region Constructor

		internal InterpretCreator( )
			{
			_code = new List<int>(8);
			_funcs = new List<InterpCall>( );
			_numbers = new List<double>(4);
			_delegates = new List<Delegate>( );

			_calls = new Stack<int>( );
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			_numbers.Add(value);
			_code.Add(Code.Number);

			if( ++_stackSize > _stackMax )
				{
				_stackMax = _stackSize;
				}
			}

		public void PutArgument( int id )
			{
			_code.Add(Code.Argument);
			_code.Add(id);

			if( ++_stackSize > _stackMax )
				{
				_stackMax = _stackSize;
				}
			}

		public void PutFunction( MethodInfo func )
			{
			int argc = _calls.Pop();
			if( argc >= 0 ) // params method
				{
				int argv = _calls.Pop();

				_code.Add(Code.Function);
				_code.Add(AddFunc(new InterpCall(func, argc, argv)));

				_stackSize -= (argc + argv - 1);
				return;
				}

			argc = func.GetParameters().Length;
			switch( argc )
				{
				case 0:
					_code.Add(Code.Delegate0);
					_code.Add(AddDelegate(func, _eval0));
					if(++_stackSize > _stackMax)
						{
						_stackMax = _stackSize;
						}
					break;

				case 1:
					_code.Add(Code.Delegate1);
					_code.Add(AddDelegate(func, _eval1));
					break;

				case 2:
					_code.Add(Code.Delegate2);
					_code.Add(AddDelegate(func, _eval2));
					_stackSize--;
					break;

				default:
					_code.Add(Code.Function);
					_code.Add(AddFunc(new InterpCall(func, argc)));
					_stackSize -= argc - 1;
					break;
				}
			}

		public void PutOperator( int oper )
			{
			_code.Add(oper);

			if( oper != Code.Neg )
				{
				_stackSize--;
				}
			}

		public void PutSeparator( ) { }

		public void BeginCall( int fixCount, int varCount )
			{
			if(fixCount >= 0)
				{
				_calls.Push(varCount);
				}
			_calls.Push(fixCount);
			}

		public void PutExprEnd( )
			{
			_code.Add(Code.Return);
			_code.Add(0); // fictive code
			}

		#endregion
		#region Helpers

		private int AddDelegate( MethodInfo func, Type delegateType )
			{
			for(int i = 0; i < _delegates.Count; i++)
				{
				if(_delegates[i].Method == func) return i;
				i++;
				}

			var del = Delegate.CreateDelegate(delegateType, null, func);

			_delegates.Add(del);
			return _delegates.Count - 1;
			}

		private int AddFunc( InterpCall call )
			{
			MethodInfo func = call.Method;
			for(int i = 0; i < _funcs.Count; i++)
				{
				if(_funcs[i].Method == func) return i;
				i++;
				}

			_funcs.Add(call);
			return _funcs.Count - 1;
			}

		#endregion
		#region Static Data

		// Types ==================================================

		private static readonly Type _eval0 = typeof(EvalFunc0);
		private static readonly Type _eval1 = typeof(EvalFunc1);
		private static readonly Type _eval2 = typeof(EvalFunc2);

		#endregion
		}
	}
