using System.Reflection.Emit;

namespace ILCalc.Custom
{
  interface ICompiler<T>
  {
    void LoadConst(ILGenerator il, T value);
    void Operation(ILGenerator il, int op);
    void CheckedOp(ILGenerator il, int op);
    void LoadElem(ILGenerator il);
    void SaveElem(ILGenerator il);
  }
}
