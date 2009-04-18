//#define VISUALIZE
using System;
using System.Reflection.Emit;

namespace ILCalc
	{
	sealed class EvaluatorCompiler : CompilerBase, IExpressionOutput
		{
		#region Fields

		private readonly bool paramsArgs;
		private readonly int argsCount;
		
		#endregion
		#region Methods

		public EvaluatorCompiler( int argCount, bool check )
			: base(	valueType, ArgTypes(argCount), check )
			{
			argsCount = argCount;
			paramsArgs = argCount > 2;
			}

		public Evaluator CreateEvaluator( string expr )
			{
			body.Emit(OpCodes.Ret);

			#if VISUALIZE
			DynamicMethodVisualizer.Visualizer.Show(_eval);
			#endif

			Delegate method = dynMethod.CreateDelegate(EvalTypes(argsCount));
			return new Evaluator( expr, method, argsCount );
			}

		#endregion
		#region IExpressionOutput

		public void PutArgument( int id )
			{
			if(paramsArgs) 
				{
				body.Emit(OpCodes.Ldarg_0);
				body_EmitLoadI4(id);
				body.Emit(opLoadElem);
				}
			else body.Emit(opArgsLoad[id]);
			}

		#endregion
		#region Static Data

		// Helpers ================================================

		private static Type[] ArgTypes( int count )
			{
			if( count > 3 ) count = 3;
			return argsTypes[count];
			}

		private static Type EvalTypes( int count )
			{
			if( count > 3 ) count = 3;
			return evalTypes[count];
			}

		// Types ==================================================

		private static readonly Type[][] argsTypes =
			{
				null,
				new[] { valueType },
				new[] { valueType, valueType },
				new[] { arrayType }
			};

		private static readonly Type[] evalTypes = 
			{
				typeof( EvalFunc0 ),
				typeof( EvalFunc1 ),
				typeof( EvalFunc2 ),
				typeof( EvalFuncN )
			};

		// OpCodes ================================================

		private static readonly OpCode[] opArgsLoad =
			{
				OpCodes.Ldarg_0,
				OpCodes.Ldarg_1,
				OpCodes.Ldarg_2
			};

		#endregion
		}
	}
