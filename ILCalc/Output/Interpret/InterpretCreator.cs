using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
{
	internal sealed class InterpretCreator : IExpressionOutput
	{
		#region Fields

		private readonly List<int> code;
		private readonly List<double> numbers;
		private readonly List<FuncCall> funcs;
#if !CF2
		private readonly List<Delegate> delegates;
#endif

		private int stackMax;
		private int stackSize;

		#endregion
		#region Constructor

		public InterpretCreator()
		{
			this.code = new List<int>(8);
			this.funcs = new List<FuncCall>();
			this.numbers = new List<double>(4);
#if !CF2
			this.delegates = new List<Delegate>();
#endif
		}

		#endregion
		#region Properties

		public int[]      GetCodes()     { return this.code.ToArray(); }
		public double[]   GetNumbers()   { return this.numbers.ToArray(); }
		public FuncCall[] GetFunctions() { return this.funcs.ToArray(); }

#if !CF2

		public Delegate[] GetDelegates() { return this.delegates.ToArray(); }
#endif

		public int StackMax
		{
			get { return this.stackMax; }
		}

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			this.numbers.Add(value);
			this.code.Add(Code.Number);

			if (++this.stackSize > this.stackMax)
			{
				this.stackMax = this.stackSize;
			}
		}

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);

			this.code.Add(Code.Argument);
			this.code.Add(id);

			if (++this.stackSize > this.stackMax)
			{
				this.stackMax = this.stackSize;
			}
		}

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));

			this.code.Add(oper);

			if (oper != Code.Neg)
			{
				this.stackSize--;
			}
		}

		public void PutSeparator() { }
		public void PutBeginCall() { }

		// TODO: reuse code!
		public void PutFunction(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);

#if CF2
			this.code.Add(Code.Function);
			this.code.Add(this.AppendFunc(func, argsCount));
			this.RecalcStackSize(argsCount);
#else
			if (func.HasParamArray || func.ArgsCount > 2)
			{
				this.code.Add(Code.Function);
				this.code.Add(this.AppendFunc(func, argsCount));
				this.RecalcStackSize(argsCount);
				return;
			}

			switch (func.ArgsCount)
			{
				case 0:
					this.code.Add(Code.Delegate0);
					this.code.Add(this.AppendDelegate(func, EvalType0));
					if (++this.stackSize > this.stackMax)
					{
						this.stackMax = this.stackSize;
					}

					break;

				case 1:
					this.code.Add(Code.Delegate1);
					this.code.Add(this.AppendDelegate(func, EvalType1));
					break;

				case 2:
					this.code.Add(Code.Delegate2);
					this.code.Add(this.AppendDelegate(func, EvalType2));
					this.stackSize--;
					break;
			}
#endif
		}

		public void PutExprEnd()
		{
			this.code.Add(Code.Return);
			this.code.Add(0); // fictive code
		}

		#endregion
		#region Helpers

		private void RecalcStackSize(int argsCount)
		{
			Debug.Assert(argsCount >= 0);

			if (argsCount == 0)
			{
				if (++this.stackSize > this.stackMax)
				{
					this.stackMax = this.stackSize;
				}
			}
			else
			{
				this.stackSize -= argsCount - 1;
			}
		}

#if !CF2

		private int AppendDelegate(FunctionItem func, Type delegateType)
		{
			Debug.Assert(func != null);
			Debug.Assert(delegateType != null);

			for (int i = 0; i < this.delegates.Count; i++)
			{
				if (this.delegates[i].Method == func.Method)
				{
					return i;
				}
			}

			this.delegates.Add(
				Delegate.CreateDelegate(delegateType, func.Target, func.Method));

			return this.delegates.Count - 1;
		}

#endif

		private int AppendFunc(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);

			for (int i = 0; i < this.funcs.Count; i++)
			{
				if (this.funcs[i].IsReusable(func, argsCount))
				{
					return i;
				}
			}

			this.funcs.Add(new FuncCall(func, argsCount));
			return this.funcs.Count - 1;
		}

		#endregion
		#region Static Data

		// Types ==================================================
		private static readonly Type EvalType0 = typeof(EvalFunc0);
		private static readonly Type EvalType1 = typeof(EvalFunc1);
		private static readonly Type EvalType2 = typeof(EvalFunc2);

		#endregion

		public Interpret Create(string expression, int argsCount, bool checks)
		{
			return new Interpret(expression, argsCount, checks, this);
		}
	}
}