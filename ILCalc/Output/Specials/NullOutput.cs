namespace ILCalc
{
	internal sealed class NullWriter : IExpressionOutput
	{
		public void PutNumber(double value) { }
		public void PutOperator(int oper) { }
		public void PutArgument(int id) { }
		public void PutSeparator() { }
		public void PutBeginCall() { }
		public void PutFunction(FunctionItem func, int argsCount) { }
		public void PutExprEnd() { }
	}
}