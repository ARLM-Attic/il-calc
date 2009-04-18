using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace ILCalc
	{
	//NOTE: maybe struct => more lightweight? Test.

	[Serializable]
	sealed partial class FuncCall
		{
		#region Fields

		private readonly MethodInfo method;
		private readonly int lastIndex;

		private readonly object[] fixArgs;
		private readonly double[] varArgs;
		private readonly object syncRoot;

		public MethodInfo Method
			{
			[DebuggerHidden] get { return method; }
			}

		#endregion
		#region Constructor

		public FuncCall( MethodInfo method, int fixCount, int varCount )
			{
			if( varCount >= 0 )
				{
				varArgs = new double[varCount];
				fixArgs = new object[fixCount + 1];
				fixArgs[fixCount] = varArgs;
				}
			else
				{
				fixArgs = new object[fixCount];
				varArgs = null;
				}

			this.method = method;
			lastIndex = fixCount - 1;
			syncRoot = new object( );
			}

		#endregion
		#region IsReusable

		public bool IsReusable( int fixCount, int varCount )
			{
			if( varArgs == null )
				{
				return varCount < 0
					&& fixCount == fixArgs.Length;
				}

			return varCount == varArgs.Length
				&& fixCount == fixArgs.Length - 1;
			}

		#endregion
		#region Invoke Method

		public void Invoke( double[] stack, ref int pos )
			{
			if( Monitor.TryEnter(syncRoot) )
				{
				// params array:
				if( varArgs != null )
					{
					for( int i = varArgs.Length - 1; i >= 0; i-- )
						{
						varArgs[i] = stack[pos--];
						}
					}

				// standart args:
				object[] fixTemp = fixArgs;
				for( int i = lastIndex; i >= 0; i-- )
					{
					fixTemp[i] = stack[pos--];
					}

				// invoke via reflection:
				try
					{
					stack[++pos] = (double)
						method.Invoke(null, fixTemp);
					}
				finally { Monitor.Exit(syncRoot); }
				}
			else
				InvokeSync(stack, ref pos);
			}

		private void InvokeSync( double[] stack, ref int pos )
			{
			var fixTemp = new object[fixArgs.Length];

			// params array:
			if( varArgs != null )
				{
				var varTemp = new double[varArgs.Length];
				for( int i = varArgs.Length - 1; i >= 0; i-- )
					{
					varTemp[i] = stack[pos--];
					}

				fixTemp[lastIndex + 1] = varTemp;
				}

			// standart args:
			for( int i = lastIndex; i >= 0; i-- )
				{
				fixTemp[i] = stack[pos--];
				}

			// invoke via reflection:
			stack[++pos] = (double)
				method.Invoke(null, fixTemp);
			}

		#endregion
		}
	}