namespace ILCalc
	{
	// NOTE: enumeration?

	static class Code
		{
		//public const int None = -1;
		public const int Return = 255;

		// Operators ======================
		public const int Sub = 0;
		public const int Add = 1;
		public const int Mul = 2;
		public const int Div = 3;
		public const int Rem = 4;
		public const int Pow = 5;
		public const int Neg = 6;

		// Elements =======================
		public const int Number    = 8;
		public const int Argument  = 9;
		public const int Function  = 10;
		public const int Separator = 11;
		public const int ParamCall = 12;
		public const int BeginCall = 13;
		
		// For Interpret ==================
		public const int Delegate0 = 16;
		public const int Delegate1 = 17;
		public const int Delegate2 = 18;

		public static bool IsOperator( int code )
			{
			return code < Number;
			}
		}
	}
