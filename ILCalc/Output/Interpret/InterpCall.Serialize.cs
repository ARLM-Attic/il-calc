using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ILCalc
	{
	sealed partial class InterpCall : ISerializable
		{
		[NonSerialized]
		private static readonly Type _minfoType = typeof(MethodInfo);

		#region Serialization

		// TODO: test!
		// TODO: не доделал ваще!

		private InterpCall( SerializationInfo info, StreamingContext context )
			{
			if( info == null )
				{
				throw new ArgumentNullException("info");
				}

			int fixCount = info.GetInt32("fix");
			fixArgs = new object[fixCount];

			int lastIndex = fixCount - 1;
			if( info.GetBoolean("params") )
				{
				argLastIdx = fixCount - 2;
				varArgs = new double[info.GetInt32("var")];
				fixArgs[lastIndex] = varArgs;
				}
			else
				{
				argLastIdx = lastIndex;
				varArgs = null;
				}

			method = ( MethodInfo ) info.GetValue("method", _minfoType);
			}

		[SecurityPermission(SecurityAction.LinkDemand,
			Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData( SerializationInfo info, StreamingContext context )
			{
			if( info == null )
				{
				throw new ArgumentNullException("info");
				}

			info.AddValue("fix", fixArgs.Length);

			if( varArgs != null )
				info.AddValue("var", varArgs.Length);

			info.AddValue("method", method, _minfoType);
			}

		#endregion
		}
	}
