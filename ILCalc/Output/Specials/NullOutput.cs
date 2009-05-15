using System.Reflection;

namespace ILCalc
{
	internal sealed class NullWriter : IExpressionOutput
	{
		public void PutNumber(double value)
		{
		}

		public void PutOperator(int oper)
		{
		}

		public void PutArgument(int id)
		{
		}

		public void PutSeparator()
		{
		}

		public void PutBeginCall()
		{
		}

		public void PutBeginParams(int fixCount, int varCount)
		{
		}

		public void PutMethod(MethodInfo method, int fixCount)
		{
		}

		public void PutFunction(FunctionItem func, int argsCount)
		{
		}

		public void PutExprEnd()
		{
		}
	}
}