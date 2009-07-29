using System;
using System.Diagnostics;

namespace ILCalc
{
	internal sealed class InterpretCreator : IExpressionOutput
	{
		#region Fields

		private int numsPos;
		private int codesPos;
		private int funcsPos;

		private int[] codes;
		private double[] nums;
		private FuncCall[] funcs;

		private static readonly
			FuncCall[] EmptyCalls = new FuncCall[0];

#if !CF2

		private Delegate[] delegs;
		private int delegsPos;

		private static readonly
			Delegate[] EmptyDelegs = new Delegate[0];

#endif

		private int stackMax;
		private int stackSize;

		#endregion
		#region Constructor

		public InterpretCreator()
		{
			this.codes = new int[8];
			this.nums  = new double[4];

			this.funcs = EmptyCalls;
#if !CF2
			this.delegs = EmptyDelegs;
#endif
		}

		#endregion
		#region Properties

		public int[] Codes { get { return this.codes; } }
		public double[] Numbers { get { return this.nums; } }
		public FuncCall[] Functions { get { return this.funcs; } }
#if !CF2
		public Delegate[] Delegates { get { return this.delegs; } }
#endif

		public int StackMax { get { return this.stackMax; } }

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			AddCode(Code.Number);

			if (this.numsPos == this.nums.Length)
			{
				ExpandArray(ref this.nums);
			}

			this.nums[this.numsPos++] = value;

			if (++this.stackSize > this.stackMax)
			{
				this.stackMax = this.stackSize;
			}
		}

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);
			AddCodes(Code.Argument, id);

			if (++this.stackSize > this.stackMax)
			{
				this.stackMax = this.stackSize;
			}
		}

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));
			AddCode(oper);

			if (oper != Code.Neg)
			{
				this.stackSize--;
			}
		}

		public void PutSeparator() { }
		public void PutBeginCall() { }

		public void PutFunction(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);

			int index;
#if CF2
			index = AppendFunc(func, argsCount);
			AddCodes(Code.Function, index);
			RecalcStackSize(argsCount);
#else
			if (func.HasParamArray || func.ArgsCount > 2)
			{
				index = AppendFunc(func, argsCount);
				AddCodes(Code.Function, index);
				RecalcStackSize(argsCount);
				return;
			}

			switch (func.ArgsCount)
			{
				case 0:
					index = AppendDelegate(func, EvalType0);
					AddCodes(Code.Delegate0, index);

					if (++this.stackSize > this.stackMax)
					{
						this.stackMax = this.stackSize;
					}

					break;

				case 1:
					index = AppendDelegate(func, EvalType1);
					AddCodes(Code.Delegate1, index);
					break;

				case 2:
					index = AppendDelegate(func, EvalType2);
					AddCodes(Code.Delegate2, index);
					this.stackSize--;
					break;
			}
#endif
		}

		public void PutExprEnd()
		{
			AddCodes(Code.Return, 0);
			// 0 - fictive code
		}

		#endregion
		#region Helpers

		private void AddCode(int c)
		{
			if (this.codesPos == this.codes.Length)
			{
				ExpandArray(ref this.codes);
			}

			this.codes[this.codesPos++] = c;
		}

		private void AddCodes(int a, int b)
		{
			if (this.codesPos + 1 >= this.codes.Length)
			{
				ExpandArray(ref this.codes);
			}

			this.codes[this.codesPos++] = a;
			this.codes[this.codesPos++] = b;
		}

		private static void ExpandArray<T>(ref T[] src)
		{
			int size = (src.Length == 0) ? 4 : src.Length * 2;
			var dest = new T[size];

			if (src.Length > 0)
			{
				Array.Copy(src, 0, dest, 0, src.Length);
			}

			src = dest;
		}

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

		private int AppendDelegate(FunctionItem func, Type delegType)
		{
			Debug.Assert(func != null);
			Debug.Assert(delegType != null);

			if (this.delegsPos == this.delegs.Length)
			{
				ExpandArray(ref this.delegs);
			}

			this.delegs[this.delegsPos] =
				Delegate.CreateDelegate(
					delegType, func.Target, func.Method);

			return this.delegsPos++;
		}

#endif

		private int AppendFunc(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);

			if (this.funcsPos == this.funcs.Length)
			{
				ExpandArray(ref this.funcs);
			}

			this.funcs[this.funcsPos] =
				new FuncCall(func, argsCount);

			return this.funcsPos++;
		}

		#endregion
		#region Static Data

		private static readonly Type EvalType0 = typeof(EvalFunc0);
		private static readonly Type EvalType1 = typeof(EvalFunc1);
		private static readonly Type EvalType2 = typeof(EvalFunc2);

		#endregion
	}
}