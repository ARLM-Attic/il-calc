namespace ILCalc
{
  sealed class NullWriter<T> : IExpressionOutput<T>
  {
    public static readonly
      NullWriter<T> Instance = new NullWriter<T>();

    public void PutConstant(T value) { }
    public void PutOperator(Code oper) { }
    public void PutArgument(int id) { }
    public void PutSeparator() { }
    public void PutBeginCall() { }
    public void PutCall(FunctionInfo<T> func, int args) { }
    public void PutExprEnd() { }
  }
}