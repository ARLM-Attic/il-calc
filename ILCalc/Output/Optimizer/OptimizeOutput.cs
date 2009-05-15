using System.Collections.Generic;
using System.Diagnostics;

// TODO: feature x ^  0		=> 1
// TODO: feature x ^ -2		=> 1 x x * /
// TODO: feature x *  0		=> 0
// TODO: feature 2 + x + 2	=> 4 x +
namespace ILCalc
{
	internal sealed class OptimizeOutput : BufferOutput, IExpressionOutput
	{
		#region Fields

		private readonly IExpressionOutput output;
		private readonly QuickInterpret interp;
		private readonly OptimizeModes mode;

		#endregion
		#region Constructor

		public OptimizeOutput(IExpressionOutput output, OptimizeModes mode)
		{
			Debug.Assert(output != null);

			this.output = output;
			this.mode = mode;

			this.interp = new QuickInterpret(null, false);
		}

		#endregion
		#region Properties

		private bool ConstantFolding
		{
			get { return (this.mode & OptimizeModes.ConstantFolding) != 0; }
		}

		private bool FuncionFolding
		{
			get { return (this.mode & OptimizeModes.FunctionFolding) != 0; }
		}

		private bool PowOptimize
		{
			get { return (this.mode & OptimizeModes.PowOptimize) != 0; }
		}

		private double LastNumber
		{
			get { return this.numbers[this.numbers.Count - 1];  }
			set { this.numbers[this.numbers.Count - 1] = value; }
		}

		#endregion
		#region IExpressionOutput

		public new void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));

			if (this.ConstantFolding)
			{
				// Unary operator optimize ======================
				if (oper == Code.Neg && this.IsLastKnown())
				{
					this.PerformNegate();
					return;
				}

				// Binary operator optimize =====================
				if (this.IsLastTwoKnown())
				{
					this.PerformBinaryOp(oper);
					return;
				}

				// Power operator optimize ======================
				Debug.Assert(this.code.Count >= 2);

				if (oper == Code.Pow
				 && this.PowOptimize
				 && LastValue(code, 1) == Code.Number
				 && LastValue(code, 2) == Code.Argument)
				{
					int value = GetIntegerValue(this.LastNumber);
					if (value > 0 && value < 16)
					{
						this.OptimizePow(value);
						return;
					}
				}
			}

			code.Add(oper);
		}

		public new void PutFunction(FunctionItem func, int argsCount)
		{
			if (this.FuncionFolding)
			{
				int pos = code.Count - 1;
				bool allArgsKnown = true;

				while (!this.IsCallBegin(pos))
				{
					if (code[pos--] == Code.Number)
					{
						if (code[pos] == Code.Separator)
						{
							pos--;
						}
					}
					else
					{
						allArgsKnown = false;
						break;
					}
				}

				if (allArgsKnown)
				{
					this.FoldFunction(pos, func, argsCount);
					return;
				}
			}

			base.PutFunction(func, argsCount);
		}

		public new void PutExprEnd()
		{
			this.WriteTo(this.output);
			this.output.PutExprEnd();
		}

		#endregion
		#region Helpers

		// NOTE: bring out of here
		private static int GetIntegerValue(double value)
		{
			var intVal = (int) value;
			return (intVal == value) ? intVal : -1;
		}

		// ReSharper disable SuggestBaseTypeForParameter

		private static T LastValue<T>(List<T> list, int id)
		{
			Debug.Assert(id <= list.Count);
			return list[list.Count - id];
		}

		private static void RemoveLast<T>(List<T> list)
		{
			Debug.Assert(list.Count >= 1);
			list.RemoveAt(list.Count - 1);
		}

		// ReSharper restore SuggestBaseTypeForParameter

		private bool IsLastKnown()
		{
			Debug.Assert(code.Count >= 1);
			return code[code.Count - 1] == Code.Number;
		}

		private bool IsLastTwoKnown()
		{
			Debug.Assert(code.Count >= 2);
			int index = code.Count;
			return code[index - 1] == Code.Number
				&& code[index - 2] == Code.Number;
		}

		private bool IsCallBegin(int pos)
		{
			Debug.Assert(code.Count >= 1);

			int op = code[pos];
			return op == Code.ParamCall
				|| op == Code.BeginCall;
		}

		#endregion
		#region Optimizations

		private void PerformNegate()
		{
			Debug.Assert(this.numbers.Count >= 1);

			this.interp.PutNumber(this.LastNumber);
			this.interp.PutOperator(Code.Neg);

			this.LastNumber = this.interp.Result;

			this.interp.Reset();
		}

		private void PerformBinaryOp(int oper)
		{
			Debug.Assert(this.numbers.Count >= 2);
			Debug.Assert(this.code.Count >= 1);

			this.interp.PutNumber(LastValue(this.numbers, 2));
			this.interp.PutNumber(LastValue(this.numbers, 1));
			this.interp.PutOperator(oper);

			RemoveLast(this.numbers);
			RemoveLast(code);

			this.LastNumber = this.interp.Result;

			this.interp.Reset();
		}

		private void FoldFunction(int start, FunctionItem func, int argsCount)
		{
			Debug.Assert(start >= 0);
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);
			Debug.Assert(argsCount <= this.numbers.Count);

			int numIndex = this.numbers.Count - argsCount;
			var stack = new double[argsCount];
			this.numbers.CopyTo(numIndex, stack, 0, argsCount);

			if (code[start] == Code.ParamCall)
			{
				Debug.Assert(data.Count >= 2);
				data.RemoveRange(data.Count - 2, 2);
			}

			Debug.Assert(this.code.Count > start);
			this.code.RemoveRange(start, code.Count - start);

			if (argsCount > 0)
			{
				Debug.Assert(this.numbers.Count > numIndex);
				this.numbers.RemoveRange(numIndex, argsCount);
			}

			this.PutNumber(func.Invoke(stack, argsCount));
		}

		private void OptimizePow(int value)
		{
			Debug.Assert(value > 0);
			Debug.Assert(this.numbers.Count >= 1);
			Debug.Assert(this.code.Count >= 1);

			RemoveLast(this.numbers);
			RemoveLast(code);
			int argumentId = LastValue(data, 1);

			for (int i = 1; i < value; i++)
			{
				PutArgument(argumentId);
				base.PutOperator(Code.Mul);
			}
		}

		#endregion
	}
}