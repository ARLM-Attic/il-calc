using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
{
	internal static class TypeHelper
	{
		#region Fields

		public static readonly Type ValueType = typeof(Double);
		public static readonly Type ArrayType = typeof(Double[]);

		private static readonly List<Type> TypesList = new List<Type> { ArrayType }; 

		#endregion
		#region Methods

		public static Type GetArrayType(int rank)
		{
			Debug.Assert(rank > 0);

			if (rank >= TypesList.Count)
			lock (((ICollection) TypesList).SyncRoot)
			{
				int count = rank - TypesList.Count;
				Type last = TypesList[TypesList.Count - 1];
#if CF
				var buf = new System.Text.StringBuilder(last.FullName);
				for (int i = 0; i < count; i++)
				{
					buf.Append("[]");
					TypesList.Add(Type.GetType(buf.ToString()));
				}
#else
				for (int i = 0; i < count; i++)
				{
					last = last.MakeArrayType();
					TypesList.Add(last);
				}
#endif
			}

			return TypesList[rank - 1];
		}

		#endregion
	}
}