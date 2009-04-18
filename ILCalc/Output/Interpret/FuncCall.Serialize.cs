using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ILCalc
	{
	sealed partial class FuncCall : ISerializable
		{
		private static readonly Type mInfoType = typeof(MethodInfo);

		private FuncCall( SerializationInfo info, StreamingContext context )
			{
			int fixCount = info.GetInt32("fix");
			int varCount = info.GetInt32("var");

			fixArgs = new object[fixCount];

			if( varCount >= 0 )
				{
				varArgs = new double[varCount];
				fixArgs[--fixCount] = varArgs;
				}
			else varArgs = null;

			lastIndex = fixCount - 1;
			syncRoot = new object( );
			method = (MethodInfo) info.GetValue("method", mInfoType);
			}

		[SecurityPermission(SecurityAction.LinkDemand,
			Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData( SerializationInfo info, StreamingContext context )
			{
			info.AddValue("fix", fixArgs.Length);
			info.AddValue("var", varArgs == null? -1: varArgs.Length);
			info.AddValue("method", method, mInfoType);
			}
		}
	}
