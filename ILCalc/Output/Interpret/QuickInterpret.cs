using System;
using System.Reflection;

namespace ILCalc
	{
	//TODO: use less memory for standart calls?

	sealed class QuickInterpret : IExpressionOutput
		{
		#region Fields

		private readonly double[] _args;
		private readonly bool _check;

		private double[] _stack = new double[4];
		private int[] _calls = new int[6];
		private int _pos = -1;
		private int _cpos = 0;
		
		#endregion
		#region Members

		public double Result
			{
			get { return _stack[0]; }
			}

		public void Clear( )
			{
			_pos = -1;
			_cpos = 0;
			}

		public QuickInterpret( double[] args, bool check )
			{
			_check = check;
			_args = args;
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			if(++_pos == _stack.Length)
				{
				var dest = new double[_pos * 2];
				Array.Copy(_stack, 0, dest, 0, _pos);
				_stack = dest;
				}

			_stack[_pos] = value;
			}

		public void PutArgument( int id )
			{
			if(++_pos == _stack.Length)
				{
				var dest = new double[_pos * 2];
				Array.Copy(_stack, 0, dest, 0, _pos);
				_stack = dest;
				}

			_stack[_pos] = _args[id];
			}

		public void PutSeparator( ) { }

		public void PutFunction( MethodInfo func )
			{
			int argc = _calls[--_cpos];
			int argv = _calls[--_cpos];

			object[] stdArgs; // args array

			if(argc >= 0) // params call
				{
				var varArgs = new double[argv];

				// fill params args array
				for(int i = argv - 1; i >= 0; i--)
					{
					varArgs[i] = _stack[_pos--];
					}

				stdArgs = new object[argc + 1];
				stdArgs[argc] = varArgs;
				}
			else // std call
				{
				argc = func.GetParameters( ).Length; // bad
				stdArgs = new object[argc];
				}

			// fill std args array
			for(int i = argc - 1; i >= 0; i--)
				{
				stdArgs[i] = _stack[_pos--];
				}

			// invoke via reflection
			_stack[++_pos] = (double) func.Invoke(null, stdArgs);
			}

		public void PutOperator( int oper )
			{
			double value = _stack[_pos--];
			if( oper != Code.Neg )
				{
				if( oper == Code.Add ) _stack[_pos] += value; else
				if( oper == Code.Mul ) _stack[_pos] *= value; else
				if( oper == Code.Sub ) _stack[_pos] -= value; else
				if( oper == Code.Div ) _stack[_pos] /= value; else
				if( oper == Code.Rem ) _stack[_pos] %= value; else
					_stack[_pos] = Math.Pow(_stack[_pos], value);
				}
			else _stack[++_pos] = -value;
			}

		public void BeginCall( int fixCount, int varCount )
			{
			if(_cpos == _calls.Length)
				{
				var dest = new int[_cpos * 2];
				Array.Copy(_calls, 0, dest, 0, _cpos);
				_calls = dest;
				}

			_calls[_cpos++] = varCount;
			_calls[_cpos++] = fixCount;
			}

		public void PutExprEnd( )
			{
			if( !_check ) return;

			double res = _stack[0];

			if(	double.IsInfinity(res)
			||	double.IsNaN(res) )
				{
				throw new NotFiniteNumberException(res.ToString());
				}

			}

		#endregion
		}
	}
