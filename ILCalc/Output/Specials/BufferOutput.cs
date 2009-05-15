using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
{
	internal class BufferOutput : IExpressionOutput
	{
		#region Fields

		protected readonly List<FunctionItem> functions;
		protected readonly List<double> numbers;
		protected readonly List<int> code;
		protected readonly List<int> data;

		#endregion
		#region Constructor

		public BufferOutput()
		{
			this.functions = new List<FunctionItem>(2);
			this.numbers = new List<double>(4);
			this.code = new List<int>(8);
			this.data = new List<int>(2);
		}

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			this.code.Add(Code.Number);
			this.numbers.Add(value);
		}

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));

			this.code.Add(oper);
		}

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);

			this.code.Add(Code.Argument);
			this.data.Add(id);
		}

		public void PutSeparator()
		{
			this.code.Add(Code.Separator);
		}

		public void PutBeginCall()
		{
			this.code.Add(Code.BeginCall);
		}

		public void PutBeginParams(int fixCount, int varCount)
		{
			this.code.Add(Code.ParamCall);
			this.data.Add(fixCount);
			this.data.Add(varCount);
		}

		public void PutFunction(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);

			this.code.Add(Code.Function);
			this.data.Add(argsCount);
			this.functions.Add(func);
		}

		public void PutExprEnd()
		{
			this.code.Add(Code.Return);
		}

		#endregion
		#region Methods

		public void WriteTo(IExpressionOutput output)
		{
			int n = 0, f = 0, d = 0;

			for (int i = 0; i < this.code.Count; i++)
			{
				int op = this.code[i];

				if (Code.IsOperator(op))
				{
					output.PutOperator(op);
				}
				else if (op == Code.Number)
				{
					output.PutNumber(this.numbers[n++]);
				}
				else if (op == Code.Argument)
				{
					output.PutArgument(this.data[d++]);
				}
				else if (op == Code.Function)
				{
					output.PutFunction(this.functions[f++], this.data[d++]);
				}
				else if (op == Code.Separator)
				{
					output.PutSeparator();
				}
				else if (op == Code.BeginCall)
				{
					output.PutBeginCall();
				}
				else if (op == Code.ParamCall)
				{
					output.PutBeginParams(this.data[d++], this.data[d++]);
				}
				else
				{
					output.PutExprEnd();
				}
			}
		}

		#endregion
	}
}