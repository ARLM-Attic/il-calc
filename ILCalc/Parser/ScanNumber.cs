using System;
using System.Globalization;

namespace ILCalc
	{
	sealed partial class Parser
		{
		private double ScanNumber( char c, ref int i )
			{
			// =================================== Fractal part ==

			// numbers not like ".123"
			if( c != dotSymbol )
				{
				// skip digits
				while( i < exprLen && Char.IsDigit(expr[i]) ) i++;

				// skip dot
				if( i < exprLen && expr[i] == dotSymbol ) i++;
				}

			// skip digits
			while( i < exprLen && Char.IsDigit(expr[i]) ) i++;

			// ================================ Exponental part ==

			// at least 2 chars
			if(i + 1 < exprLen)
				{
				// E character
				c = expr[i];
				if( c == 'e' || c == 'E' )
					{
					int j = i;

					// exponetal sign
					c = expr[++j];
					if( c == '-' || c == '+' ) j++;

					// eponental part
					if( i < exprLen && Char.IsDigit(expr[j]) )
						{
						j++;
						while( j < exprLen && Char.IsDigit(expr[j]) ) j++;
						i = j;
						}
					}
				}

			// =================================== Try to parse ==

			// extract number substring
			string number = expr.Substring(curPos, i - curPos);
			try
				{
				return Double.Parse(number,
					NumberStyles.AllowDecimalPoint |
					NumberStyles.AllowExponent, numFormat);
				}
			catch(FormatException e)
				{
				throw NumberFormat(number, e);
				}
			catch(OverflowException e)
				{
				throw NumberOverflow(number, e);
				}
			}
		}
	}
