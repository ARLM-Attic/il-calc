using System;
using System.Diagnostics;
using System.Threading;

namespace ILCalc
{
	// TODO: rewrite to use range checks elimination

	[Serializable]
	internal sealed partial class FuncCall
	{
		#region Fields

		private readonly FunctionItem func;
		private readonly int lastIndex; // TODO: without it?

		private readonly object[] fixArgs;
		private readonly double[] varArgs;
		private readonly object syncRoot;
		private readonly int argsCount;

		#endregion
		#region Constructor

		public FuncCall(FunctionItem func, int argsCount)
		{
			Debug.Assert(func != null);
			Debug.Assert(argsCount >= 0);
			Debug.Assert(
				( func.HasParamArray && func.ArgsCount <= argsCount) ||
				(!func.HasParamArray && func.ArgsCount == argsCount));

			int fixCount = func.ArgsCount;

			if (func.HasParamArray)
			{
				this.varArgs = new double[argsCount - fixCount];
				this.fixArgs = new object[fixCount + 1];
				this.fixArgs[fixCount] = this.varArgs;
			}
			else
			{
				this.fixArgs = new object[fixCount];
			}

			this.func = func;
			this.lastIndex = fixCount - 1;
			this.argsCount = argsCount;
			this.syncRoot = new object();
		}

		#endregion
		#region Methods

		public bool IsReusable(FunctionItem other, int otherArgsCount)
		{
			Debug.Assert(other != null);
			Debug.Assert(otherArgsCount >= 0);

			// NOTE: modify when impl instance calls
			return this.func.Method == other.Method
				&& this.argsCount == otherArgsCount;

		}

		public void Invoke(double[] stack, ref int pos)
		{
			Debug.Assert(stack != null);
			Debug.Assert(stack.Length > pos);

			if (Monitor.TryEnter(this.syncRoot))
			{
				try
				{
					// fill parameters array:
					if (this.varArgs != null)
					{
						for (int i = this.varArgs.Length - 1; i >= 0; i--)
						{
							this.varArgs[i] = stack[pos--];
						}
					}

					// fill arguments:
					object[] fixTemp = this.fixArgs;
					for (int i = this.lastIndex; i >= 0; i--)
					{
						fixTemp[i] = stack[pos--];
					}

					// invoke via reflection:
					stack[++pos] = this.func.Invoke(fixTemp);
				}
				finally
				{
					Monitor.Exit(this.syncRoot);
				}
			}
			else
			{
				double result = this.func.Invoke(stack, pos, this.argsCount);
				pos -= this.argsCount - 1;
				stack[pos] = result; // TODO: is all right here?
			}
		}

		#endregion
	}
}