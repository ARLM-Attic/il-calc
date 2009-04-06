using System.Reflection;

namespace ILCalc
	{
	interface IExpressionOutput
		{
		void PutNumber   ( double value );
		void PutFunction ( MethodInfo func );
		void PutOperator ( int oper );
		void PutArgument ( int id );
		void PutSeparator( );
		void BeginCall   ( int fixCount, int varCount );
		void PutExprEnd  ( );

		// NOTE: separate?
		//void PutBeginCall( );
		//void PutBeginParams( int fixCount, int varCount );

		// NOTE: for the future
		// void PutFunction( System.Delegate func );
		}
	}
