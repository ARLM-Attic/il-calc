using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ILCalc
{
	internal abstract class CompilerBase
	{
		#region Fields

		// general:
		protected readonly DynamicMethod dynMethod;
		protected readonly ILGenerator il;
		protected readonly int argsCount;

		// method calls:
		private readonly bool checkedMode;
		private readonly Stack<CallInfo> callsStack;
		private bool useParams;

		#endregion
		#region Constructor

		//TODO: good owner
		protected CompilerBase(int argsCount, Type returnType, Type[] paramTypes, bool check)
		{
			Debug.Assert(argsCount >= 0);
			Debug.Assert(paramTypes != null);

			this.checkedMode = check;
			this.argsCount = argsCount;
			this.callsStack = new Stack<CallInfo>(2);

			this.dynMethod = new DynamicMethod(
				"ilcalc", returnType, paramTypes, TypeHelper.ValueType, true);

			this.il = this.dynMethod.GetILGenerator();
		}

		#endregion
		#region IExpressionOutput

		public void PutNumber(double value)
		{
			this.il.Emit(OpCodes.Ldc_R8, value);
		}

		public void PutOperator(int oper)
		{
			Debug.Assert(Code.IsOperator(oper));

			if (oper != Code.Pow)
			{
				this.il.Emit(OpOperators[oper]);
			}
			else
			{
				this.il.Emit(OpCodes.Call, PowMethod);
			}
		}

		public void PutSeparator()
		{
			if (this.useParams)
			{
				var info = this.callsStack.Peek();

				Debug.Assert(info != null);

				if (info.NextIsLastFixed())
				{
					this.EmitParamArr(info.VarCount, info.Local);
				}
				else if (info.Current > 0)
				{
					this.il.Emit(OpSaveElem);
					this.il.Emit(OpCodes.Ldloc, info.Local);
					EmitLoadI4(this.il, info.Current);
				}
			}
		}

		public void PutBeginCall()
		{
			this.callsStack.Push(null);
			this.useParams = false;
		}

		public void PutBeginParams(int fixCount, int varCount)
		{
			Debug.Assert(fixCount >= 0);
			Debug.Assert(varCount >= 0);

			LocalBuilder local = (varCount > 0) ?
				this.il.DeclareLocal(TypeHelper.ArrayType) : null;

			var info = new CallInfo(fixCount, varCount, local);
			this.callsStack.Push(info);

			if (fixCount == 0 && varCount > 0)
			{
				this.EmitParamArr(varCount, local);
			}

			this.useParams = true;
		}

		public void PutFunction(FunctionItem func, int argzCount)
		{
			Debug.Assert(func != null);

			var info = this.callsStack.Pop();

			if (func.HasParamArray)
			{
				Debug.Assert(info != null);

				if (info.VarCount > 0)
				{
					this.il.Emit(OpSaveElem);
					this.il.Emit(OpCodes.Ldloc, info.Local);
				}
				else
				{
					EmitLoadI4(this.il, 0);
					this.il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
				}
			}

			// NOTE: replace with Callvirt when impl instance calls
			this.il.Emit(OpCodes.Call, func.Method);

			if (this.callsStack.Count > 0)
			{
				this.useParams = this.callsStack.Peek() != null;
			}
		}

		public void PutExprEnd()
		{
			// il.Emit(OpCodes.Call, typeof(Debugger).GetMethod("Break"));
			if (this.checkedMode)
			{
				this.il.Emit(OpCodes.Ckfinite);
			}
		}

		#endregion
		#region Helpers

		protected static void EmitLoadI4(ILGenerator body, int value)
		{
			if (value < sbyte.MinValue
			 || value > sbyte.MaxValue)
			{
				body.Emit(OpCodes.Ldc_I4, value);
			}
			else if (value < -1 || value > 8)
			{
				body.Emit(OpCodes.Ldc_I4_S, (byte) value);
			}
			else
			{
				body.Emit(OpLoadConst[value + 1]);
			}
		}

		private void EmitParamArr(int size, LocalBuilder local)
		{
			Debug.Assert(size >= 0);
			Debug.Assert(local != null);

			EmitLoadI4(this.il, size);
			this.il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
			this.il.Emit(OpCodes.Stloc, local);
			this.il.Emit(OpCodes.Ldloc, local);
			EmitLoadI4(this.il, 0);
		}

		#endregion
		#region Static Data

		// OpCodes ================================================
		protected static readonly OpCode OpLoadElem = OpCodes.Ldelem_R8;
		protected static readonly OpCode OpSaveElem = OpCodes.Stelem_R8;

		private static readonly OpCode[] OpLoadConst =
			{
				OpCodes.Ldc_I4_M1,
				OpCodes.Ldc_I4_0,
				OpCodes.Ldc_I4_1,
				OpCodes.Ldc_I4_2,
				OpCodes.Ldc_I4_3,
				OpCodes.Ldc_I4_4,
				OpCodes.Ldc_I4_5,
				OpCodes.Ldc_I4_6,
				OpCodes.Ldc_I4_7,
				OpCodes.Ldc_I4_8
			};

		private static readonly OpCode[] OpOperators =
			{
				OpCodes.Sub,
				OpCodes.Add,
				OpCodes.Mul,
				OpCodes.Div,
				OpCodes.Rem,
				OpCodes.Nop,
				OpCodes.Neg
			};

		private static readonly MethodInfo PowMethod
			= typeof(Math).GetMethod("Pow");

		#endregion
		#region CallInfo

		private sealed class CallInfo
		{
			private readonly LocalBuilder local;
			private readonly int varCount;
			private int current;

			public CallInfo(int fixCount, int varCount, LocalBuilder local)
			{
				this.varCount = varCount;
				this.current = -fixCount;
				this.local = local;
			}

			public LocalBuilder Local
			{
				get { return this.local; }
			}

			public int VarCount
			{
				get { return this.varCount; }
			}

			public int Current
			{
				get { return this.current; }
			}

			public bool NextIsLastFixed()
			{
				return ++this.current == 0;
			}
		}

		#endregion

//		public class EvaluatorOwner
//		{
//			private object[] closures;
//
//			public EvaluatorOwner(List<object> closures)
//			{
//				this.closures = closures.ToArray();
//			}
//
//			public static Type MeType = typeof(EvaluatorOwner);
//			public static Type ObjArray = typeof(object[]);
//		}
	}
}