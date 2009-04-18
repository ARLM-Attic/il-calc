using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ILCalc.Tests
	{
	using FuncPair = KeyValuePair<string, Function>;

	public class ExprGenerator
		{
		#region Fields

		private static readonly Random random = new Random( );
		private readonly CalcContext context;
		private readonly CultureInfo culture;
		private readonly NumberFormatInfo format;
		private readonly List<string> idens;
		private readonly List<FuncPair> funcs;
		private readonly char separator;

		public CalcContext Context
			{
			get { return context; }
			}

		#endregion
		#region Constructor

		public ExprGenerator(CalcContext calc)
			{
			context = calc;
			culture = calc.Culture ?? CultureInfo.InvariantCulture;
			format = culture.NumberFormat;
			separator = culture.TextInfo.ListSeparator[0];

			idens = new List<string>( );

			if( calc.Arguments != null ) idens.AddRange(calc.Arguments);
			if( calc.Constants != null ) idens.AddRange(calc.Constants.Keys);

			funcs = new List<FuncPair>( );

			if( calc.Functions != null )
			foreach( var item in calc.Functions )
				{
				string name = item.Key;
				foreach( Function f in item.Value )
					{
					funcs.Add(new FuncPair(name, f));
					}
				}
			}

		#endregion
		#region Generators

		private static void PutSpace( StringBuilder buf )
			{
			// 0-3 space characters
			for( int count = FromTo(0, 3); count > 0; count-- )
				{
				buf.Append(' ');
				}
			}

		private static void PutNumber( StringBuilder buf, IFormatProvider format )
			{
			// pow(int, int)
			int del = 1;
			for( int i = FromTo(6, 9); i > 0; i-- ) del *= 10;

			int frac = FromTo( int.MinValue / del,
							   int.MaxValue / del );

			if( OneOf(4) )
				{
				buf.AppendFormat(format,
					OneOf(3)? "{0:E3}": "{0:G}", 
					frac * Math.Pow(10, FromTo(-250, 250))
					);
				}
			else
				{
				if( OneOf(3) )
					 buf.AppendFormat(format, "{0:F}", frac);
				else buf.Append(frac);
				}
			}

		private static void PutOperator( StringBuilder buf )
			{
			buf.Append(OneOf(7)? '%': "+-*/^"[FromTo(0, 4)]);
			}

		private void PutIdentifier( StringBuilder buf )
			{
			string name = idens[FromTo(0, idens.Count)];

			buf.Append(
				context.IgnoreCase? name:
				RandomCase(name, culture)
				);
			}

		private void PutValueItem( StringBuilder buf )
			{
			if( OneOf(3) )
				{
				if( idens.Count > 0 )
					 PutIdentifier(buf);
				else PutNumber(buf, format);
				}
			else PutNumber(buf, format);
			}

		private void PutFunction( StringBuilder buf, int depth )
			{
			FuncPair pair = funcs[FromTo(0, funcs.Count)];
			Function f = pair.Value;

			buf.Append(pair.Key); PutSpace(buf);
			buf.Append('(');

			int count = f.ArgsCount;
			if( f.HasParamArray && !OneOf(4) )
				{
				count += FromTo(0, 10);
				}

			for( int i = 0; i < count; i++ )
				{
				PutExpression(buf, depth);

				//separator
				if( i + 1 != count ) buf.Append(separator);
				}

			buf.Append(')');
			}

		private void PutExpression( StringBuilder buf, int depth )
			{
			PutValueItem(buf);
			for( int i = 1; i < depth; i++ )
				{
				if( OneOf(depth) )
					{
					PutSpace(buf);
					PutOperator(buf);

					if( OneOf(3) ) PutBraceExpr(buf, depth - 1);
					else
						{
						if( OneOf(4) )
							PutNumber(buf, format);
						PutFunction(buf, depth - 1);
						}
					}
				else
					{
					PutSpace(buf); PutOperator(buf);
					PutSpace(buf); PutValueItem(buf);
					}
				}
			}

		private void PutBraceExpr( StringBuilder buf, int depth )
			{
			PutSpace(buf);

			if( OneOf(3) && idens.Count != 0 )
				{
				PutValueItem(buf);
				PutSpace(buf);
				}

			buf.Append('(');
			PutSpace(buf);
			PutExpression(buf, depth);
			PutSpace(buf);
			buf.Append(')');
			}

		public string Next( )
			{
			var buf = new StringBuilder(16);
			PutExpression(buf, FromTo(3, 5));
			return buf.ToString( );
			}

		public Enumerator Generate( int count )
			{
			return new Enumerator(this, count, FromTo(3, 5));
			}

		public struct Enumerator : IEnumerable<string>, IEnumerator<string>
			{
			private readonly ExprGenerator gen;
			private readonly int count;
			private readonly int depth;

			private string expr;
			private int i;

			public string Current		{ get { return expr; } }
			object IEnumerator.Current	{ get { return expr; } }

			public void Dispose( )	{ }
			public void Reset( )	{ i = 0; }

			public bool MoveNext( )
				{
				if( i < count )
					{
					i++;
					var buf = new StringBuilder(8);
					gen.PutExpression(buf, depth);
					expr = buf.ToString( );
					return true;
					}

				return false;
				}

			public IEnumerator<string> GetEnumerator( )	{ return this; }
			IEnumerator IEnumerable.GetEnumerator( )	{ return this; }

			internal Enumerator(ExprGenerator gen, int count, int depth)
				{
				i = 0;
				this.gen = gen;
				this.count = count;
				this.depth = depth;
				expr = string.Empty;
				}
			}

		#endregion
		#region Helpers

		private static bool OneOf( int number )
			{
			return random.Next() % number == 0;
			}

		private static int FromTo( int min, int max )
			{
			return random.Next(min, max);
			}

		private static string RandomCase( string text, CultureInfo culture )
			{
			if( OneOf(3) ) switch( random.Next( ) % 3 )
				{
				case 0: return text.ToLowerInvariant( );
				case 1: return text.ToUpperInvariant( );
				case 2: return culture.TextInfo.ToTitleCase(text);
				default: return text;
				}

			var buf = new StringBuilder(text.Length);
			bool flag = OneOf(2);

			for( int i = 0, len = text.Length; i < len; flag = !flag )
				{
				int ln = FromTo(1, len - i);

				string part = text.Substring(i, ln);
				buf.Append(flag?
					part.ToUpperInvariant( ):
					part.ToLowerInvariant( )
					);

				i += ln;
				}

			return buf.ToString( );
			}

		#endregion
		}
	}
