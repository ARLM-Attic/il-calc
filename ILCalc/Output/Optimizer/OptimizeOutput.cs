using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	//TODO: feature x ^  0		=> 1
	//TODO: feature x ^ -2		=> 1 x x * /
	//TODO: feature x *  0		=> 0
	//TODO: feature 2 + x + 2	=> 4 x +

	sealed class OptimizeOutput : BufferOutput, IExpressionOutput
		{
		#region Fields

		private readonly IExpressionOutput output;
		private readonly QuickInterpret interp;
		private readonly OptimizeModes mode;

		#endregion
		#region Constructor

		public OptimizeOutput( IExpressionOutput output, OptimizeModes mode )
			{
			this.output = output;
			this.mode = mode;

			interp = new QuickInterpret(null, false);
			}

		#endregion
		#region Properties

		private bool ConstantFolding
			{
			get { return (mode & OptimizeModes.ConstantFolding) != 0; }
			}

		private bool FuncionFolding
			{
			get { return (mode & OptimizeModes.FunctionFolding) != 0; }
			}

		private bool PowOptimize
			{
			get { return (mode & OptimizeModes.PowOptimize) != 0; }
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
			return code[code.Count - 1] == Code.Number;
			}

		private bool IsLastTwoKnown( )
			{
			int index = code.Count;
			return code[index - 1] == Code.Number
				&& code[index - 2] == Code.Number;
			}

		private double LastNumber
			{
			get { return nums[nums.Count - 1];  }
			set { nums[nums.Count - 1] = value; }
			}

		private bool IsCallBegin( int pos )
			{
			int op = code[pos];
			return op == Code.ParamCall
				|| op == Code.BeginCall;
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
				&&	LastValue(code, 1) == Code.Number
				&&	LastValue(code, 2) == Code.Argument )
					{
					int val = GetIntegerValue(LastNumber);
					if( val > 0 && val < 16 )
						{
						OptimizePow(val);
						return;
						}
					}
				}

			code.Add(oper);
			}

		public new void PutMethod( MethodInfo method, int argsCount )
			{
			if( FuncionFolding )
				{
				int pos = code.Count - 1;
				bool argsKnown = true;

				while( !IsCallBegin(pos) )
					{
					if( code[pos--] == Code.Number )
						{
						if( code[pos] == Code.Separator ) pos--;
						}
					else
						{
						argsKnown = false;
						break;
						}
					}

				if( argsKnown )
					{
					OptimizeFunc(pos, method, argsCount);
					return;
					}
				}

			base.PutMethod(method, argsCount);
			}

		public new void PutExprEnd( )
			{
			WriteTo(output);
			output.PutExprEnd( );
			}

		#endregion
		#region Optimizations

		private void OptimizeNegate( )
			{
			interp.PutNumber(LastNumber);
			interp.PutOperator(Code.Neg);

			LastNumber = interp.Result;

			interp.Clear( );
			}

		private void OptimizeBinaryOp( int oper )
			{
			interp.PutNumber(LastValue(nums, 2));
			interp.PutNumber(LastValue(nums, 1));
			interp.PutOperator(oper);

			RemoveLast(nums);
			RemoveLast(code);

			LastNumber = interp.Result;

			interp.Clear( );
			}

		private void OptimizeFunc( int start, MethodInfo func, int argsCount )
			{
			int numIdx = CountNumberShift(start);
			int numStart = numIdx;

			if( code[start] == Code.ParamCall )
				{
				int varCount = PopLast(data);
				int fixCount = PopLast(data);

				interp.PutBeginParams(fixCount, varCount);
				}
			else
				interp.PutBeginCall( );

			for( int i = start + 1; i < code.Count; i++ )
				{
				if( code[i] == Code.Separator )
					 interp.PutSeparator( );
				else interp.PutNumber(nums[numIdx++]);
				}

			interp.PutMethod(func, argsCount);

			nums.RemoveRange(numStart, numIdx - numStart);
			code.RemoveRange(start, code.Count - start);

			PutNumber(interp.Result);
			interp.Clear( );
			}

		private void OptimizePow( int val )
			{
			RemoveLast(nums);
			RemoveLast(code);
			int argId = LastValue(data, 1);

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
				if( code[i] == Code.Number ) count++;
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