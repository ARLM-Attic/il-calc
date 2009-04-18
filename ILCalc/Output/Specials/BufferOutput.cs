using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	class BufferOutput : IExpressionOutput
		{
		#region Fields

		protected readonly List<MethodInfo> funcs;
		protected readonly List<double> nums;
		protected readonly List<int> code;
		protected readonly List<int> data;
		
		#endregion
		#region Constructor

		public BufferOutput()
			{
			funcs = new List<MethodInfo>( );
			nums = new List<double>(4);
			code = new List<int>(8);
			data = new List<int>(2);
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			code.Add(Code.Number);
			nums.Add(value);
			}

		public void PutOperator( int oper ) { code.Add(oper); }

		public void PutArgument( int id )
			{
			code.Add(Code.Argument);
			data.Add(id);
			}

		public void PutSeparator( ) { code.Add(Code.Separator); }

		public void PutBeginCall( ) { code.Add(Code.BeginCall); }

		public void PutBeginParams( int fixCount, int varCount )
			{
			code.Add(Code.ParamCall);
			data.Add(fixCount);
			data.Add(varCount);
			}

		public void PutMethod( MethodInfo method, int fixCount )
			{
			code.Add(Code.Function);
			data.Add(fixCount);
			funcs.Add(method);
			}

		public void PutExprEnd( ) { code.Add(Code.Return); }

		#endregion
		#region Methods

		public void WriteTo( IExpressionOutput output )
			{
			int n = 0, f = 0, d = 0;

			for( int i = 0; i < code.Count; i++ )
				{
				int op = code[i];

				if( Code.IsOperator(op)       ) output.PutOperator(op);
				else if( op == Code.Number    ) output.PutNumber(nums[n++]);
				else if( op == Code.Argument  ) output.PutArgument(data[d++]);
				else if( op == Code.Function  ) output.PutMethod(funcs[f++], data[d++]);
				else if( op == Code.Separator ) output.PutSeparator( );
				else if( op == Code.BeginCall ) output.PutBeginCall( );
				else if( op == Code.ParamCall ) output.PutBeginParams(data[d++], data[d++]);
				else output.PutExprEnd( );
				}
			}

		#endregion
		}
	}