namespace ILCalc
{
  interface IExpressionOutput<T>
  {
    void PutConstant(T value);
    void PutOperator(Code oper);
    void PutArgument(int id);
    void PutSeparator();
    void PutBeginCall();
    void PutCall(FunctionInfo<T> func, int args);
    void PutExprEnd(); // => remove?
  }
}