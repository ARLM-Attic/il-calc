using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	static class FunctionFactory
		{
		#region Fields

		public static readonly Type valueType = typeof(double);
		public static readonly Type arrayType = typeof(double[]);
		
		private static readonly Type runtimeMethodType
			= typeof(Math).GetMethod("Sin").GetType( );

		#endregion
		#region Methods

		public static Function CreateInstance( MethodInfo method, bool throwOnFailure )
			{
			// MethodInfo from DynamicMethod shouldn't pass here:
			if( method.GetType() != runtimeMethodType )
				{
				if( throwOnFailure )
					{
					throw MethodImportFailure(method,
						Resources.errMethodNotRuntimeMethod);
					}

				return null;
				}

			// (should be removed in future)
			if( !method.IsStatic )
				{
				if( throwOnFailure )
					{
					throw MethodImportFailure(method,
						Resources.errMethodNotStatic);
					}

				return null;
				}

			// now validate return type:
			if( method.ReturnType != valueType )
				{
				if( throwOnFailure )
					throw InvalidMethodReturn(method);

				return null;
				}

			// and method parameters types:
			var args = method.GetParameters( );
			foreach( ParameterInfo param in args )
				{
				if( param.ParameterType != valueType )
					{
					// maybe this is params method?
					if( IsParamArrayParameter(param, args.Length) )
						{
						return new Function(method, args.Length - 1, true);
						}

					if( throwOnFailure )
						throw InvalidParamType(method, param);

					return null;
					}

#if !CF
				if( !IsValidParameter(param) )
					{
					if( throwOnFailure )
						throw InvalidParameter(method, param);

					return null;
					}
#endif
				}

			return new Function(method, args.Length, false);
			}

		[DebuggerHidden]
		public static bool CheckDelegate( Delegate deleg, bool throwOnFailure )
			{
			if( deleg == null )
				throw new ArgumentNullException("deleg");

			// check the invocation count:
			if( deleg.GetInvocationList( ).Length != 1 )
				{
				if( throwOnFailure )
					{
					throw new ArgumentException(
						Resources.errDelegateInvCount);
					}

				return false;
				}

			// stop DynamicMethod here:
			if( deleg.Method.GetType( ) != runtimeMethodType )
				{
				if( throwOnFailure )
					{
					throw MethodImportFailure(deleg.Method,
						Resources.errMethodNotRuntimeMethod);
					}

				return false;
				}

			// delegates with targets are not supported:
			if( deleg.Target != null )
				{
				if( throwOnFailure )
					{
					throw new ArgumentException(Resources.errDelegateWithTarget);
					}

				return false;
				}

			return true;
			}

		[DebuggerHidden]
		public static MethodInfo GetHelper( Type type, string methodName, int argsCount )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			if( methodName == null )
				throw new ArgumentNullException("methodName");

			const BindingFlags flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			MethodInfo method = (argsCount < 0) ?
				type.GetMethod(methodName, flags) :
				type.GetMethod(methodName, flags, null,
					MakeParamsTypes(argsCount), null);

			if( method == null )
				{
				throw new ArgumentException(
					string.Format(Resources.errMethodNotFounded, methodName)
					);
				}

			return method;
			}

		[DebuggerHidden]
		private static Type[] MakeParamsTypes( int count )
			{
			var paramTypes = new Type[count];

			for( int i = 0; i < count; i++ )
				{
				paramTypes[i] = valueType;
				}

			return paramTypes;
			}

		#endregion
		#region Predicates

#if !CF

		private static bool IsValidParameter( ParameterInfo param )
			{
			return !param.IsOut
				&& !param.IsOptional;
			}

#endif

		private static bool IsParamArrayParameter( ParameterInfo param, int argsCount )
			{
			return param.Position == argsCount - 1
				&& param.ParameterType == arrayType
#if !CF
				&& !param.IsOptional
				&& !param.IsOut
#endif
				;
			}

		#endregion
		#region ThrowHelpers

		private static ArgumentException InvalidMethodReturn( MethodInfo method )
			{
			return MethodImportFailure(method,
				Resources.errMethodBadReturn,
				method.ReturnType.FullName,
				valueType.Name);
			}

		private static ArgumentException InvalidParamType(
				MethodInfo method, ParameterInfo param )
			{
			return MethodImportFailure(method,
				Resources.errMethodBadParam,
				param.Position,
				param.ParameterType.Name,
				valueType.Name);
			}

		private static ArgumentException InvalidParameter( MethodInfo method,
				ParameterInfo param )
			{
			return MethodImportFailure(method,
				Resources.errMethodParamInvalid,
				param.Position);
			}

		[DebuggerHidden]
		private static ArgumentException MethodImportFailure( MethodInfo func,
				string format, params object[] args )
			{
			var buf = new StringBuilder(Resources.errMethodImportFailed);

			buf.Append(" \""); buf.Append(func);
			buf.Append("\" "); buf.AppendFormat(format, args);

			return new ArgumentException(buf.ToString( ));
			}

		#endregion
		}
	}