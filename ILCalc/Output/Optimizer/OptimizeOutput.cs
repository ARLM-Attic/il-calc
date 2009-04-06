using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	//TODO: x ^  0 => 1
	//TODO: x ^ -2 => 1 x x * /

	sealed class OptimizeOutput : BufferOutput, IExpressionOutput
		{
		#region Fields

		private readonly IExpressionOutput _output;
		private readonly QuickInterpret _interp;

		private readonly OptimizeModes _mode;

		#endregion
		#region Constructor

		public OptimizeOutput( IExpressionOutput output, OptimizeModes mode )
			{
			_output = output;
			_mode = mode;

			_interp = new QuickInterpret(null, false);
			}

		#endregion
		#region Members

		private bool ConstantFolding
			{
			get { return (_mode & OptimizeModes.ConstantFolding) != 0; }
			}

		private bool FuncionFolding
			{
			get { return (_mode & OptimizeModes.FunctionFolding) != 0; }
			}

		private bool PowOptimize
			{
			get { return (_mode & OptimizeModes.PowOptimize) != 0; }
			}

		#endregion
		#region Helpers

		private static T LastValue<T>( List<T> list, int id )
			{
			return list[list.Count - id];
			}

		private static T PopLast<T>( List<T> list )
			{
			int pos = list.Count - 1;
			T value = list[pos];
			list.RemoveAt(pos);
			return value;
			}

		private static void RemoveLast<T>( List<T> list )
			{
			list.RemoveAt(list.Count - 1);
			}

		private bool IsLastKnown( )
			{
			return _code[_code.Count - 1] == Code.Number;
			}

		private bool IsLastTwoKnown( )
			{
			int index = _code.Count;
			return _code[index - 1] == Code.Number
				&& _code[index - 2] == Code.Number;
			}

		private double LastNumber
			{
			get { return _nums[_nums.Count - 1];  }
			set { _nums[_nums.Count - 1] = value; }
			}

		private bool IsCallBegin( int pos )
			{
			int code = _code[pos];
			return code == Code.ParamCall
				|| code == Code.BeginCall;
			}

		#endregion
		#region IExpressionOutput

		public new void PutOperator( int oper )
			{
			if( ConstantFolding )
				{
				// Unary operator optimize ======================
				if( oper == Code.Neg && IsLastKnown( ) )
					{
					OptimizeNegate( );
					return;
					}

				// Binary operator optimize =====================
				if( IsLastTwoKnown( ) )
					{
					OptimizeBinaryOp(oper);
					return;
					}

				// Power operator optimize ======================
				if( oper == Code.Pow
				&&	PowOptimize
				&&	LastValue(_code, 1) == Code.Number
				&&	LastValue(_code, 2) == Code.Argument )
					{
					int val = GetIntegerValue(LastNumber);
					if( val > 0 && val < 16 )
						{
						OptimizePow(val);
						return;
						}
					}
				}

			_code.Add(oper);
			}

		public new void PutFunction( MethodInfo func )
			{
			if( FuncionFolding )
				{
				int pos = _code.Count - 1;
				bool argsKnown = true;

				while( !IsCallBegin(pos) )
					{
					if( _code[pos--] == Code.Number )
						{
						if( _code[pos] == Code.Separator ) pos--;
						}
					else
						{
						argsKnown = false;
						break;
						}
					}

				if( argsKnown )
					{
					OptimizeFunc(func, pos);
					return;
					}
				}

			base.PutFunction(func);
			}

		public new void PutExprEnd( )
			{
			WriteTo(_output);
			_output.PutExprEnd( );
			}

		#endregion
		#region Optimizations

		private void OptimizeNegate( )
			{
			_interp.PutNumber(LastNumber);
			_interp.PutOperator(Code.Neg);

			LastNumber = _interp.Result;

			_interp.Clear( );
			}

		private void OptimizeBinaryOp( int oper )
			{
			_interp.PutNumber(LastValue(_nums, 2));
			_interp.PutNumber(LastValue(_nums, 1));
			_interp.PutOperator(oper);

			RemoveLast(_nums);
			RemoveLast(_code);

			LastNumber = _interp.Result;

			_interp.Clear( );
			}

		private void OptimizeFunc( MethodInfo func, int start )
			{
			int numIdx = CountNumberShift(start);
			int numStart = numIdx;

			if( _code[start] == Code.ParamCall )
				{
				int varCount = PopLast(_data);
				int fixCount = PopLast(_data);
				_interp.BeginCall(fixCount, varCount);
				}
			else _interp.BeginCall(-1, 0);

			for( int i = start + 1; i < _code.Count; i++ )
				{
				if( _code[i] == Code.Separator )
					 _interp.PutSeparator( );
				else _interp.PutNumber(_nums[numIdx++]);
				}

			_interp.PutFunction(func);

			_nums.RemoveRange(numStart, numIdx - numStart);
			_code.RemoveRange(start, _code.Count - start);

			PutNumber(_interp.Result);
			_interp.Clear( );
			}

		private void OptimizePow( int val )
			{
			RemoveLast(_nums);
			RemoveLast(_code);
			int argId = LastValue(_data, 1);

			for( int i = 1; i < val; i++ )
				{
				PutArgument(argId);
				base.PutOperator(Code.Mul);
				}
			}

		private int CountNumberShift( int pos )
			{
			int count = 0;
			for( int i = 0; i < pos; i++ )
				{
				if( _code[i] == Code.Number ) count++;
				}

			return count;
			}

		// NOTE: bring out of here in the future
		private static int GetIntegerValue( double value )
			{
			int intVal = ( int ) value;
			return (intVal == value) ? intVal : -1;
			}

		#endregion
		}
	}