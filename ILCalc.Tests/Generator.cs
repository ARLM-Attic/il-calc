using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ILCalc.Tests
	{
	public class Generator
		{
		#region Fields

		private readonly Random _rnd = new Random( );
		private CalcContext _calc;

		#endregion
		#region Properties

		public CalcContext Calc
			{
			get { return _calc; }
			set { _calc = value; }
			}

		public CultureInfo Culture
			{
			get { return _calc.Culture ?? CultureInfo.InvariantCulture; }
			}

		#endregion
		#region Constructor

		public Generator(CalcContext calc)
			{
			_calc = calc;
			}

		#endregion
		#region Generators

		private void GenWhitespace( StringBuilder buf )
			{
			for( int count = rnd(0, 2); count > 0; count-- )
				{
				buf.Append(' ');
				}
			}

		private void GenNumber( StringBuilder buf )
			{
			int del = 1;

			for( int len = rnd(5, 9); len > 0; len-- )
				{
				del *= 10;
				}
			
			int frac = rnd( int.MinValue / del,
							int.MaxValue / del );

			if( rndFrom(4) )
				{
				int exp = rnd(-250, 250);
				double value = frac * Math.Pow(10, exp);

				buf.AppendFormat(rndFrom(3)? "{0:E3}": "{0:G}", value);
				}
			else
				{
				if( rndFrom(4) )
					 buf.AppendFormat(Culture, "{0:F}", frac);
				else buf.Append(frac);
				}
			}

		private void GenOperator( StringBuilder buf )
			{
			if( rndFrom(6) )
				 buf.Append(rndFrom(3)? '%': '^');
			else buf.Append("+-*/"[rnd(0, 3)]);
			}

		private void GenItem( StringBuilder buf )
			{
			if( !rndFrom(3) )
				{
				GenNumber(buf);
				}
			else
				{
				if(!GenIden(buf))
					{
					GenNumber(buf);
					}
				}
			}

		private bool GenIden( StringBuilder buf )
			{
			var aList = _calc.Arguments;
			var cList = _calc.Constants;

			if( rndFrom(2) )
				{
				if( aList.Count == 0 ) return false;
				GenName(buf, GetRandomArg(aList));
				}
			else
				{
				if( cList.Count == 0 ) return false;
				GenName(buf, GetRandomConst(cList));
				}

			return true;
			}

		private void GenName( StringBuilder buf, string name )
			{
			if( _calc.IgnoreCase ) 
				 buf.Append(RandomCase(name));
			else buf.Append(name);
			}

		private void GenExpression( StringBuilder buf, int depth )
			{
			GenItem(buf);
			for(int i = 1; i < depth; i++)
				{
				if( rndFrom(depth) )
					{
					GenWhitespace(buf);

					if( !rndFrom(3) )
						{
						GenOperator(buf);
						GenWhitespace(buf);
						}

					buf.Append('(');
					GenExpression(buf, depth-1);
					GenWhitespace(buf);
					buf.Append(')');

					if( !rndFrom(4) ) continue;

					GenWhitespace(buf);
					if( !rndFrom(2) )
						{
						if( !GenIden(buf) )
							{
							GenSubExpr(buf, depth);
							}
						}
					else GenSubExpr(buf, depth);
					}
				else
					{
					GenWhitespace(buf);
					GenOperator(buf);
					GenWhitespace(buf);
					GenItem(buf);
					}
				}
			}

		private void GenSubExpr( StringBuilder buf, int depth )
			{
			buf.Append('(');
			GenExpression(buf, depth - 1);
			GenWhitespace(buf);
			buf.Append(')');
			}

		#endregion
		#region Helpers

		private bool rndFrom( int number )
			{
			return _rnd.Next() % number == 0;
			}

		private int rnd( int min, int max )
			{
			return _rnd.Next(min, max);
			}

		private string GetRandomArg( IList<string> list )
			{
			int index = _rnd.Next(0, list.Count);

			return list[index];
			}

		private string GetRandomConst( IDictionary<string, double> list )
			{
			int index = _rnd.Next(0, list.Count);

			int i = 0;
			foreach(string name in list.Keys)
				{
				if(i == index) return name;
				i++;
				}

			throw new Exception( );
			}

		private string RandomCase( string text )
			{
			if( !rndFrom(3) )
				{
				var buf = new StringBuilder( text.Length );
				bool flag = rndFrom(2);

				for(int i = 0, len = text.Length; i < len; )
					{
					int ln = rnd(1, len - i);

					string part = text.Substring(i, ln);
					buf.Append(flag?
						part.ToUpperInvariant( ):
						part.ToLowerInvariant( )
						);

					flag = !flag;
					i += ln;
					}

				return buf.ToString( );
				}

			switch( _rnd.Next( ) % 3 )
				{
				case 0: return text.ToLowerInvariant( );
				case 1: return text.ToUpperInvariant( );
				case 2: return Culture.TextInfo.ToTitleCase(text);
				default: return text;
				}
			}

		#endregion
		#region Members

		public string Next( )
			{
			var buf = new StringBuilder(16);
			GenExpression(buf, rnd(3, 5));
			return buf.ToString( );
			}

		#endregion
		}
	}
