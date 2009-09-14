using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ILCalc
{
  sealed class EvaluatorCompiler<T> : CompilerBase<T>
  {
    #region Fields

    readonly int argsCount;

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
        il_EmitLoadElem(il);
      }
      else
      {
        il_EmitLoadArg(il, index);
      }
    }

    public Delegate CreateDelegate()
    {
      int args = this.argsCount;
      if (args > 3) args = 3;

      Type[] argsTypes = ArgsTypes[args];
      object owner = OwnerFixup(ref argsTypes);

      var method = new DynamicMethod(
        "evaluator", TypeHelper<T>.ValueType,
        argsTypes, OwnerType, true);

      // ======================================================

      ILGenerator il = method.GetILGenerator();
      CodeGen(il);
      il.Emit(OpCodes.Ret);

      // DynamicMethodVisualizer.Visualizer.Show(method);
      // ======================================================

      return GetDelegate(method, DelegateTypes[args], owner);
    }

    #endregion
    #region Static Data

    // Types ==================================================
    static readonly Type[][] ArgsTypes =
      {
        Type.EmptyTypes,
        new[] { TypeHelper<T>.ValueType },
        new[] { TypeHelper<T>.ValueType,
                TypeHelper<T>.ValueType },
        new[] { TypeHelper<T>.ArrayType }
      };

    static readonly Type[] DelegateTypes =
      {
        typeof(EvalFunc0<T>),
        typeof(EvalFunc1<T>),
        typeof(EvalFunc2<T>),
        typeof(EvalFuncN<T>)
      };

    #endregion
  }
}