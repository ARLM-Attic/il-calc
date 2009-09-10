namespace ILCalc
{
  sealed class NullWriter<T> : IExpressionOutput<T>
  {
    public void PutConstant(T value) { }
    public void PutOperator(int oper) { }
    public void PutArgument(int id) { }
    public void PutSeparator() { }
    public void PutBeginCall() { }
    public void PutCall(FunctionItem<T> func, int argsCount) { }
    public void PutExprEnd() { }
  }
}