using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ILCalc
{
	internal sealed class EvaluatorCompiler : CompilerBase, IExpressionOutput
	{
		#region Fields

		private readonly bool paramsArgs;
		
		#endregion
		#region Methods

		public EvaluatorCompiler(int argsCount, bool check)
			: base(argsCount, TypeHelper.ValueType, ArgsType(argsCount), check)
			{
			Debug.Assert(argsCount >= 0);

			this.paramsArgs = argsCount > 2;
			}

		public Evaluator CreateEvaluator(string expression)
		{
			this.il.Emit(OpCodes.Ret);

			// DynamicMethodVisualizer.Visualizer.Show(_eval);

			Delegate method = this.dynMethod.CreateDelegate(this.DelegateType);
			return new Evaluator(expression, method, this.argsCount);
		}

		#endregion
		#region IExpressionOutput

		public void PutArgument(int id)
		{
			Debug.Assert(id >= 0);

			if (this.paramsArgs)
			{
				this.il.Emit(OpCodes.Ldarg_0);
				EmitLoadI4(this.il, id);
				this.il.Emit(OpLoadElem);
			}
			else
			{
				this.il.Emit(OpArgsLoad[id]);
			}
		}

		#endregion
		#region Helpers

		private static Type[] ArgsType(int count)
		{
			Debug.Assert(count >= 0);

			return ArgsTypes[count <= 3 ? count : 3];
		}

		private Type DelegateType
		{
			get
			{
				return DelegateTypes[this.argsCount <= 3 ? this.argsCount : 3];
			}
		}

		#endregion
		#region Static Data

		// Types ==================================================
		private static readonly Type[][] ArgsTypes =
			{
				null,
				new[] { TypeHelper.ValueType },
				new[] { TypeHelper.ValueType, TypeHelper.ValueType },
				new[] { TypeHelper.ArrayType }
			};

		private static readonly Type[] DelegateTypes = 
			{
				typeof(EvalFunc0),
				typeof(EvalFunc1),
				typeof(EvalFunc2),
				typeof(EvalFuncN)
			};

		// OpCodes ================================================
		private static readonly OpCode[] OpArgsLoad =
			{
				OpCodes.Ldarg_0,
				OpCodes.Ldarg_1,
				OpCodes.Ldarg_2
			};

		#endregion
	}
}
