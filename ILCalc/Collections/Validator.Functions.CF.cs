using System.Reflection;

namespace ILCalc
	{
	static partial class Validator
		{
		public static bool IsImportable( MethodInfo func )
			{
			if( func.ReturnType != T_type || !func.IsStatic )
				{
				return false;
				}

			var args = func.GetParameters( );
			bool res = true;

			foreach( ParameterInfo param in args )
				{
				if( param.ParameterType != T_type )
					{
					if( param.Position == args.Length - 1
					&&	param.ParameterType == T_array )
						{
						return true;
						}

					res = false;
					break;
					}
				}

			return res;
			}

		public static void ImportableMethod( string name, MethodInfo func )
			{
			if( !func.IsStatic )
				{
				throw FuncError(name, func,
					Resources.errMethodNotStatic
					);
				}

			if( func.ReturnType != T_type )
				{
				throw FuncError(name, func,
					Resources.errMethodBadReturn,
					func.ReturnType.FullName,
					T_type.Name
					);
				}

			var args = func.GetParameters( );

			foreach( ParameterInfo param in args )
				{
				if( param.ParameterType == T_type ) continue;
				
				if(	param.Position != args.Length - 1
				||	param.ParameterType != T_array )
					{
					throw FuncError(name, func,
						Resources.errMethodBadParam,
						param.Position,
						param.ParameterType.Name,
						T_type.Name);
					}

				return;
				}
			}
		}
	}
