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
			if( c != _dot )
				{
				// skip digits
				while( i < _len && Char.IsDigit(_expr[i]) ) i++;

				// skip dot
				if( i < _len && _expr[i] == _dot ) i++;
				}

			// skip digits
			while( i < _len && Char.IsDigit(_expr[i]) ) i++;

			// ================================ Exponental part ==

			// at least 2 chars
			if(i + 1 < _len)
				{
				// E character
				c = _expr[i];
				if( c == 'e' || c == 'E' )
					{
					int j = i;

					// exponetal sign
					c = _expr[++j];
					if( c == '-' || c == '+' ) j++;

					// eponental part
					if( i < _len && Char.IsDigit(_expr[j]) )
						{
						j++;
						while( j < _len && Char.IsDigit(_expr[j]) ) j++;
						i = j;
						}
					}
				}

			// =================================== Try to parse ==

			// extract number substring
			string number = _expr.Substring(_curPos, i - _curPos);
			try
				{
				return Double.Parse(number,
					NumberStyles.AllowDecimalPoint |
					NumberStyles.AllowExponent, _numf);
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
