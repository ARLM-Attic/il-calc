using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ILCalc
{
	internal sealed class EvaluatorCompiler : CompilerBase
	{
		#region Fields

		private readonly int argsCount;

		#endregion
		#region Constructor

		public EvaluatorCompiler(int argsCount, bool checks)
			: base(checks)
		{
			Debug.Assert(argsCount >= 0);

			this.argsCount = argsCount;
		}

		#endregion
		#region Methods

		protected override void EmitLoadArg(ILGenerator il, int index)
		{
			if (this.argsCount > 2)
			{
				il_EmitLoadArg(il, 0);
				il_EmitLoadI4(il, index);
				il.Emit(OpLoadElem);
			}
			else
			{
				il_EmitLoadArg(il, index);
			}
		}

		public Evaluator CreateEvaluator(string expression)
		{
			int count = this.argsCount;
			if (count > 3) count = 3;

			Type[] argsTypes = !OwnerUsed ?
				ArgsTypes1[count] :
				ArgsTypes2[count] ;

			var method = new DynamicMethod(
				"evaluator", TypeHelper.ValueType, argsTypes, OwnerType, true);

			// ======================================================

			ILGenerator il = method.GetILGenerator();

			CodeGen(il);
			il.Emit(OpCodes.Ret);

			// ======================================================

			Type delType = DelegateTypes[count];

			Delegate delg = OwnerUsed ?
				method.CreateDelegate(delType, GetClosure()) :
				method.CreateDelegate(delType);

			//DynamicMethodVisualizer.Visualizer.Show(method);

			return new Evaluator(expression, delg, this.argsCount);
		}

		#endregion
		#region Static Data

		// Types ==================================================
		private static readonly Type[][] ArgsTypes1 =
			{
				null,
				new[] { TypeHelper.ValueType },
				new[] { TypeHelper.ValueType, TypeHelper.ValueType },
				new[] { TypeHelper.ArrayType }
			};

		private static readonly Type[][] ArgsTypes2 =
			{
				new[] { OwnerType },
				new[] { OwnerType, TypeHelper.ValueType },
				new[] { OwnerType, TypeHelper.ValueType, TypeHelper.ValueType },
				new[] { OwnerType, TypeHelper.ArrayType }
			};

		private static readonly Type[] DelegateTypes = 
			{
				typeof(EvalFunc0),
				typeof(EvalFunc1),
				typeof(EvalFunc2),
				typeof(EvalFuncN)
			};

		#endregion
	}
}