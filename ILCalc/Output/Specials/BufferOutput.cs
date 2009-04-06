using System.Collections.Generic;
using System.Reflection;

namespace ILCalc
	{
	class BufferOutput : IExpressionOutput
		{
		#region Fields

		protected readonly List<MethodInfo> _funs;
		protected readonly List<double> _nums;
		protected readonly List<int> _code;
		protected readonly List<int> _data;
		
		#endregion
		#region Constructor

		public BufferOutput()
			{
			_funs = new List<MethodInfo>( );
			_nums = new List<double>(4);
			_code = new List<int>(8);
			_data = new List<int>(2);
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			_code.Add(Code.Number);
			_nums.Add(value);
			}

		public void PutFunction( MethodInfo func )
			{
			_code.Add(Code.Function);
			_funs.Add(func);
			}

		public void PutOperator( int oper )
			{
			_code.Add(oper);
			}

		public void PutArgument( int id )
			{
			_code.Add(Code.Argument);
			_data.Add(id);
			}

		public void PutSeparator( )
			{
			_code.Add(Code.Separator);
			}

		public void BeginCall( int fixCount, int varCount )
			{
			if( fixCount >= 0 )
				{
				_code.Add(Code.ParamCall);
				_data.Add(fixCount);
				_data.Add(varCount);
				}
			else _code.Add(Code.BeginCall);
			}

		public void PutExprEnd( )
			{
			_code.Add(Code.Return);
			}

		#endregion
		#region Members

		public void WriteTo( IExpressionOutput output )
			{
			int numbPos = 0,
				funcPos = 0,
				dataPos = 0;

			for( int i = 0; i < _code.Count; i++ )
				{
				int code = _code[i];

				if( Code.IsOperator(code)		) output.PutOperator(code);
				else if( code == Code.Number	) output.PutNumber(_nums[numbPos++]);
				else if( code == Code.Argument	) output.PutArgument(_data[dataPos++]);
				else if( code == Code.Function	) output.PutFunction(_funs[funcPos++]);
				else if( code == Code.Separator	) output.PutSeparator( );
				else if( code == Code.BeginCall ) output.BeginCall(-1, 0);
				else if( code == Code.ParamCall	) output.BeginCall(_data[dataPos++], _data[dataPos++]);
				else output.PutExprEnd( );
				}
			}

		#endregion
		}
	}