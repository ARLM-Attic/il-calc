using System.Reflection;

namespace ILCalc
	{
	sealed class NullWriter : IExpressionOutput
		{
		public void PutNumber   ( double value ) { }
		public void PutFunction ( MethodInfo func ) { }
		public void PutOperator ( int oper ) { }
		public void PutArgument ( int id ) { }
		public void PutSeparator( ) { }
		public void BeginCall   ( int fixCount, int varCount ) { }
		public void PutExprEnd  ( ) { }
		}
	}
