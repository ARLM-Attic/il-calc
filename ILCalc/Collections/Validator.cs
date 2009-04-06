using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	static partial class Validator
		{
		#region Fields

		public static readonly Type T_type  = typeof( double );
		public static readonly Type T_array = typeof( double[] );

		#endregion

		public static void IdentifierName( string name )
			{
			if( string.IsNullOrEmpty(name) )
				{
				throw new ArgumentException(Resources.errIdentifierEmpty);
				}

			if( !char.IsLetter(name[0]) && name[0] != '_' )
				{
				throw new ArgumentException(
					string.Format(Resources.errIdentifierStartsWith, name)
					);
				}

			foreach( char c in name )
				{
				if( !char.IsLetterOrDigit(c) && c != '_' )
					{
					throw new ArgumentException(
						string.Format(Resources.errIdentifierSymbol, c, name)
						);
					}
				}
			}

		// NOTE: maybe rewrite

		[DebuggerHidden]
		public static ArgumentException FuncError(
				string name, MethodInfo func,
				string format, params object[] args )
			{
			var buf = new StringBuilder(Resources.sFunction);

			buf.Append(" \"");
			buf.Append(name); buf.Append("\" = ");
			buf.Append(func); buf.Append(' ');
			buf.AppendFormat(format, args);

			return new ArgumentException(buf.ToString( ));
			}
		}
	}