using System;
using System.Diagnostics;

namespace ILCalc
{
	internal sealed class QuickInterpret : IExpressionOutput
	{
		#region Fields

		private readonly double[] arguments;
		private readonly bool checkedMode;
		private double[] stack;
		private int pos;
		
		#endregion
		#region Constructor

		public QuickInterpret(double[] arguments, bool check)
		{
			this.stack = new double[4];
			this.arguments = arguments;
			this.checkedMode = check;
			this.pos = -1;
		}

		#endregion
		#region Members

		public double Result
		{
			get
			{
				Debug.Assert(this.pos == 0);
				return this.stack[0];
			}
		}

		public void Reset()
		{
			this.pos = -1;
		}

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			if (++this.pos == this.stack.Length)
			{
				var newStack = new double[this.pos * 2];
				Array.Copy(this.stack, 0, newStack, 0, this.pos);
				this.stack = newStack;
			}

			this.stack[this.pos] = value;
		}

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);

			if (++this.pos == this.stack.Length)
			{
				var newStack = new double[this.pos * 2];
				Array.Copy(this.stack, 0, newStack, 0, this.pos);
				this.stack = newStack;
			}

			this.stack[this.pos] = this.arguments[id];
		}

		public void PutBeginCall() { }
		public void PutSeparator() { }

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));
			Debug.Assert(this.pos >= 0);

			double value = this.stack[this.pos--];
			if (oper != Code.Neg)
			{
				Debug.Assert(this.pos >= 0);
				Debug.Assert(this.pos < this.stack.Length);

				if (oper == Code.Add)
				{
					this.stack[this.pos] += value;
				}
				else if (oper == Code.Mul)
				{
					this.stack[this.pos] *= value;
				}
				else if (oper == Code.Sub)
				{
					this.stack[this.pos] -= value;
				}
				else if (oper == Code.Div)
				{
					this.stack[this.pos] /= value;
				}
				else if (oper == Code.Rem)
				{
					this.stack[this.pos] %= value;
				}
				else
				{
					this.stack[this.pos] = Math.Pow(this.stack[this.pos], value);
				}
			}
			else
			{
				if (this.stack != null)
				{
					this.stack[++this.pos] = -value;
				}
			}
		}

		public void PutFunction(FunctionItem func, int argsCount)
		{
			Debug.Assert(this.pos + 1 >= argsCount);

			double result = func.Invoke(this.stack, this.pos, argsCount);
			this.pos -= argsCount;

			if (argsCount > 0)
			{
				this.stack[++this.pos] = result;
			}
			else
			{
				this.PutNumber(result);
			}
		}

		public void PutExprEnd()
		{
			if (this.checkedMode)
			{
				Debug.Assert(this.pos == -1);

				double res = this.stack[0];
				if (double.IsInfinity(res) || double.IsNaN(res))
				{
					throw new NotFiniteNumberException(res.ToString());
				}
			}
		}

		#endregion
	}
}