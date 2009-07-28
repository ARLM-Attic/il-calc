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

			Type[] argsTypes = ArgsTypes[count];
			object owner = GetOwnerFull();

			if (OwnerUsed)
			{
				var withOwner = new Type[argsTypes.Length + 1];
				Array.Copy(argsTypes, 0, withOwner, 1, argsTypes.Length);

				withOwner[0] = OwnerType;
				argsTypes = withOwner;
			}

			var method = new DynamicMethod(
				"evaluator", TypeHelper.ValueType, argsTypes, OwnerType, true);

			// ======================================================

			ILGenerator il = method.GetILGenerator();

			CodeGen(il);
			il.Emit(OpCodes.Ret);

			// ======================================================

			Type delType = DelegateTypes[count];

			Delegate delg = OwnerUsed ?
				method.CreateDelegate(delType, owner) :
				method.CreateDelegate(delType);

			//DynamicMethodVisualizer.Visualizer.Show(method);

			return new Evaluator(expression, delg, this.argsCount);
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

		#endregion
	}
}