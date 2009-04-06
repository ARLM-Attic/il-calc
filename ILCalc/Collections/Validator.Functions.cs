using System;
using System.Reflection;

namespace ILCalc
	{
	static partial class Validator
		{
		private static readonly Type _runtimeMethod =
			Type.GetType("System.Reflection.RuntimeMethodInfo");

		// TODO: perform this while adding to list

		public static bool IsImportable( MethodInfo func )
			{
			if( func.GetType( ) != _runtimeMethod ||
				func.ReturnType != T_type ||
				!func.IsStatic )
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
					&&	param.ParameterType == T_array
					&&	!param.IsOptional
					&&	!param.IsOut )
						{
						return true;
						}

					res = false;
					break;
					}

				if( param.IsOut || param.IsOptional )
					{
					res = false;
					break;
					}
				}

			return res;
			}

		public static void ImportableMethod( string name, MethodInfo func )
			{
			if( func.GetType() != _runtimeMethod )
				{
				throw FuncError(name, func,
					Resources.errMethodNotRuntimeMethod
					);
				}

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
				if( param.ParameterType != T_type )
					{
					if( param.Position != args.Length - 1
					||	param.ParameterType != T_array
					||	param.IsOptional
					||	param.IsOut )
						{
						throw FuncError(name, func,
							Resources.errMethodBadParam,
							param.Position,
							param.ParameterType.Name,
							T_type.Name);
						}

					return;
					}

				if( param.IsOut )
					{
					throw FuncError(name, func,
						Resources.errMethodParamOut,
						param.Position);
					}

				if( param.IsOptional )
					{
					throw FuncError(name, func,
						Resources.errMethodParamOptional,
						param.Position);
					}
				}
			}
		}
	}
