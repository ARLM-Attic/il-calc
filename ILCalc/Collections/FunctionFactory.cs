using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
{
	// TODO: tests!

	internal static class FunctionFactory
	{
		#region Fields

		// standard runtime method type:
		private static readonly Type RuntimeMethodType =
			typeof(FunctionFactory).GetMethod("TryResolve").GetType();

		#endregion
		#region Methods

		public static FunctionItem FromReflection(
			MethodInfo method, object target, bool throwOnFailure)
		{
			// DynamicMethod shouldn't pass here:
			if (method.GetType() != RuntimeMethodType)
			{
				if (throwOnFailure)
					throw MethodImportFailure(method, Resource.errMethodNotRuntimeMethod);

				return null;
			}

			// validate static methods:
			if (target == null && !method.IsStatic)
			{
				if (throwOnFailure)
					throw MethodImportFailure(method, Resource.errMethodNotStatic);

				return null;
			}

			// validate target type:
			if (target != null)
			{
				if (method.IsStatic)
				{
					if (throwOnFailure)
						throw MethodImportFailure(method,
							Resource.errMethodNotInstance);

					return null;
				}

				Debug.Assert(!method.IsStatic);

				Type thisType = method.DeclaringType;
				Type targetType = target.GetType();

				if (!thisType.IsAssignableFrom(targetType))
				{
					if (throwOnFailure)
						throw InvalidMethodTarget(thisType, targetType);

					return null;
				}
			}

			// validate return type:
			if (method.ReturnType != TypeHelper.ValueType)
			{
				if (throwOnFailure)
					throw InvalidMethodReturn(method);

				return null;
			}

			// and method parameters types:
			var parameters = method.GetParameters();
			foreach (ParameterInfo p in parameters)
			{
				if (p.ParameterType != TypeHelper.ValueType)
				{
					// maybe this is params method?
					if (p.Position == parameters.Length - 1
#if !CF
						&& !p.IsOptional
						&& !p.IsOut
#endif
						&& p.ParameterType == TypeHelper.ArrayType)
					{
						return new FunctionItem(
							method, target, parameters.Length - 1, true);
					}

					if (throwOnFailure)
						throw InvalidParamType(method, p);

					return null;
				}

#if !CF
				if (p.IsOut || p.IsOptional)
				{
					if (throwOnFailure)
						throw InvalidParameter(method, p);

					return null;
				}
#endif
			}

			return new FunctionItem(
				method, target, parameters.Length, false);
		}

		

#if !CF2

		public static FunctionItem FromDelegate(
			Delegate target, int argsCount, bool hasParams, bool throwOnFailure)
		{
			Debug.Assert(argsCount >= 0);

			// common callers argument check:
			if (target == null)
				throw new ArgumentNullException("target");

			// check the invocation count:
			if (target.GetInvocationList().Length != 1)
			{
				if (throwOnFailure)
					throw new ArgumentException(
						Resource.errDelegateInvCount);

				return null;
			}

			// stop DynamicMethod here:
			if (target.Method.GetType() != RuntimeMethodType)
			{
				// cool hack here =)
				// get the Delegate invoker:
				var invoker = target.GetType().GetMethod("Invoke");

				// and create function with Delegate target:
				return new FunctionItem(
					invoker, target, argsCount, hasParams);
			}

			return new FunctionItem(
				target.Method, target.Target, argsCount, hasParams);
		}

#endif

		public static MethodInfo TryResolve(
			Type type, string name, int argsCount)
		{
			// common arguments checks:
			if (type == null) throw new ArgumentNullException("type");
			if (name == null) throw new ArgumentNullException("name");

			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			MethodInfo method = (argsCount < 0) ?
				type.GetMethod(name, Flags) :
				type.GetMethod(name, Flags, null, MakeArgs(argsCount), null);

			if (method == null)
			{
				throw new ArgumentException(
					string.Format(Resource.errMethodNotFounded, name));
			}

			return method;
		}

		#endregion
		#region Helpers

		private static Type[] MakeArgs(int count)
		{
			var types = new Type[count];
			for (int i = 0; i < types.Length; i++)
			{
				types[i] = TypeHelper.ValueType;
			}

			return types;
		}

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
			Debug.Assert(param  != null);

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

		private static Exception InvalidMethodTarget(Type thisType, Type targetType)
		{
			return new ArgumentException(string.Format(
				Resource.errWrongTargetType,
				targetType.FullName,
				thisType.FullName));
		}

		private static Exception MethodImportFailure(
			MethodInfo method, string format, params object[] arguments)
		{
			Debug.Assert(format != null);
			Debug.Assert(arguments != null);

			var buf = new StringBuilder(
				Resource.errMethodImportFailed);

			buf.Append(" \"")
			   .Append(method)
			   .Append("\" ")
			   .AppendFormat(format, arguments);

			return new ArgumentException(buf.ToString());
		}

		#endregion
	}
}