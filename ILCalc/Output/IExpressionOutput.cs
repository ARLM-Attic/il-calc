namespace ILCalc
{
	internal interface IExpressionOutput
	{
		void PutNumber(double value);
		void PutOperator(int oper);
		void PutArgument(int id);
		void PutSeparator();
		void PutBeginCall();

		// TODO: rename => PutCall?
		void PutFunction(FunctionItem func, int argsCount);
		void PutExprEnd();

		// TODO: bool SendCallBegins { get; } is it needed?
	}
}
