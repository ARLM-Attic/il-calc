using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ILCalc
{
	// TODO: Reuse BufferWriter if Compiler + Optimizer
	// TODO: merge with Optimizer?
	// TODO: do not write this.functions at all

	internal abstract class CompilerBase : BufferOutput, IExpressionOutput
	{
		#region Fields

		private readonly List<FuncCall> calls;
		private readonly bool emitChecks;
		private List<object> closure;
		private bool useOwner;

		protected bool OwnerUsed
		{
			get { return useOwner; }
		}

		#endregion
		#region Constructor

		protected CompilerBase(bool checks)
		{
			this.emitChecks = checks;
			this.calls = new List<FuncCall>();
		}

		#endregion
		#region CodeGen

		protected void CodeGen(ILGenerator il)
		{
			int n = 0, d = 0, c = 0;
			Stack<FuncCall> stack = null;
			FuncCall current = null;

			for(int i = 0;; i++)
			{
				int op = this.code[i];

				if (Code.IsOperator(op))
				{
					EmitOperation(il, op);
				}
				else if (op == Code.Number) // ================================
				{
					double value = this.numbers[n++];
					EmitLoadConst(il, value);
				}
				else if (op == Code.Argument) // ==============================
				{
					int index = this.data[d++];
					EmitLoadArg(il, index);
				}
				else if (op == Code.Separator) // =============================
				{
					// separator needed only for params calls
					Debug.Assert(current != null);
					if (current.VarCount >= 0)
					{
						EmitSeparator(il, current);
					}
				}
				else if (op == Code.Function) // ==============================
				{
					EmitFunctionCall(il, current);

					// parent call info:
					if (stack == null || stack.Count == 0)
					     current = null;
					else current = stack.Pop();
				}
				else if (op == Code.BeginCall) // =============================
				{
					if (current != null)
					{
						// allocate if needed
						if (stack == null) stack = new Stack<FuncCall>();
						stack.Push(current);
					}

					current = this.calls[c++];

					if (current.VarCount > 0)
					{
						// need for local to store params array:
						current.Local = il.DeclareLocal(TypeHelper.ArrayType);
					}

					if (current.Target != null)
					{
						EmitLoadTarget(il, current.Target);
					}

					if (current.Current == 0
					 && current.VarCount > 0)
					{
						EmitParamArr(il, current);
					}
				}
				else // =======================================================
				{
					//il.Emit(OpCodes.Call, typeof(Debugger).GetMethod("Break"));
					if (this.emitChecks)
					{
						il.Emit(OpCodes.Ckfinite);
					}

					break;
				}
			}
		}

		#endregion
		#region Emitters

		private void EmitLoadTarget(ILGenerator il, object target)
		{
			Debug.Assert(il != null);
			Debug.Assert(target != null);

			if (this.closure == null)
				this.closure = new List<object>();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, ClosureField);
			il_EmitLoadI4(il, this.closure.Count);
			il.Emit(OpCodes.Ldelem_Ref);

			Type targetType = target.GetType();

			if (targetType.IsValueType)
				il.Emit(OpCodes.Unbox_Any, targetType);
			else
				il.Emit(OpCodes.Castclass, targetType);

			this.closure.Add(target);
		}

		private static void EmitLoadConst(ILGenerator il, double value)
		{
			Debug.Assert(il != null);

			il.Emit(OpCodes.Ldc_R8, value);
		}

		protected abstract void EmitLoadArg(ILGenerator il, int index);

		private static void EmitOperation(ILGenerator il, int op)
		{
			Debug.Assert(il != null);
			Debug.Assert(Code.IsOperator(op));

			if (op != Code.Pow)
				il.Emit(OpOperators[op]);
			else
				il.Emit(OpCodes.Call, PowMethod);
		}

		private static void EmitSeparator(ILGenerator il, FuncCall call)
		{
			Debug.Assert(il != null);
			Debug.Assert(call != null);

			if (call.NextIsLastFixed())
			{
				EmitParamArr(il, call);
			}
			else if (call.Current > 0)
			{
				il.Emit(OpSaveElem);
				il.Emit(OpCodes.Ldloc, call.Local);
				il_EmitLoadI4(il, call.Current);
			}
		}

		private static void EmitFunctionCall(ILGenerator il, FuncCall call)
		{
			Debug.Assert(il != null);

			if (call.VarCount >= 0)
			{
				Debug.Assert(call != null);

				if (call.VarCount > 0)
				{
					il.Emit(OpSaveElem);
					il.Emit(OpCodes.Ldloc, call.Local);
				}
				else
				{
					il_EmitLoadI4(il, 0);
					il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
				}
			}

			if (call.Target == null)
				il.Emit(OpCodes.Call, call.Method);
			else
				il.Emit(OpCodes.Callvirt, call.Method);
		}

		private static void EmitParamArr(ILGenerator il, FuncCall call)
		{
			il_EmitLoadI4(il, call.VarCount);
			il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
			il.Emit(OpCodes.Stloc, call.Local);
			il.Emit(OpCodes.Ldloc, call.Local);
			il_EmitLoadI4(il, 0);
		}

		// ReSharper disable InconsistentNaming

		protected static void il_EmitLoadI4(ILGenerator il, int value)
		{
			if (value < sbyte.MinValue ||
			    value > sbyte.MaxValue)
			{
				il.Emit(OpCodes.Ldc_I4, value);
			}
			else if (value < -1 || value > 8)
			{
				il.Emit(OpCodes.Ldc_I4_S, (byte) value);
			}
			else
			{
				il.Emit(OpLoadConst[value + 1]);
			}
		}

		protected void il_EmitLoadArg(ILGenerator il, int index)
		{
			Debug.Assert(index >= 0);

			if (OwnerUsed) index++;

			if (index <= 3)
				il.Emit(OpArgsLoad[index]);
			else
				il.Emit(OpCodes.Ldarg_S, (byte) index);
		}

		// ReSharper restore InconsistentNaming

		#endregion
		#region FuncCall

		private sealed class FuncCall
		{
			private int current;

			public FuncCall(FunctionItem func, int argsCount)
			{
				Target = func.Target;
				Method = func.Method;

				if (func.HasParamArray)
				{
					this.current = -func.ArgsCount;
					VarCount = argsCount - func.ArgsCount;
				}
				else
				{
					this.current = 0;
					VarCount = -1;
				}
			}

			public int VarCount      { get; private set; }
			public object Target     { get; private set; }
			public MethodInfo Method { get; private set; }
			public LocalBuilder Local { get; set; }

			public int Current { get { return this.current; } }

			public bool NextIsLastFixed()
			{
				return (++this.current == 0);
			}
		}

		#endregion
		#region IExpressionOutput

		public new void PutFunction(FunctionItem func, int argzCount)
		{
			int i = this.calls.Count;
			while(this.calls[--i] != null)
			{
				Debug.Assert(i >= 0);
			}

			this.calls[i] = new FuncCall(func, argzCount);
			this.useOwner |= (func.Target != null);

			this.code.Add(Code.Function);
			// NOTE: do not call base impl!
		}

		public new void PutBeginCall()
		{
			this.calls.Add(null);
			base.PutBeginCall();
		}

		#endregion
		#region ILCalcClosure

		protected ILCalcClosure GetClosure()
		{
			return new ILCalcClosure(this.closure.ToArray());
		}

		// TODO: try to make it struct?
		internal sealed class ILCalcClosure
		{
			private object[] closure;

			public ILCalcClosure(object[] closure)
			{
				Debug.Assert(closure != null);
				this.closure = closure;
			}
		}

		#endregion
		#region Static Data

		// Owner types =================================

		protected static readonly Type OwnerType = typeof(ILCalcClosure);

		private static readonly FieldInfo ClosureField =
			OwnerType.GetField("closure", BindingFlags.NonPublic | BindingFlags.Instance);

		// OpCodes =====================================

		// load-save values:
		protected static readonly OpCode OpLoadElem = OpCodes.Ldelem_R8;
		protected static readonly OpCode OpSaveElem = OpCodes.Stelem_R8;

		// operations:
		private static readonly OpCode[] OpOperators =
			{
				OpCodes.Sub, OpCodes.Add,
				OpCodes.Mul, OpCodes.Div,
				OpCodes.Rem, OpCodes.Nop,
				OpCodes.Neg
			};

		private static readonly MethodInfo PowMethod =
			typeof(Math).GetMethod("Pow");

		// arguments:
		private static readonly OpCode[] OpArgsLoad =
			{
				OpCodes.Ldarg_0,
				OpCodes.Ldarg_1,
				OpCodes.Ldarg_2,
				OpCodes.Ldarg_3
			};

		// int32 constants:
		private static readonly OpCode[] OpLoadConst =
			{
				OpCodes.Ldc_I4_M1,
				OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1,
				OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3,
				OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5,
				OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7,
				OpCodes.Ldc_I4_8
			};

		#endregion
	}
}