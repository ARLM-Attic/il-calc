using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ILCalc
{
	using Allocator = Tabulator.Allocator;

	internal sealed class TabulatorCompiler : CompilerBase, IExpressionOutput
	{
		#region Fields

		private readonly List<LocalBuilder> argsLocals;
		private readonly Stack<LocalBuilder> stepLocals;
		private readonly Stack<LocalBuilder> locals;
		private readonly Stack<Label> labels;

		#endregion
		#region Methods

		public TabulatorCompiler(int argsCount, bool check)
			: base(argsCount, ReturnType(argsCount), ArgsType(argsCount), check)
		{
			Debug.Assert(argsCount > 0);

			this.labels = new Stack<Label>(argsCount * 2);
			this.argsLocals = new List<LocalBuilder>(argsCount);

			if (argsCount > 2)
			{
				this.stepLocals = new Stack<LocalBuilder>(argsCount);
				this.locals = new Stack<LocalBuilder>(argsCount * 2);
				this.BeginMulti();
			}
			else
			{
				this.locals = new Stack<LocalBuilder>(argsCount == 1 ? 1 : 3);
				this.BeginSimple();
			}
		}

		public Tabulator CreateTabulator(string expr)
		{
			il.Emit(OpSaveElem);

			if (this.argsCount > 2)
			{
				this.EndMulti();
			}
			else
			{
				this.EndSimple();
			}

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ret);

			// DynamicMethodVisualizer.Visualizer.Show(dynMethod);

			Delegate method = this.dynMethod.CreateDelegate(this.DelegateType);
			if( argsCount > 2 )
			{
				Allocator alloc = AllocCompiler.Resolve(argsCount);
				return new Tabulator(expr, method, argsCount, alloc);
			}
			
			return new Tabulator(expr, method, this.argsCount);
		}

		#endregion
		#region Emitters

		private void BeginSimple()
		{
			LocalBuilder indexLocal = il.DeclareLocal(IndexType);
			LocalBuilder varLocal = il.DeclareLocal(TypeHelper.ValueType);

			// double x = beginx;
			if (this.argsCount == 1)
			{
				this.il.Emit(OpCodes.Ldarg_2);
			}
			else
			{
				this.il.Emit(OpCodes.Ldarg_S, (byte) 4);
			}

			this.il.Emit(OpCodes.Stloc, varLocal);

			// int i = 0;
			this.il.Emit(OpCodes.Ldc_I4_0);
			this.il.Emit(OpCodes.Stloc, indexLocal);

			this.EmitLoopBegin();

			this.argsLocals.Add(varLocal);
			this.locals.Push(indexLocal);

			if (this.argsCount == 2)
			{
				LocalBuilder index2Local = this.il.DeclareLocal(IndexType);
				LocalBuilder arrayLocal = this.il.DeclareLocal(TypeHelper.ArrayType);
				LocalBuilder var2Local = this.il.DeclareLocal(TypeHelper.ValueType);

				// double b = a[i];
				this.il.Emit(OpCodes.Ldarg_0);
				this.il.Emit(OpCodes.Ldloc, indexLocal);
				this.il.Emit(OpCodes.Ldelem_Ref);
				this.il.Emit(OpCodes.Stloc, arrayLocal);

				// double y = begin2;
				this.il.Emit(OpCodes.Ldarg_3);
				this.il.Emit(OpCodes.Stloc, var2Local);

				// int j = 0;
				this.il.Emit(OpCodes.Ldc_I4_0);
				this.il.Emit(OpCodes.Stloc, index2Local);

				this.EmitLoopBegin();
				this.argsLocals.Add(var2Local);
				this.locals.Push(index2Local);
				this.locals.Push(arrayLocal);

				// b[i] = 
				this.il.Emit(OpCodes.Ldloc, arrayLocal);
				this.il.Emit(OpCodes.Ldloc, index2Local);
			}
			else
			{
				// a[i] = 
				this.il.Emit(OpCodes.Ldarg_0);
				this.il.Emit(OpCodes.Ldloc, indexLocal);
			}
		}

		private void BeginMulti()
		{
			for (int i = 0; i < this.argsCount; i++)
			{
				LocalBuilder stepLocal = il.DeclareLocal(TypeHelper.ValueType);

				this.il.Emit(OpCodes.Ldarg_1);
				EmitLoadI4(this.il, i);
				this.il.Emit(OpLoadElem);
				this.il.Emit(OpCodes.Stloc, stepLocal);

				this.stepLocals.Push(stepLocal);
			}

			LocalBuilder lastIndex = null;
			LocalBuilder lastArray = null;

			for (int i = 0, t = this.argsCount; i < this.argsCount; i++, t--)
			{
				Type arrayType = TypeHelper.GetArrayType(t);

				LocalBuilder arrayLocal = this.il.DeclareLocal(arrayType);
				LocalBuilder indexLocal = this.il.DeclareLocal(IndexType);
				LocalBuilder varLocal = this.il.DeclareLocal(TypeHelper.ValueType);

				if (i == 0)
				{
					// a = (double[][][]) ar;
					this.il.Emit(OpCodes.Ldarg_0);
					this.il.Emit(OpCodes.Castclass, arrayType);
					this.il.Emit(OpCodes.Stloc, arrayLocal);
				}
				else
				{
					Debug.Assert(lastArray != null);
					Debug.Assert(lastIndex != null);

					// double[] b = a[i];
					this.il.Emit(OpCodes.Ldloc, lastArray);
					this.il.Emit(OpCodes.Ldloc, lastIndex);
					this.il.Emit(OpCodes.Ldelem_Ref);
					this.il.Emit(OpCodes.Stloc, arrayLocal);
				}

				// double x = begins[0];
				this.il.Emit(OpCodes.Ldarg_1);
				EmitLoadI4(this.il, i + this.argsCount);
				this.il.Emit(OpLoadElem);
				this.il.Emit(OpCodes.Stloc, varLocal);

				// i++;
				this.il.Emit(OpCodes.Ldc_I4_0);
				this.il.Emit(OpCodes.Stloc, indexLocal);

				this.EmitLoopBegin();
				this.argsLocals.Add(varLocal);
				this.locals.Push(arrayLocal);
				this.locals.Push(indexLocal);

				lastArray = arrayLocal;
				lastIndex = indexLocal;
			}

			Debug.Assert(lastIndex != null);
			Debug.Assert(lastArray != null);

			// c[z] = 
			this.il.Emit(OpCodes.Ldloc, lastArray);
			this.il.Emit(OpCodes.Ldloc, lastIndex);
		}

		private void EndSimple()
		{
			if (this.argsCount == 2)
			{
				LocalBuilder arrayLocal = this.locals.Pop();
				LocalBuilder index2Local = this.locals.Pop();
				LocalBuilder var2Local = this.argsLocals[1];

				// x += step;
				this.il.Emit(OpCodes.Ldloc, var2Local);
				this.il.Emit(OpCodes.Ldarg_2);
				this.il.Emit(OpCodes.Add);
				this.il.Emit(OpCodes.Stloc, var2Local);

				this.EmitLoopEnd(index2Local, arrayLocal);
			}

			LocalBuilder indexLocal = this.locals.Pop();
			LocalBuilder varLocal = this.argsLocals[0];
			
			this.il.Emit(OpCodes.Ldloc, varLocal);
			this.il.Emit(OpCodes.Ldarg_1);
			this.il.Emit(OpCodes.Add);
			this.il.Emit(OpCodes.Stloc, varLocal);

			this.EmitLoopEnd(indexLocal, null);
		}

		private void EndMulti()
		{
			for( int i = 0, j = this.argsCount - 1; i < this.argsCount; i++, j-- )
			{
				LocalBuilder indexLocal = locals.Pop();
				LocalBuilder arrayLocal = locals.Pop();
				LocalBuilder varLocal = argsLocals[j];

				// x += xstep;
				il.Emit(OpCodes.Ldloc, varLocal);
				il.Emit(OpCodes.Ldloc, stepLocals.Pop());
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, varLocal);

				this.EmitLoopEnd(indexLocal, arrayLocal);
			}
		}

		private void EmitLoopBegin()
		{
			Label lbCond  = this.il.DefineLabel();
			Label lbBegin = this.il.DefineLabel();

			this.il.Emit(OpCodes.Br, lbCond);
			this.il.MarkLabel(lbBegin);

			this.labels.Push(lbBegin);
			this.labels.Push(lbCond);
		}

		private void EmitLoopEnd(LocalBuilder index, LocalBuilder array)
		{
			Debug.Assert(index != null);

			// i++;
			this.il.Emit(OpCodes.Ldloc, index);
			this.il.Emit(OpCodes.Ldc_I4_1);
			this.il.Emit(OpCodes.Add);
			this.il.Emit(OpCodes.Stloc, index);

			Label lbCond  = this.labels.Pop();
			Label lbBegin = this.labels.Pop();

			// while(i < a.Length)
			this.il.MarkLabel(lbCond);
			this.il.Emit(OpCodes.Ldloc, index);

			if( array == null )
			{
				this.il.Emit(OpCodes.Ldarg_0);
			}
			else
			{
				this.il.Emit(OpCodes.Ldloc, array);
			}

			this.il.Emit(OpCodes.Ldlen);
			this.il.Emit(OpCodes.Conv_I4);
			this.il.Emit(OpCodes.Blt, lbBegin);
		}

		#endregion
		#region IExpressionOutput

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);
			Debug.Assert(id < argsLocals.Count);

			this.il.Emit(OpCodes.Ldloc, this.argsLocals[id]);
		}

		#endregion
		#region Helpers

		private static Type[] ArgsType(int count)
		{
			Debug.Assert(count > 0);

			return ArgsTypes[count <= 2 ? count - 1 : 2];
		}

		private static Type ReturnType(int count)
		{
			Debug.Assert(count > 0);

			return ArgsTypes[count <= 2 ? count - 1 : 2][0];
		}

		private Type DelegateType
		{
			get
			{
				return DelegateTypes[this.argsCount <= 2 ? this.argsCount - 1 : 2];
			}
		}

		#endregion
		#region Static Data

		// Types ================================================================
		private static readonly Type IndexType = typeof(Int32);
		private static readonly Type SystemArrayType = typeof(Array);
		private static readonly Type Array2DType = typeof(Double[][]);

		private static readonly Type[][] ArgsTypes = new[]
		{
			new[] { TypeHelper.ArrayType, TypeHelper.ValueType, TypeHelper.ValueType },
			new[] { Array2DType, TypeHelper.ValueType, TypeHelper.ValueType, TypeHelper.ValueType, TypeHelper.ValueType },
			new[] { SystemArrayType, TypeHelper.ArrayType }
		};

		private static readonly Type[] DelegateTypes = new[]
		{
			typeof(Tabulator.TabFunc1),
			typeof(Tabulator.TabFunc2),
			typeof(Tabulator.TabFuncN)
		};

		#endregion
		#region AllocCompiler

		internal static class AllocCompiler
		{
			#region Fields

			private static readonly Dictionary<int, Allocator> Cache
				= new Dictionary<int, Allocator>();

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
				var alloc = new DynamicMethod("Alloc", SystemArrayType, AllocArgs, AllocType);
				var il = alloc.GetILGenerator();

				var locals = new Stack<LocalBuilder>();
				var labels = new Stack<Label>();

				for (int i = rank - 1, j = 0; i > 0; i--, j++)
				{
					var arrayLoc = il.DeclareLocal(TypeHelper.GetArrayType(i + 1));
					var indexLoc = il.DeclareLocal(IndexType);

					// a = new double[count[i]][]..;
					il.Emit(OpCodes.Ldarg_0);
					EmitLoadI4(il, j);
					il.Emit(OpCodes.Ldelem_I4);
					il.Emit(OpCodes.Newarr, TypeHelper.GetArrayType(i));
					il.Emit(OpCodes.Stloc, arrayLoc);

					// int i = 0;
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Stloc, indexLoc);

					var lbCond  = il.DefineLabel();
					var lbBegin = il.DefineLabel();

					il.Emit(OpCodes.Br, lbCond);
					il.MarkLabel(lbBegin);

					locals.Push(indexLoc);
					locals.Push(arrayLoc);
					labels.Push(lbBegin);
					labels.Push(lbCond);
				}

				LocalBuilder lastLocal = null;

				for (int i = 0; i < rank - 1; i++)
				{
					LocalBuilder arrayLoc = locals.Pop();
					LocalBuilder indexLoc = locals.Pop();

					if (i == 0)
					{
						// arr[i] =
						il.Emit(OpCodes.Ldloc, arrayLoc);
						il.Emit(OpCodes.Ldloc, indexLoc);

						// = new double[counts[<rank-1>]]
						il.Emit(OpCodes.Ldarg_0);
						EmitLoadI4(il, rank - 1);
						il.Emit(OpCodes.Ldelem_I4);
						il.Emit(OpCodes.Newarr, TypeHelper.ValueType);
						il.Emit(OpCodes.Stelem_Ref);
					}
					else
					{
						Debug.Assert(lastLocal != null);

						// b[j] = a;
						il.Emit(OpCodes.Ldloc, arrayLoc);
						il.Emit(OpCodes.Ldloc, indexLoc);
						il.Emit(OpCodes.Ldloc, lastLocal);
						il.Emit(OpCodes.Stelem_Ref);
					}

					// i++
					il.Emit(OpCodes.Ldloc, indexLoc);
					il.Emit(OpCodes.Ldc_I4_1);
					il.Emit(OpCodes.Add);
					il.Emit(OpCodes.Stloc, indexLoc);

					// while(i < a.Lengh)
					il.MarkLabel(labels.Pop());
					il.Emit(OpCodes.Ldloc, indexLoc);
					il.Emit(OpCodes.Ldloc, arrayLoc);
					il.Emit(OpCodes.Ldlen);
					il.Emit(OpCodes.Conv_I4);
					il.Emit(OpCodes.Blt, labels.Pop());

					lastLocal = arrayLoc;
				}

				Debug.Assert(lastLocal != null);

				il.Emit(OpCodes.Ldloc, lastLocal);
				il.Emit(OpCodes.Ret);

				return (Allocator) alloc.CreateDelegate(AllocType);
			}

			#endregion
		}

		#endregion
	}
}