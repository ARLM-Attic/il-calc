using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	internal static class FunctionFactory
		{
		#region Fields

		private static readonly Type RuntimeMethodType
			= typeof(Math).GetMethod("Sin").GetType();

		#endregion
		#region Methods

		public static FunctionItem CreateInstance(MethodInfo method, bool throwOnFailure)
		{
			// MethodInfo from DynamicMethod shouldn't pass here:
			if (method.GetType() != RuntimeMethodType)
			{
				if (throwOnFailure)
				{
					throw MethodImportFailure(
						method, Resource.errMethodNotRuntimeMethod);
				}

				return null;
			}

			// (should be removed in future)
			if (!method.IsStatic)
			{
				if (throwOnFailure)
				{
					throw MethodImportFailure(
						method, Resource.errMethodNotStatic);
				}

				return null;
			}

			// now validate return type:
			if (method.ReturnType != TypeHelper.ValueType)
			{
				if (throwOnFailure)
				{
					throw InvalidMethodReturn(method);
				}

				return null;
			}

			// and method parameters types:
			var args = method.GetParameters();
			foreach (ParameterInfo param in args)
			{
				if (param.ParameterType != TypeHelper.ValueType)
				{
					// maybe this is params method?
					if (IsParamArrayParameter(param, args.Length))
					{
						return new FunctionItem(method, args.Length - 1, true);
					}

					if (throwOnFailure)
					{
						throw InvalidParamType(method, param);
					}

					return null;
				}

#if !CF
				if (param.IsOut || param.IsOptional)
				{
					if (throwOnFailure)
					{
						throw InvalidParameter(method, param);
					}

					return null;
				}
#endif
			}

			return new FunctionItem(method, args.Length, false);
		}

#if !CF2

		[DebuggerHidden]
		public static bool CheckDelegate(Delegate target, bool throwOnFailure)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			// check the invocation count:
			if (target.GetInvocationList().Length != 1)
			{
				if (throwOnFailure)
				{
					throw new ArgumentException(Resource.errDelegateInvCount);
				}

				return false;
			}

			// stop DynamicMethod here:
			if (target.Method.GetType() != RuntimeMethodType)
			{
				if (throwOnFailure)
				{
					throw MethodImportFailure(
						target.Method, Resource.errMethodNotRuntimeMethod);
				}

				return false;
			}

			// delegates with targets are not supported:
			if (target.Target != null)
			{
				if (throwOnFailure)
				{
					throw new ArgumentException(
						Resource.errDelegateWithTarget);
				}

				return false;
			}

			return true;
			}

#endif

		[DebuggerHidden]
		public static MethodInfo GetHelper(Type type, string methodName, int argsCount)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (methodName == null)
				throw new ArgumentNullException("methodName");

			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			MethodInfo method = (argsCount < 0) ?
				type.GetMethod(methodName, Flags) :
				type.GetMethod(methodName, Flags, null, MakeParamsTypes(argsCount), null);

			if (method == null)
			{
				throw new ArgumentException(
					string.Format(Resource.errMethodNotFounded, methodName));
			}

			return method;
		}

		[DebuggerHidden]
		private static Type[] MakeParamsTypes(int count)
		{
			var paramTypes = new Type[count];

			for (int i = 0; i < count; i++)
			{
				paramTypes[i] = TypeHelper.ValueType;
			}

			return paramTypes;
		}

		#endregion
		#region Predicates

		private static bool IsParamArrayParameter(ParameterInfo param, int argsCount)
		{
			return param.Position == argsCount - 1
#if !CF
				&& !param.IsOptional
				&& !param.IsOut
#endif
				&& param.ParameterType == TypeHelper.ArrayType;
		}

		#endregion
		#region ThrowHelpers

		private static Exception InvalidMethodReturn(MethodInfo method)
		{
			Debug.Assert(method != null);

			return MethodImportFailure(
				method,
				Resource.errMethodBadReturn,
				method.ReturnType.FullName,
				TypeHelper.ValueType.Name);
		}

		private static Exception InvalidParamType(MethodInfo method, ParameterInfo param)
		{
			Debug.Assert(method != null);
			Debug.Assert(param != null);

			return MethodImportFailure(
				method,
				Resource.errMethodBadParam,
				param.Position,
				param.ParameterType.Name,
				TypeHelper.ValueType.Name);
		}

		private static Exception InvalidParameter(MethodInfo method, ParameterInfo param)
		{
			Debug.Assert(method != null);
			Debug.Assert(param != null);

			return MethodImportFailure(
				method,
				Resource.errMethodParamInvalid,
				param.Position);
		}

		[DebuggerHidden]
		private static Exception MethodImportFailure(
			MethodInfo method, string format, params object[] arguments)
		{
			var buf = new StringBuilder(Resource.errMethodImportFailed);

			buf.Append(" \"");
			buf.Append(method);
			buf.Append("\" ");
			buf.AppendFormat(format, arguments);

			return new ArgumentException(buf.ToString());
		}

		#endregion
	}
}