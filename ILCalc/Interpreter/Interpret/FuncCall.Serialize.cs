using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ILCalc
{
  sealed partial class FuncCall<T> : ISerializable
  {
    static readonly Type
      FunctionType = typeof(FunctionItem<T>);

    FuncCall(SerializationInfo info, StreamingContext context)
    {
      int fixCount = info.GetInt32("fix");
      int varCount = info.GetInt32("var");

      this.fixArgs = new object[fixCount];

      if (varCount >= 0)
      {
        this.varArgs = new T[varCount];
        this.fixArgs[--fixCount] = this.varArgs;
      }

      this.func = (FunctionItem<T>)
        info.GetValue("func", FunctionType);

      this.argsCount = fixCount + varCount;
      this.lastIndex = fixCount - 1;
      this.syncRoot = new object();
    }

    [SecurityPermission(SecurityAction.LinkDemand,
      Flags = SecurityPermissionFlag.SerializationFormatter)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("fix", fixArgs.Length);
      info.AddValue("var", varArgs == null ? -1 : varArgs.Length);
      info.AddValue("func", func, FunctionType);
    }
  }
}