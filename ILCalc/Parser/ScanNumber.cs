using System;
using System.Globalization;

namespace ILCalc
{
	internal sealed partial class Parser
	{
		private double ScanNumber(char c, ref int i)
		{
			// =================================== Fractal part ==

			// numbers not like ".123"
			if (c != dotSymbol)
			{
				// skip digits
				while (i < this.exprLen
					&& char.IsDigit(this.expr[i]))
				{
					i++;
				}

				// skip dot
				if (i < this.exprLen
				 && this.expr[i] == dotSymbol)
				{
					i++;
				}
			}

			// skip digits
			while (i < this.exprLen
				&& char.IsDigit(this.expr[i]))
			{
				i++;
			}

			// ================================ Exponental part ==

			// at least 2 chars
			if (i + 1 < this.exprLen)
			{
				// E character
				c = this.expr[i];
				if (c == 'e' || c == 'E')
				{
					int j = i;

					// exponetal sign
					c = this.expr[++j];
					if (c == '-' || c == '+')
					{
						j++;
					}

					// eponental part
					if (i < this.exprLen
					 && char.IsDigit(this.expr[j]))
					{
						j++;
						while (j < this.exprLen
							&& char.IsDigit(this.expr[j]))
						{
							j++;
						}

						i = j;
					}
				}
			}

			// =================================== Try to parse ==

			// extract number substring
			string number = this.expr.Substring(this.curPos, i - this.curPos);
			try
			{
				return Double.Parse(
					number,
					NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
					numFormat);
			}
			catch (FormatException e)
			{
				throw this.NumberFormat(Resource.errNumberFormat, number, e);
			}
			catch (OverflowException e)
			{
				throw this.NumberFormat(Resource.errNumberOverflow, number, e);
			}
		}
	}
}