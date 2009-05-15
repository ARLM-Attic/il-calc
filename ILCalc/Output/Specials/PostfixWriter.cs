#if DEBUG
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
{
	internal sealed class PostfixWriter : IExpressionOutput
	{
		#region Fields

		private readonly StringBuilder buffer;
		private readonly IList<string> argList;

		#endregion
		#region Constructor

		public PostfixWriter(IList<string> argsList)
		{
			Debug.Assert(argsList != null);

			this.buffer = new StringBuilder();
			this.argList = argsList;
		}

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			this.buffer.Append(value);
			this.buffer.Append(' ');
		}

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));

			this.buffer.Append("-+*/%^¬"[oper]);
			this.buffer.Append(' ');
		}

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);

			this.buffer.Append(this.argList[id]);
			this.buffer.Append(' ');
		}

		public void PutSeparator()
		{
			this.buffer.Append("; ");
		}

		public void PutBeginCall()
		{
			this.buffer.Append("( ");
		}

		public void PutBeginParams(int fixCount, int varCount)
		{
			this.buffer.Append("( ");
		}

		public void PutFunction(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);

			this.buffer.Append(") ");
			this.buffer.Append(func.MethodName);
			this.buffer.Append(' ');
		}

		public void PutExprEnd()
		{
			this.buffer.Append(';');
		}

		#endregion
		#region Methods

		public override string ToString()
		{
			return this.buffer.ToString();
		}

		#endregion
	}
}

#endif