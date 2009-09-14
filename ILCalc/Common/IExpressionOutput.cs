namespace ILCalc
{
  interface IExpressionOutput<T>
  {
    void PutConstant(T value);
    void PutOperator(int oper);
    void PutArgument(int id);
    void PutSeparator();
    void PutBeginCall();
    void PutCall(FunctionInfo<T> func, int argsCount);
    void PutExprEnd(); //TODO: remove?
  }
}