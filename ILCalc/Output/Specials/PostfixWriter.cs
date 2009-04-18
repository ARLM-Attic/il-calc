#if DEBUG
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	sealed class PostfixWriter : IExpressionOutput
		{
		#region Fields

		private readonly StringBuilder buffer;
		private readonly IList<string> argList;

		#endregion
		#region Constructor

		public PostfixWriter( IList<string> argsList )
			{
			buffer = new StringBuilder();
			argList = argsList;
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			buffer.Append(value);
			buffer.Append(' ');
			}

		public void PutOperator( int oper )
			{
			buffer.Append("-+*/%^¬"[oper]);
			buffer.Append(' ');
			}

		public void PutArgument( int id )
			{
			buffer.Append(argList[id]);
			buffer.Append(' ');
			}

		public void PutSeparator( ) { buffer.Append("; "); }
		public void PutBeginCall( ) { buffer.Append("( "); }

		public void PutBeginParams( int fixCount, int varCount )
			{
			buffer.Append("( ");
			}

		public void PutMethod( MethodInfo method, int argsCount )
			{
			buffer.Append(") ");
			buffer.Append(method.Name);
			buffer.Append(' ');
			}

		public void PutExprEnd( ) { buffer.Append(';'); }

		#endregion
		#region Methods

		public override string ToString( )
			{
			return buffer.ToString( );
			}

		#endregion
		}
	}

#endif