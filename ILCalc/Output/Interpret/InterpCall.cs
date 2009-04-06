using System;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
	{
	[Serializable]
	sealed partial class InterpCall
		{
		#region Fields

		private readonly MethodInfo method;
		private readonly int argLastIdx;
		[NonSerialized] private readonly object[] fixArgs;
		[NonSerialized] private readonly double[] varArgs;

		#endregion
		#region Property

		public MethodInfo Method
			{
			[DebuggerHidden]
			get { return method; }
			}

		#endregion
		#region Constructors

		public InterpCall( MethodInfo method, int argc, int argv )
			{
			this.method = method;

			fixArgs = new object[argc + 1];
			varArgs = new double[argv];
			fixArgs[argc] = varArgs;

			argLastIdx = argc - 1;
			}

		public InterpCall( MethodInfo method, int argc )
			{
			this.method = method;

			fixArgs = new object[argc];
			argLastIdx = argc - 1;
			}

		#endregion
		#region Members

		public void InvokeFunc( double[] stack, ref int pos )
			{
			// params array
			if(varArgs != null)
				{
				for(int i = varArgs.Length - 1; i >= 0; i--)
					{
					varArgs[i] = stack[pos--];
					}
				}

			// standart args
			object[] fix = fixArgs;
			for(int i = argLastIdx; i >= 0; i--)
				{
				fix[i] = stack[pos--];
				}

			// invoke via reflection
			stack[++pos] = (double) method.Invoke(null, fix);
			}

		#endregion
		}
	}