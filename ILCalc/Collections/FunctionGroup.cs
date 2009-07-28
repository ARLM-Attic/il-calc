using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// TODO: fix DebugView
// TODO: ICollection?

namespace ILCalc
{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Represents the function overload group that contains
	/// <see cref="FunctionItem"/> items with different values of
	/// <see cref="FunctionItem.ArgsCount"/> and
	/// <see cref="FunctionItem.HasParamArray"/> properties.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>
	[DebuggerDisplay("{Count} functions")]
	[Serializable]

	public sealed class FunctionGroup : IEnumerable<FunctionItem>
	{
		#region Fields

		[DebuggerBrowsable(State.RootHidden)]
		private readonly List<FunctionItem> funcList;

		[DebuggerBrowsable(State.Never)]
		private int paramsFuncsCount;

		#endregion
		#region Constructors

		internal FunctionGroup(FunctionItem function)
		{
			Debug.Assert(function != null);

			this.funcList = new List<FunctionItem>(1) { function };
			this.paramsFuncsCount = function.HasParamArray ? 1 : 0;
		}

		// For clone
		internal FunctionGroup(FunctionGroup other)
		{
			Debug.Assert(other != null);

			this.funcList = new List<FunctionItem>(other.funcList);
			this.paramsFuncsCount = other.paramsFuncsCount;
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets the count of <see cref="FunctionItem">functions</see>
		/// that this group represents.</summary>
		[DebuggerBrowsable(State.Never)]
		public int Count
		{
			get { return this.funcList.Count; }
		}

		/// <summary>
		/// Gets the <see cref="FunctionItem"/> at the specified index.</summary>
		/// <param name="index">The index of the <see cref="FunctionItem"/> to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.
		/// <br/>-or-<br/>index is equal to or greater than <see cref="Count"/></exception>
		/// <returns>The <see cref="FunctionItem"/> at the specified index.</returns>
		public FunctionItem this[int index]
		{
			get { return this.funcList[index]; }
		}

		#endregion
		#region Methods

		// TODO: maybe Add & ICollection<T>?

		/// <summary>
		/// Removes the <see cref="FunctionItem"/> with the specified
		/// <paramref name="argsCount"/> and <paramref name="hasParamArray"/>
		/// values from the <see cref="FunctionGroup"/>.</summary>
		/// <param name="argsCount"><see cref="FunctionItem"/> arguments count.</param>
		/// <param name="hasParamArray">Indicates that <see cref="FunctionItem"/>
		/// has an parameters array.</param>
		/// <returns><b>true</b> if specified <see cref="FunctionItem"/>
		/// is founded in the group and was removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove(int argsCount, bool hasParamArray)
		{
			for (int i = 0; i < this.Count; i++)
			{
				FunctionItem func = this.funcList[i];

				if (func.ArgsCount == argsCount
				 && func.HasParamArray == hasParamArray)
				{
					if (func.HasParamArray)
					{
						this.paramsFuncsCount--;
					}

					this.funcList.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the <see cref="FunctionItem"/> at the specified
		/// index of the <see cref="FunctionGroup"/>.</summary>
		/// <param name="index">The zero-based index
		/// of the <see cref="FunctionItem"/> to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/>
		/// is less than 0, equal to or greater than Count.</exception>
		public void RemoveAt(int index)
		{
			if (this.funcList[index].HasParamArray)
			{
				this.paramsFuncsCount--;
			}

			this.funcList.RemoveAt(index);
		}

		/// <summary>
		/// Removes all <see cref="FunctionItem">functions</see>
		/// from the <see cref="FunctionGroup"/>.</summary>
		public void Clear()
		{
			this.paramsFuncsCount = 0;
			this.funcList.Clear();
		}

		/// <summary>
		/// Determines whether a <see cref="FunctionItem"/> with the specified
		/// <paramref name="argsCount"/> and <paramref name="hasParamArray"/>
		/// values is contains in the <see cref="FunctionGroup"/>.</summary>
		/// <param name="argsCount"><see cref="FunctionItem"/> arguments count.</param>
		/// <param name="hasParamArray">Indicates that <see cref="FunctionItem"/>
		/// has an parameters array.</param>
		/// <returns><b>true</b> if function is found in the group;
		/// otherwise, <b>false</b>.</returns>
		public bool Contains(int argsCount, bool hasParamArray)
		{
			foreach (FunctionItem func in this.funcList)
			{
				if (func.ArgsCount == argsCount
				 && func.HasParamArray == hasParamArray)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through
		/// the <see cref="FunctionItem">functions</see>
		/// in <see cref="FunctionGroup"/>.</summary>
		/// <returns>An enumerator for the all <see cref="FunctionItem">
		/// functions</see> in <see cref="FunctionGroup"/>.</returns>
		public IEnumerator<FunctionItem> GetEnumerator()
		{
			return this.funcList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.funcList.GetEnumerator();
		}

		#endregion
		#region Internals

		internal bool Append(FunctionItem function)
		{
			Debug.Assert(function != null);

			foreach (FunctionItem f in this.funcList)
			{
				if (function.ArgsCount == f.ArgsCount
				 && function.HasParamArray == f.HasParamArray)
				{
					return false;
				}
			}

			if (function.HasParamArray)
			{
				this.paramsFuncsCount++;
			}

			this.funcList.Add(function);
			return true;
		}

		internal string MakeMethodsArgsList()
		{
			switch (this.funcList.Count)
			{
				case 0: return string.Empty;
				case 1: return this.funcList[0].ArgsString;
			}

			var buf = new StringBuilder();
			this.funcList.Sort(ArgsCountComparator);

			// output first:
			buf.Append(this.funcList[0].ArgsString);

			// and others:
			for (int i = 1, last = this.Count - 1; i < this.Count; i++)
			{
				FunctionItem func = this.funcList[i];

				if (i == last)
				{
					buf.Append(' ').Append(Resource.sAnd).Append(' ');
				}
				else buf.Append(", ");

				buf.Append(func.ArgsString);
			}

			return buf.ToString();
		}

		internal FunctionItem GetOverload(int argsCount)
		{
			Debug.Assert(argsCount >= 0);

			if (this.paramsFuncsCount > 0)
			{
				return GetParamsOverload(argsCount);
			}

			foreach (FunctionItem func in this.funcList)
			{
				if (func.ArgsCount == argsCount)
				{
					return func;
				}
			}

			return null;
		}

		private static int ArgsCountComparator(FunctionItem a, FunctionItem b)
		{
			if (a.ArgsCount == b.ArgsCount)
			{
				if (a.HasParamArray == b.HasParamArray) return 0;
				return a.HasParamArray ? 1 : -1;
			}

			return a.ArgsCount < b.ArgsCount ? -1 : 1;
		}

		private FunctionItem GetParamsOverload(int argsCount)
		{
			Debug.Assert(argsCount >= 0);

			FunctionItem best = null;
			int fixCount = -1;

			foreach (var func in this.funcList)
			{
				if (func.HasParamArray)
				{
					if (func.ArgsCount <= argsCount
					 && func.ArgsCount  >  fixCount)
					{
						best = func;
						fixCount = func.ArgsCount;
					}
				}
				else if (func.ArgsCount == argsCount)
				{
					return func;
				}
			}

			return best;
		}

		#endregion
	}
}