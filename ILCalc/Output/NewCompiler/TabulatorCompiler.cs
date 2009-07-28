using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ILCalc
{
	using Allocator = Tabulator.Allocator;

	internal sealed class TabulatorCompiler : CompilerBase
	{
		#region Fields

		private readonly List<LocalBuilder> argsLocals;
		private readonly Stack<LocalBuilder> stepLocals;
		private readonly Stack<LocalBuilder> locals;
		private readonly Stack<Label> labels;
		private readonly int argsCount;

		#endregion
		#region Constructors

		public TabulatorCompiler(int argsCount, bool checks)
			: base(checks)
		{
			Debug.Assert(argsCount > 0);

			this.labels = new Stack<Label>(argsCount * 2);
			this.argsLocals = new List<LocalBuilder>(argsCount);

			if (argsCount > 2)
			{
				this.stepLocals = new Stack<LocalBuilder>(argsCount);
				this.locals = new Stack<LocalBuilder>(argsCount * 2);
				//this.BeginMulti();
			}
			else
			{
				this.locals = new Stack<LocalBuilder>(argsCount == 1 ? 1 : 3);
				//this.BeginSimple();
			}

			this.argsCount = argsCount;
		}

		#endregion
		#region Properties

		private List <LocalBuilder> ArgsLocs { get { return this.argsLocals; } }
		private Stack<LocalBuilder> StepLocs { get { return this.stepLocals; } }
		private Stack<LocalBuilder> Locals   { get { return this.locals; } }
		private Stack<Label>        Labels   { get { return this.labels; } }

		private int ArgsCount { get { return this.argsCount; } }

		#endregion
		#region Methods

		protected override void EmitLoadArg(ILGenerator il, int index)
		{
			Debug.Assert(index >= 0);
			Debug.Assert(index < ArgsLocs.Count);

			il.Emit(OpCodes.Ldloc, ArgsLocs[index]);
		}

		public Tabulator CreateTabulator(string expression)
		{
			int count = ArgsCount - 1;
			if (count > 2) count = 2;

			Type[] argsTypes = ArgsTypes1[count];
			object owner = GetOwnerFull();

			if (OwnerUsed)
			{
				var withOwner = new Type[argsTypes.Length + 1];
				Array.Copy(argsTypes, 0, withOwner, 1, argsTypes.Length);

				withOwner[0] = OwnerType;
				argsTypes = withOwner;
			}

			Type returnType = ArgsTypes1[count][0];

			var method = new DynamicMethod(
				"tabulator", returnType, argsTypes, OwnerType, true);

			// ======================================================

			ILGenerator il = method.GetILGenerator();

			if (count < 2)
			{
				BeginSimple(il);
				CodeGen(il);
				il.Emit(OpSaveElem);
				EndSimple(il);
			}
			else
			{
				BeginMulti(il);
				CodeGen(il);
				il.Emit(OpSaveElem);
				EndMulti(il);
			}

			il_EmitLoadArg(il, 0);
			il.Emit(OpCodes.Ret);

			// ======================================================

			Type delType = DelegateTypes[count];

			Delegate delg = OwnerUsed ?
				method.CreateDelegate(delType, owner) :
				method.CreateDelegate(delType);

			//DynamicMethodVisualizer.Visualizer.Show(method);

			if (count == 2)
			{
				Allocator alloc = AllocCompiler.Resolve(ArgsCount);
				return new Tabulator(
					expression, delg, ArgsCount, alloc);
			}

			return new Tabulator(
				expression, delg, ArgsCount);
		}

		#endregion
		#region Emitters

		private void BeginSimple(ILGenerator il)
		{
			Debug.Assert(il != null);
			Debug.Assert(ArgsCount > 0);
			Debug.Assert(ArgsCount < 3);

			LocalBuilder index = il.DeclareLocal(IndexType);
			LocalBuilder var = il.DeclareLocal(TypeHelper.ValueType);

			// double x = beginx;
			if (ArgsCount == 1)
				il_EmitLoadArg(il, 2);
			else
				il_EmitLoadArg(il, 3);

			il.Emit(OpCodes.Stloc, var);

			// int i = 0;
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc, index);

			EmitLoopBegin(il);

			ArgsLocs.Add(var);
			Locals.Push(index);

			if (ArgsCount == 2)
			{
				LocalBuilder index2 = il.DeclareLocal(IndexType);
				LocalBuilder array = il.DeclareLocal(TypeHelper.ArrayType);
				LocalBuilder var2 = il.DeclareLocal(TypeHelper.ValueType);

				// double b = a[i];
				il_EmitLoadArg(il, 0);
				il.Emit(OpCodes.Ldloc, index);
				il.Emit(OpCodes.Ldelem_Ref);
				il.Emit(OpCodes.Stloc, array);

				// double y = begin2;
				il_EmitLoadArg(il, 4);
				il.Emit(OpCodes.Stloc, var2);

				// int j = 0;
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Stloc, index2);

				EmitLoopBegin(il);

				SaveContext(var2, index2, array);

				// b[i] = 
				il.Emit(OpCodes.Ldloc, array);
				il.Emit(OpCodes.Ldloc, index2);
			}
			else
			{
				// a[i] = 
				il_EmitLoadArg(il, 0);
				il.Emit(OpCodes.Ldloc, index);
			}
		}

		private void BeginMulti(ILGenerator il)
		{
			Debug.Assert(il != null);
			Debug.Assert(ArgsCount > 2);

			for (int i = 0; i < ArgsCount; i++)
			{
				LocalBuilder step = il.DeclareLocal(TypeHelper.ValueType);

				il_EmitLoadArg(il, 1);
				il_EmitLoadI4(il, i);
				il.Emit(OpLoadElem);
				il.Emit(OpCodes.Stloc, step);

				StepLocs.Push(step);
			}

			LocalBuilder lastIndex = null;
			LocalBuilder lastArray = null;

			for (int i = 0, t = ArgsCount; i < ArgsCount; i++, t--)
			{
				Type arrayType = TypeHelper.GetArrayType(t);

				LocalBuilder array = il.DeclareLocal(arrayType);
				LocalBuilder index = il.DeclareLocal(IndexType);
				LocalBuilder var = il.DeclareLocal(TypeHelper.ValueType);

				if (i == 0)
				{
					// a = (double[][][]) ar;
					il_EmitLoadArg(il, 0);
					il.Emit(OpCodes.Castclass, arrayType);
					il.Emit(OpCodes.Stloc, array);
				}
				else
				{
					Debug.Assert(lastArray != null);
					Debug.Assert(lastIndex != null);

					// double[] b = a[i];
					il.Emit(OpCodes.Ldloc, lastArray);
					il.Emit(OpCodes.Ldloc, lastIndex);
					il.Emit(OpCodes.Ldelem_Ref);
					il.Emit(OpCodes.Stloc, array);
				}

				// double x = begins[0];
				il_EmitLoadArg(il, 1);
				il_EmitLoadI4(il, i + ArgsCount);
				il.Emit(OpLoadElem);
				il.Emit(OpCodes.Stloc, var);

				// i++;
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Stloc, index);

				EmitLoopBegin(il);

				SaveContext(var, array, index);

				lastArray = array;
				lastIndex = index;
			}

			Debug.Assert(lastIndex != null);
			Debug.Assert(lastArray != null);

			// c[z] = 
			il.Emit(OpCodes.Ldloc, lastArray);
			il.Emit(OpCodes.Ldloc, lastIndex);
		}

		private void EndSimple(ILGenerator il)
		{
			Debug.Assert(il != null);
			Debug.Assert(ArgsCount > 0);
			Debug.Assert(ArgsCount < 3);

			if (ArgsCount == 2)
			{
				LocalBuilder array  = Locals.Pop();
				LocalBuilder index2 = Locals.Pop();
				LocalBuilder var2 = ArgsLocs[1];

				// x += step;
				il.Emit(OpCodes.Ldloc, var2);
				il_EmitLoadArg(il, 2);
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, var2);

				EmitLoopEnd(il, index2, array);
			}

			LocalBuilder index = Locals.Pop();
			LocalBuilder var = ArgsLocs[0];
			
			il.Emit(OpCodes.Ldloc, var);
			il_EmitLoadArg(il, 1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, var);

			EmitLoopEnd(il, index, null);
		}

		private void EndMulti(ILGenerator il)
		{
			Debug.Assert(il != null);
			Debug.Assert(ArgsCount > 2);

			for( int i = 0, j = ArgsCount - 1; i < ArgsCount; i++, j-- )
			{
				LocalBuilder index = Locals.Pop();
				LocalBuilder array = Locals.Pop();
				LocalBuilder var = ArgsLocs[j];

				// x += xstep;
				il.Emit(OpCodes.Ldloc, var);
				il.Emit(OpCodes.Ldloc, StepLocs.Pop());
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, var);

				EmitLoopEnd(il, index, array);
			}
		}

		private void EmitLoopBegin(ILGenerator il)
		{
			Label condition = il.DefineLabel();
			Label loopBegin = il.DefineLabel();

			il.Emit(OpCodes.Br, condition);
			il.MarkLabel(loopBegin);

			Labels.Push(loopBegin);
			Labels.Push(condition);
		}

		private void EmitLoopEnd(
			ILGenerator il, LocalBuilder index, LocalBuilder array)
		{
			Debug.Assert(index != null);

			// i++;
			il.Emit(OpCodes.Ldloc, index);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, index);

			Label condition = Labels.Pop();
			Label loopBegin = Labels.Pop();

			// while(i < a.Length)
			il.MarkLabel(condition);
			il.Emit(OpCodes.Ldloc, index);

			if (array == null)
				il_EmitLoadArg(il, 0);
			else
				il.Emit(OpCodes.Ldloc, array);

			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Blt, loopBegin);
		}

		private void SaveContext(
			LocalBuilder arg, LocalBuilder loc1, LocalBuilder loc2)
		{
			ArgsLocs.Add(arg);
			Locals.Push(loc1);
			Locals.Push(loc2);
		}

		#endregion
		#region Static Data

		// Types ================================================================
		private static readonly Type IndexType = typeof(Int32);
		private static readonly Type SystemArrayType = typeof(Array);
		private static readonly Type Array2DType = typeof(Double[][]);

		private static readonly Type[][] ArgsTypes1 = new[]
		{
			new[]
			{
				TypeHelper.ArrayType,
				TypeHelper.ValueType,
				TypeHelper.ValueType
			},
			new[]
			{
				Array2DType,
				TypeHelper.ValueType,
				TypeHelper.ValueType,
				TypeHelper.ValueType,
				TypeHelper.ValueType
			},
			new[]
			{
				SystemArrayType,
				TypeHelper.ArrayType
			}
		};

		private static readonly Type[] DelegateTypes = new[]
		{
			typeof(Tabulator.TabFunc1),
			typeof(Tabulator.TabFunc2),
			typeof(Tabulator.TabFuncN)
		};

		#endregion
		#region AllocCompiler

		public static class AllocCompiler
		{
			#region Fields

			private static readonly Dictionary<int, Allocator>
				Cache = new Dictionary<int, Allocator>();

			private static readonly Type AllocType = typeof(Allocator);
			private static readonly Type[] AllocArgs = new[] { typeof(Int32[]) };

			#endregion
			#region Methods

			public static Allocator Resolve(int rank)
			{
				Debug.Assert(rank >= 2);
				
				Allocator alloc;
				if (!Cache.TryGetValue(rank, out alloc))
				{
					lock (((ICollection) Cache).SyncRoot)
					{
						alloc = Compile(rank);
						Cache.Add(rank, alloc);
					}
				}

				return alloc;
			}

			private static Allocator Compile(int rank)
			{
				var method = new DynamicMethod(
					"alloc", SystemArrayType, AllocArgs, AllocType, true);

				ILGenerator il = method.GetILGenerator();

				var locals = new Stack<LocalBuilder>();
				var labels = new Stack<Label>();

				for (int i = rank - 1, j = 0; i > 0; i--, j++)
				{
					LocalBuilder array = il.DeclareLocal(TypeHelper.GetArrayType(i + 1));
					LocalBuilder index = il.DeclareLocal(IndexType);

					// a = new double[count[i]][]..;
					il.Emit(OpCodes.Ldarg_0);
					il_EmitLoadI4(il, j);
					il.Emit(OpCodes.Ldelem_I4);
					il.Emit(OpCodes.Newarr, TypeHelper.GetArrayType(i));
					il.Emit(OpCodes.Stloc, array);

					// int i = 0;
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Stloc, index);

					var lbCond  = il.DefineLabel();
					var lbBegin = il.DefineLabel();

					il.Emit(OpCodes.Br, lbCond);
					il.MarkLabel(lbBegin);

					locals.Push(index);
					locals.Push(array);
					labels.Push(lbBegin);
					labels.Push(lbCond);
				}

				LocalBuilder lastLocal = null;

				for (int i = 0; i < rank - 1; i++)
				{
					LocalBuilder array = locals.Pop();
					LocalBuilder index = locals.Pop();

					if (i == 0)
					{
						// arr[i] =
						il.Emit(OpCodes.Ldloc, array);
						il.Emit(OpCodes.Ldloc, index);

						// = new double[counts[<rank-1>]]
						il.Emit(OpCodes.Ldarg_0);
						il_EmitLoadI4(il, rank - 1);
						il.Emit(OpCodes.Ldelem_I4);
						il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
						il.Emit(OpCodes.Stelem_Ref);
					}
					else
					{
						Debug.Assert(lastLocal != null);

						// b[j] = a;
						il.Emit(OpCodes.Ldloc, array);
						il.Emit(OpCodes.Ldloc, index);
						il.Emit(OpCodes.Ldloc, lastLocal);
						il.Emit(OpCodes.Stelem_Ref);
					}

					// i++
					il.Emit(OpCodes.Ldloc, index);
					il.Emit(OpCodes.Ldc_I4_1);
					il.Emit(OpCodes.Add);
					il.Emit(OpCodes.Stloc, index);

					// while(i < a.Lengh)
					il.MarkLabel(labels.Pop());
					il.Emit(OpCodes.Ldloc, index);
					il.Emit(OpCodes.Ldloc, array);
					il.Emit(OpCodes.Ldlen);
					il.Emit(OpCodes.Conv_I4);
					il.Emit(OpCodes.Blt, labels.Pop());

					lastLocal = array;
				}

				Debug.Assert(lastLocal != null);

				il.Emit(OpCodes.Ldloc, lastLocal);
				il.Emit(OpCodes.Ret);

				return (Allocator)
					method.CreateDelegate(AllocType);
			}

			#endregion
		}

		#endregion
	}
}