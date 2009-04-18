//#define VISUALIZE
using System;
using System.Reflection.Emit;

namespace ILCalc
	{
	sealed class TabulatorCompiler : CompilerBase, IExpressionOutput
		{
		#region Fields

		private readonly bool hasOneArg;

		private readonly LocalBuilder indexLocal;
		private readonly LocalBuilder indexLocal2;
		private readonly LocalBuilder resultLocal;
		private readonly LocalBuilder beginLocal;
		private readonly LocalBuilder arrayLocal;
		
		private readonly Label condLabel,  beginLabel;
		private readonly Label condLabel2, beginLabel2;

		#endregion
		#region Methods

		public TabulatorCompiler( bool oneArg, bool check )
			: base(	RetTypes(oneArg),
					ArgTypes(oneArg), check )
			{
			hasOneArg = oneArg;

			condLabel  = body.DefineLabel( );
			beginLabel = body.DefineLabel( );

			indexLocal = body.DeclareLocal(indexType);
			
			if(oneArg)
				{
				resultLocal = body.DeclareLocal(arrayType);
				
				// res = new double[count];
				body.Emit(OpCodes.Ldarg_1);
				body.Emit(OpCodes.Newarr, valueType);
				body.Emit(OpCodes.Stloc, resultLocal);

				// int i = 0;
				body.Emit(OpCodes.Ldc_I4_0);
				body.Emit(OpCodes.Stloc, indexLocal);

				// jump to condition
				body.Emit(OpCodes.Br, condLabel);
				
				// res[i] = 
				body.MarkLabel(beginLabel);
				body.Emit(OpCodes.Ldloc, resultLocal);
				body.Emit(OpCodes.Ldloc, indexLocal);
				}
			else
				{
				resultLocal   = body.DeclareLocal(arrArrType);
				indexLocal2  = body.DeclareLocal(indexType);
				beginLocal = body.DeclareLocal(valueType);
				arrayLocal = body.DeclareLocal(arrayType);
				
				condLabel2  = body.DefineLabel();
				beginLabel2 = body.DefineLabel();
				
				// res = new double[count1][];
				body.Emit(OpCodes.Ldarg_1);
				body.Emit(OpCodes.Newarr, arrayType);
				body.Emit(OpCodes.Stloc, resultLocal);
				
				// begin = y
				body.Emit(OpCodes.Ldarg_2);
				body.Emit(OpCodes.Stloc, beginLocal);

				// int i = 0;
				body.Emit(OpCodes.Ldc_I4_0);
				body.Emit(OpCodes.Stloc, indexLocal);

				// jump to condition
				body.Emit(OpCodes.Br, condLabel);

				// arr = new double[count2];
				body.MarkLabel(beginLabel);
				body.Emit(OpCodes.Ldarg_3);
				body.Emit(OpCodes.Newarr, valueType);
				body.Emit(OpCodes.Stloc, arrayLocal);

				// j = 0;
				body.Emit(OpCodes.Ldc_I4_0);
				body.Emit(OpCodes.Stloc, indexLocal2);

				// jump to condition
				body.Emit(OpCodes.Br, condLabel2);

				// arr[j] =
				body.MarkLabel(beginLabel2);
				body.Emit(OpCodes.Ldloc, arrayLocal);
				body.Emit(OpCodes.Ldloc, indexLocal2);
				}
			}

		public Tabulator CreateTabulator( string expr )
			{
			body.Emit(opSaveElem);

			if(!hasOneArg)
				{
				// y += step2;
				body.Emit(OpCodes.Ldarg_2);
				body.Emit(OpCodes.Ldarg_S, (byte)5);
				body.Emit(OpCodes.Add);
				body.Emit(OpCodes.Starg_S, (byte)2);

				// j++;
				body.Emit(OpCodes.Ldloc, indexLocal2);
				body.Emit(OpCodes.Ldc_I4_1);
				body.Emit(OpCodes.Add);
				body.Emit(OpCodes.Stloc, indexLocal2);

				// while(j < count2)
				body.MarkLabel(condLabel2);
				body.Emit(OpCodes.Ldloc, indexLocal2);
				body.Emit(OpCodes.Ldarg_3);
				body.Emit(OpCodes.Blt, beginLabel2);

				// res[i] = arr;
				body.Emit(OpCodes.Ldloc, resultLocal);
				body.Emit(OpCodes.Ldloc, indexLocal);
				body.Emit(OpCodes.Ldloc, arrayLocal);
				body.Emit(OpCodes.Stelem_Ref);

				// y = begin
				body.Emit(OpCodes.Ldloc, beginLocal);
				body.Emit(OpCodes.Starg_S, (byte)2);
				}

			// x += step
			body.Emit(OpCodes.Ldarg_0);
			
			if(hasOneArg)
				 body.Emit(OpCodes.Ldarg_2);
			else body.Emit(OpCodes.Ldarg_S, (byte)4);

			body.Emit(OpCodes.Add);
			body.Emit(OpCodes.Starg_S, (byte)0);
			
			// i++;
			body.Emit(OpCodes.Ldloc, indexLocal);
			body.Emit(OpCodes.Ldc_I4_1);
			body.Emit(OpCodes.Add);
			body.Emit(OpCodes.Stloc, indexLocal);

			// while(i < count)
			body.MarkLabel(condLabel);
			body.Emit(OpCodes.Ldloc, indexLocal);
			body.Emit(OpCodes.Ldarg_1);
			body.Emit(OpCodes.Blt, beginLabel);

			// return res
			body.Emit(OpCodes.Ldloc, resultLocal);
			body.Emit(OpCodes.Ret);

			#if VISUALIZE
			DynamicMethodVisualizer.Visualizer.Show(_eval);
			#endif

			Delegate method = dynMethod.CreateDelegate(hasOneArg? tabType1: tabType2);

			return new Tabulator(expr, method, hasOneArg);
			}

		#endregion
		#region IExpressionOutput

		public void PutArgument( int id )
			{
			if(id == 0)
				 body.Emit(OpCodes.Ldarg_0);
			else body.Emit(OpCodes.Ldarg_2);
			}

		#endregion
		#region Static Data

		// Helpers ================================================

		private static Type RetTypes( bool oneArg )
			{
			return oneArg ? arrayType : arrArrType;
			}

		private static Type[] ArgTypes( bool oneArg )
			{
			return oneArg ? argsTypes1 : argsTypes2;
			}

		// Types ==================================================

		private static readonly Type indexType = typeof( int );

		private static readonly Type arrArrType = typeof( double[][] );

		private static readonly Type tabType1 = typeof( Tabulator.TabFunc1 );
		private static readonly Type tabType2 = typeof( Tabulator.TabFunc2 );
		
		private static readonly Type[] argsTypes1 = new[]
			{
				valueType,
				indexType,
				valueType
			};

		private static readonly Type[] argsTypes2 = new[]
			{
				valueType, indexType,
				valueType, indexType,
				valueType, valueType
			};

		#endregion
		}
	}