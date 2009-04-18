using System.Reflection;

namespace ILCalc
	{
	interface IExpressionOutput
		{
		void PutNumber( double value );
		void PutOperator( int oper );
		void PutArgument( int id );
		void PutSeparator( );
		void PutBeginCall( );
		void PutBeginParams( int fixCount, int varCount );
		void PutMethod( MethodInfo method, int fixCount );
		void PutExprEnd( );

		// NOTE: for the future
		// void PutDelegate( System.Delegate func );
		}
	}
