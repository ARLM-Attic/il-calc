//#define VISUALIZE
using System;
using System.Reflection.Emit;

namespace ILCalc
	{
	sealed class EvaluatorCompiler : CompilerBase, IExpressionOutput
		{
		#region Fields

		private readonly bool _paramsArgs;
		private readonly int _argCount;
		
		#endregion
		#region Members

		public EvaluatorCompiler( int argCount, bool check )
			: base(	_valType, ArgTypes(argCount), check )
			{
			_argCount = argCount;
			_paramsArgs = argCount > 2;
			}

		public Evaluator CreateEvaluator( string expr )
			{
			_body.Emit(OpCodes.Ret);

			#if VISUALIZE
			DynamicMethodVisualizer.Visualizer.Show(_eval);
			#endif

			Delegate method = _eval.CreateDelegate(EvalTypes(_argCount));
			return new Evaluator( expr, method, _argCount );
			}

		#endregion
		#region IExpressionOutput

		public void PutArgument( int id )
			{
			if(_paramsArgs) 
				{
				_body.Emit(OpCodes.Ldarg_0);
				_body_EmitLoadI4(id);
				_body.Emit(_loadElem);
				}
			else _body.Emit(_argsLoad[id]);
			}

		#endregion
		#region Static Data

		// Helpers ================================================

		private static Type[] ArgTypes( int count )
			{
			if( count > 3 ) count = 3;
			return _argsTypes[count];
			}

		private static Type EvalTypes( int count )
			{
			if( count > 3 ) count = 3;
			return _evalTypes[count];
			}

		// Types ==================================================

		private static readonly Type[][] _argsTypes =
			{
				null,
				new[] { _valType },
				new[] { _valType, _valType },
				new[] { _arrType }
			};

		private static readonly Type[] _evalTypes = 
			{
				typeof( EvalFunc0 ),
				typeof( EvalFunc1 ),
				typeof( EvalFunc2 ),
				typeof( EvalFuncN )
			};

		// OpCodes ================================================

		private static readonly OpCode[] _argsLoad =
			{
				OpCodes.Ldarg_0,
				OpCodes.Ldarg_1,
				OpCodes.Ldarg_2
			};

		#endregion
		}
	}
