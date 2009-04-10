namespace ILCalc
	{
	delegate double EvalFunc0( );
	delegate double EvalFunc1( double arg );
	delegate double EvalFunc2( double arg1, double arg2 );
	delegate double EvalFuncN( params double[] args );
	}
