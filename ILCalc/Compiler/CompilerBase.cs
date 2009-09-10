using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using ILCalc.Custom;

namespace ILCalc
{
  // TODO: Reuse BufferWriter if Compiler + Optimizer

  abstract class CompilerBase<T>
    : BufferOutput<T>,
      IExpressionOutput<T>
  {
    #region Fields

    readonly List<FuncCall> calls;
    readonly bool emitChecks;

    int targetsCount;
    Type ownerType;

    protected static readonly
      ICompiler<T> Generic = CompilerSupport.Resolve<T>();

    #endregion
    #region Constructor

    protected CompilerBase(bool checks)
    {
      this.emitChecks = checks;
      this.calls = new List<FuncCall>();
    }

    #endregion
    #region Properties

    protected Type OwnerType
    {
      get { return this.ownerType; }
    }

    #endregion
    #region CodeGen

    protected void CodeGen(ILGenerator il)
    {
      int n = 0, d = 0, c = 0;
      Stack<FuncCall> stack = null;
      FuncCall curCall = null;
      int targetIndex = 0;

      for (int i = 0; ; i++) // TODO: length-based?
      {
        int op = this.code[i];

        if (Code.IsOperator(op))
        {
          if (this.emitChecks)
               Generic.CheckedOp(il, op);
          else Generic.Operation(il, op);
        }
        else if (op == Code.Number) // ================================
        {
          Generic.LoadConst(il, this.numbers[n++]);
        }
        else if (op == Code.Argument) // ==============================
        {
          EmitLoadArg(il, this.data[d++]);
        }
        else if (op == Code.Separator) // =============================
        {
          // separator needed only for params calls
          Debug.Assert(curCall != null);
          if (curCall.VarCount >= 0)
          {
            EmitSeparator(il, curCall);
          }
        }
        else if (op == Code.Function) // ==============================
        {
          EmitFunctionCall(il, curCall);

          // parent call info:
          if (stack == null || stack.Count == 0)
               curCall = null;
          else curCall = stack.Pop();
        }
        else if (op == Code.BeginCall) // =============================
        {
          if (curCall != null)
          {
            // allocate if needed
            if (stack == null) stack = new Stack<FuncCall>();
            stack.Push(curCall);
          }

          curCall = this.calls[c++];

          // need for local to store params array:
          if (curCall.VarCount > 0)
          {
            curCall.Local = il.DeclareLocal(TypeHelper<T>.ArrayType);
          }

          if (curCall.Target != null)
          {
            EmitLoadTarget(il, curCall.Target, targetIndex++);
          }

          if (curCall.Current == 0 &&
              curCall.VarCount > 0)
          {
            EmitParamArr(il, curCall);
          }
        }
        else // =======================================================
        {
          break;
        }
      }
    }

    #endregion
    #region Emitters

    protected abstract void EmitLoadArg(ILGenerator il, int index);

    void EmitLoadTarget(ILGenerator il, object target, int index)
    {
      Debug.Assert(il != null);
      Debug.Assert(target != null);
      Debug.Assert(index < this.targetsCount);

      // loads target
      il.Emit(OpCodes.Ldarg_0);

      if (this.targetsCount == 1) {}
      else if (this.targetsCount <= 3)
      {
        Debug.Assert(this.ownerType != null);

        FieldInfo field = OwnerType.GetField(
          "obj" + index, InstanceNonPublic);

        Debug.Assert(field != null);

        // 2 hours of debugging to find the difference :(
        if (field.FieldType.IsValueType)
             il.Emit(OpCodes.Ldflda, field);
        else il.Emit(OpCodes.Ldfld,  field);
      }
      else
      {
        il.Emit(OpCodes.Ldfld, CompilerSupport.OwnerArrayField);
        il_EmitLoadI4(il, index);
        il.Emit(OpCodes.Ldelem_Ref);

        Type targetType = target.GetType();

        if (targetType.IsValueType)
             il.Emit(OpCodes.Unbox_Any, targetType);
        else il.Emit(OpCodes.Castclass, targetType);
      }
    }

    static void EmitSeparator(ILGenerator il, FuncCall call)
    {
      Debug.Assert(il != null);
      Debug.Assert(call != null);

      if (call.NextIsLastFixed())
      {
        EmitParamArr(il, call);
      }
      else if (call.Current > 0)
      {
        il_EmitSaveElem(il);
        il.Emit(OpCodes.Ldloc, call.Local);
        il_EmitLoadI4(il, call.Current);
        il_EmitLoadAdress(il);
      }
    }

    static void EmitFunctionCall(ILGenerator il, FuncCall call)
    {
      Debug.Assert(il != null);

      if (call.VarCount >= 0)
      {
        Debug.Assert(call != null);

        if (call.VarCount > 0)
        {
          il_EmitSaveElem(il);
          il.Emit(OpCodes.Ldloc, call.Local);
        }
        else
        {
          il_EmitLoadI4(il, 0);
          il.Emit(OpCodes.Newarr, TypeHelper<T>.ValueType);
        }
      }

      if (call.Target == null)
           il.Emit(OpCodes.Call, call.Method);
      else il.Emit(OpCodes.Callvirt, call.Method);
    }

    static void EmitParamArr(ILGenerator il, FuncCall call)
    {
      il_EmitLoadI4(il, call.VarCount);
      il.Emit(OpCodes.Newarr, TypeHelper<T>.ValueType);
      il.Emit(OpCodes.Stloc, call.Local);
      il.Emit(OpCodes.Ldloc, call.Local);
      il_EmitLoadI4(il, 0);
      il_EmitLoadAdress(il);
    }

    // ReSharper disable InconsistentNaming

    protected static void il_EmitLoadI4(ILGenerator il, int value)
    {
      if (value < sbyte.MinValue ||
          value > sbyte.MaxValue)
      {
        il.Emit(OpCodes.Ldc_I4, value);
      }
      else if (value < -1 || value > 8)
      {
        il.Emit(OpCodes.Ldc_I4_S, (byte) value);
      }
      else
      {
        il.Emit(OpLoadConst[value+1]);
      }
    }

    protected void il_EmitLoadArg(ILGenerator il, int index)
    {
      Debug.Assert(index >= 0);

      if (this.targetsCount > 0) index++;

      if (index <= 3)
           il.Emit(OpArgsLoad[index]);
      else il.Emit(OpCodes.Ldarg_S, (byte) index);
    }

    protected static void il_EmitLoadElem(ILGenerator il)
    {
      Type type = TypeHelper<T>.ValueType;

      if (type.IsPrimitive)
      {
        Generic.LoadElem(il);
      }
      else if (type.IsValueType)
      {
        il.Emit(OpCodes.Ldelema, type);
        il.Emit(OpCodes.Ldobj, type);
      }
      else
      {
        il.Emit(OpCodes.Ldelem_Ref);
      }
    }

    protected static void il_EmitLoadAdress(ILGenerator il)
    {
      Type type = TypeHelper<T>.ValueType;

      if (!type.IsPrimitive && type.IsValueType)
      {
        il.Emit(OpCodes.Ldelema, type);
      }
    }

    protected static void il_EmitSaveElem(ILGenerator il)
    {
      Type type = TypeHelper<T>.ValueType;

      if (type.IsPrimitive)
      {
        Generic.SaveElem(il);
      }
      else if(type.IsValueType)
      {
        il.Emit(OpCodes.Stobj,
          TypeHelper<T>.ValueType);
      }
    }

    // ReSharper restore InconsistentNaming

    #endregion
    #region FuncCall

    sealed class FuncCall
    {
      #region Fields

      private int current;

      public int VarCount { get; private set; }
      public object Target { get; private set; }
      public MethodInfo Method { get; private set; }
      public LocalBuilder Local { get; set; }

      public int Current { get { return this.current; } }

      #endregion
      #region Constructor

      public FuncCall(FunctionItem<T> func, int argsCount)
      {
        Target = func.Target;
        Method = func.Method;

        if (func.HasParamArray)
        {
          this.current = -func.ArgsCount;
          VarCount = argsCount - func.ArgsCount;
        }
        else
        {
          this.current = 0;
          VarCount = -1;
        }
      }

      #endregion
      #region Fields

      public bool NextIsLastFixed()
      {
        return (++this.current == 0);
      }

      #endregion
    }

    #endregion
    #region IExpressionOutput

    public new void PutCall(FunctionItem<T> func, int argzCount)
    {
      int i = this.calls.Count;
      while (this.calls[--i] != null)
      {
        Debug.Assert(i >= 0);
      }

      this.calls[i] = new FuncCall(func, argzCount);
      if (func.Target != null) this.targetsCount++;

      this.code.Add(Code.Function);
      // NOTE: do not call base impl!
    }

    public new void PutBeginCall()
    {
      this.calls.Add(null);
      base.PutBeginCall();
    }

    #endregion
    #region Helpers

    object GetOwner()
    {
      Debug.Assert(this.targetsCount != 0);

      var cls = new object[this.targetsCount];
      int i = 0;

      foreach (FuncCall call in this.calls)
      {
        if (call.Target != null)
          cls[i++] = call.Target;
      }

      switch (cls.Length)
      {
        case 1:
          this.ownerType = cls[0].GetType();
          return cls[0];

        case 2:
          this.ownerType = CompilerSupport.Owner2Type
            .MakeGenericType(
              cls[0].GetType(),
              cls[1].GetType());

         return Activator.CreateInstance(this.ownerType, cls);

        case 3:
          this.ownerType = CompilerSupport.Owner3Type
            .MakeGenericType(
              cls[0].GetType(),
              cls[1].GetType(),
              cls[2].GetType());

          return Activator.CreateInstance(this.ownerType, cls);

        default:
          this.ownerType = CompilerSupport.OwnerNType;

          return new CompilerSupport.Closure(cls);
      }
    }

    protected object OwnerFixup(ref Type[] argsTypes)
    {
      if (this.targetsCount > 0)
      {
        object owner = GetOwner();

        var ownerArgs = new Type[argsTypes.Length + 1];
        Array.Copy(argsTypes, 0, ownerArgs, 1, argsTypes.Length);

        ownerArgs[0] = OwnerType;
        argsTypes = ownerArgs;

        return owner;
      }

      this.ownerType = CompilerSupport.OwnerNType;
      return null;
    }

    protected static Delegate GetDelegate(
      DynamicMethod method, Type delegateType, object owner)
    {
      return owner == null ?
        method.CreateDelegate(delegateType) :
        method.CreateDelegate(delegateType, owner);
    }

    #endregion
    #region Static Data

    const BindingFlags InstanceNonPublic =
      BindingFlags.Instance | BindingFlags.NonPublic;

    // OpCodes =====================================

    // arguments:
    static readonly OpCode[] OpArgsLoad =
    {
      OpCodes.Ldarg_0,
      OpCodes.Ldarg_1,
      OpCodes.Ldarg_2,
      OpCodes.Ldarg_3
    };

    // int32 constants:
    static readonly OpCode[] OpLoadConst =
    {
      OpCodes.Ldc_I4_M1,
      OpCodes.Ldc_I4_0,
      OpCodes.Ldc_I4_1,
      OpCodes.Ldc_I4_2,
      OpCodes.Ldc_I4_3,
      OpCodes.Ldc_I4_4,
      OpCodes.Ldc_I4_5,
      OpCodes.Ldc_I4_6,
      OpCodes.Ldc_I4_7,
      OpCodes.Ldc_I4_8
    };

    #endregion
  }
}