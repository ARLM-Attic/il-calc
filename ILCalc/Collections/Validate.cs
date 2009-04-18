using System;
using System.Text;

namespace ILCalc
	{
	static class Validate
		{
		public static void Name( string name )
			{
			if( string.IsNullOrEmpty(name) )
				throw new ArgumentException(Resources.errIdentifierEmpty);

			char first = name[0];
			if( !char.IsLetter(first) && first != '_' )
				{
				throw InvalidFirstSymbol(name, first);
				}

			for( int i = 1; i < name.Length; i++ )
				{
				char ch = name[i];
				if( !char.IsLetterOrDigit(ch) && ch != '_' )
					{
					throw new ArgumentException(
						string.Format(Resources.errIdentifierSymbol, ch, name)
						);
					}
				}
			}

		private static ArgumentException InvalidFirstSymbol( string name, char first )
			{
			var buf = new StringBuilder( );
			buf.AppendFormat(Resources.errIdentifierStartsWith, name);

			if(first == '<')
				{
				buf.Append(' ');
				buf.Append(Resources.errIdentifierFromLambda);
				}

			return new ArgumentException(buf.ToString( ));
			}
		}
	}