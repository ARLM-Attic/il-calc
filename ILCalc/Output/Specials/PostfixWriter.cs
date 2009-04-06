#if DEBUG
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	sealed class PostfixWriter : IExpressionOutput
		{
		#region Fields

		private readonly StringBuilder _buf;
		private readonly IList<string> _args;

		private const string _ops = "-+*/%^¬";

		#endregion
		#region Constructor

		public PostfixWriter( IList<string> argsList )
			{
			_buf  = new StringBuilder();
			_args = argsList;
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			_buf.Append(value);
			_buf.Append(' ');
			}

		public void PutFunction( MethodInfo func )
			{
			_buf.Append(") ");
			_buf.Append(func.Name);
			_buf.Append(' ');
			}

		public void PutOperator( int oper )
			{
			_buf.Append(_ops[oper]);
			_buf.Append(' ');
			}

		public void PutArgument( int id )
			{
			_buf.Append(_args[id]);
			_buf.Append(' ');
			}

		public void PutSeparator( )
			{
			_buf.Append("; ");
			}

		public void BeginCall( int fixCount, int varCount )
			{
			_buf.Append("( ");
			}

		public void PutExprEnd( )
			{
//			int index = _buf.Length - 1;
//			_buf.Remove(index, 1);
			_buf.Append(';');
			}

		#endregion
		#region Members

		public override string ToString( )
			{
			return _buf.ToString( );
			}

		#endregion
		}

	}

#endif