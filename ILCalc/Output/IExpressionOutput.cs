namespace ILCalc
{
	internal interface IExpressionOutput
	{
		void PutNumber(double value);

		void PutOperator(int oper);

		void PutArgument(int id);

		void PutSeparator();

		void PutBeginCall();

		void PutBeginParams(int fixCount, int varCount);

		void PutFunction(FunctionItem func, int argsCount);

		void PutExprEnd();
	}
}
