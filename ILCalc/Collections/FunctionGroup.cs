using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

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
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="FunctionGroup"/> class that is empty.</summary>
		/// <overloads>Initializes a new instance
		/// of the <see cref="FunctionGroup"/> class.</overloads>
		public FunctionGroup()
		{
			this.funcList = new List<FunctionItem>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// class that has one <paramref name="function"/> item inside.</summary>
		/// <param name="function">The <see cref="FunctionItem"/> item to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		public FunctionGroup(FunctionItem function)
		{
			this.funcList = new List<FunctionItem>(1) { function };
			this.paramsFuncsCount = function.HasParamArray ? 1 : 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// class taking <see cref="FunctionItem"/> items from the other
		/// <see cref="FunctionGroup"/>.</summary>
		/// <param name="other">
		/// Other instance of <see cref="FunctionGroup"/></param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="other"/> is null.</exception>
		public FunctionGroup(FunctionGroup other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			this.funcList = new List<FunctionItem>(other.funcList);
			this.paramsFuncsCount = other.paramsFuncsCount;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// class by taking <see cref="FunctionItem"/> items from the
		/// <paramref name="functions"/> enumerable.</summary>
		/// <param name="functions">
		/// Enumerable of <see cref="FunctionItem"/> items.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="functions"/> is null.</exception>
		public FunctionGroup(IEnumerable<FunctionItem> functions)
		{
			if (functions == null)
				throw new ArgumentNullException("functions");

			this.funcList = new List<FunctionItem>();

			foreach (FunctionItem func in functions)
			{
				this.Append(func);
			}
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets the count of <see cref="FunctionItem">functions</see>
		/// that this group represents.</summary>
		[DebuggerBrowsable(State.Never)]
		public int Count
		{
			[DebuggerHidden]
			get { return this.funcList.Count; }
		}

		[DebuggerBrowsable(State.Never)]
		internal bool HasParamsFunctions
		{
			[DebuggerHidden]
			get { return this.paramsFuncsCount > 0; }
		}

		/// <summary>
		/// Gets the <see cref="FunctionItem"/> at the specified index.</summary>
		/// <param name="index">The index of the <see cref="FunctionItem"/> to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.
		/// <br/>-or-<br/>index is equal to or greater than <see cref="Count"/></exception>
		/// <returns>The <see cref="FunctionItem"/> at the specified index.</returns>
		public FunctionItem this[int index]
		{
			[DebuggerHidden]
			get { return this.funcList[index]; }
		}

		#endregion
		#region Methods

		/// <summary>
		/// Adds the <see cref="FunctionItem"/> to the <see cref="FunctionGroup"/>.</summary>
		/// <overloads>Adds the function to the <see cref="FunctionGroup"/>.</overloads>
		/// <param name="function"><see cref="FunctionItem"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(FunctionItem function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			return this.InternalAppend(function);
		}

		/// <summary>
		/// Adds the method reflection to the <see cref="FunctionGroup"/>.</summary>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method
		/// to be added to the <see cref="FunctionDictionary"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			return this.InternalAppend(
				FunctionFactory.CreateInstance(method, true));
		}

		/// <summary>
		/// Adds the method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// in the <see cref="FunctionGroup"/>.</summary>
		/// <param name="methodName">Type's method name to be imported.</param>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.<br/>-or-<br/>
		/// <paramref name="methodName"/>is null.</exception>
		/// <exception cref="ArgumentException">
		/// Method with <paramref name="methodName"/> is not founded.
		/// <br/>-or-<br/>Founded method is not valid to be added
		/// into this <see cref="FunctionGroup"/>.</exception>
		/// <exception cref="System.Reflection.AmbiguousMatchException">
		/// If <paramref name="type"/> contains more than one methods
		/// matching the specified <paramref name="methodName"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(string methodName, Type type)
		{
			return this.InternalAppend(
				FunctionFactory.GetHelper(type, methodName, -1));
		}

		/// <summary>
		/// Adds the method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// and arguments count to the <see cref="FunctionGroup"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <param name="methodName">Type's method name to be imported.</param>
		/// <param name="parametersCount">Method parameters count.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.<br/>-or-<br/>
		/// <paramref name="methodName"/>is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="parametersCount"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">
		/// Method with <paramref name="methodName"/> is not founded.
		/// <br/>-or-<br/>Founded method is not valid to be added
		/// to the <see cref="FunctionGroup"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(Type type, string methodName, int parametersCount)
		{
			if (parametersCount < 0)
				throw new ArgumentOutOfRangeException("parametersCount");

			return this.InternalAppend(
				FunctionFactory.GetHelper(type, methodName, parametersCount));
		}

#if !CF2

		/// <summary>
		/// Adds the <see cref="EvalFunc0"/> delegate to the <see cref="FunctionGroup"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc0"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionGroup"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(EvalFunc0 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			return this.InternalAppend(new FunctionItem(target.Method, 0, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc1"/> delegate to the <see cref="FunctionGroup"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc1"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionGroup"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(EvalFunc1 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			return this.InternalAppend(new FunctionItem(target.Method, 1, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc2"/> delegate to the <see cref="FunctionGroup"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc2"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionGroup"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(EvalFunc2 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			return this.InternalAppend(new FunctionItem(target.Method, 2, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFuncN"/> delegate to the <see cref="FunctionGroup"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFuncN"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionGroup"/>.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append(EvalFuncN target)
		{
			FunctionFactory.CheckDelegate(target, true);
			return this.InternalAppend(new FunctionItem(target.Method, 0, true));
		}

#endif

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

				if (func.ArgsCount == argsCount &&
					func.HasParamArray == hasParamArray)
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
		/// Determines whether a <see cref="FunctionItem"/>
		/// is contains in the <see cref="FunctionGroup"/>.</summary>
		/// <overloads>Determines whether a specified <see cref="FunctionItem"/>
		/// is contains in the <see cref="FunctionGroup"/>.</overloads>
		/// <param name="item"><see cref="FunctionItem"/>
		/// to locate in <see cref="FunctionGroup"/>.</param>
		/// <returns><b>true</b> if function is found in the group;
		/// otherwise, <b>false</b>.</returns>
		public bool Contains(FunctionItem item)
		{
			return this.funcList.Contains(item);
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

		internal string MakeMethodsArgsList()
		{
			if (this.funcList.Count == 0)
			{
				return string.Empty;
			}
			
			if (this.funcList.Count == 1)
			{
				return this.funcList[0].ArgsString;
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
					buf.Append(' ');
					buf.Append(Resource.sAnd);
					buf.Append(' ');
				}
				else
				{
					buf.Append(", ");
				}

				buf.Append(func.ArgsString);
				}

			return buf.ToString();
		}

		internal FunctionItem GetOverload(int argsCount)
		{
			if (this.HasParamsFunctions)
			{
				return this.GetParamsOverload(argsCount);
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

		internal bool InternalAppend(FunctionItem function)
		{
			foreach (FunctionItem func in this.funcList)
			{
				if (func.ArgsCount == function.ArgsCount
				    && func.HasParamArray == function.HasParamArray)
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

		private static int ArgsCountComparator(FunctionItem a, FunctionItem b)
		{
			if (a.ArgsCount == b.ArgsCount)
			{
				return a.HasParamArray == b.HasParamArray ? 0 :
					   a.HasParamArray ? 1 : -1;
			}

			return (a.ArgsCount < b.ArgsCount) ? -1 : 1;
		}

		private bool InternalAppend(MethodInfo method)
		{
			return this.InternalAppend(
				FunctionFactory.CreateInstance(method, true));
		}

		private FunctionItem GetParamsOverload(int argsCount)
		{
			int fixCount = -1;
			FunctionItem best = null;

			foreach (FunctionItem func in this.funcList)
			{
				if (func.HasParamArray)
				{
					if (func.ArgsCount <= argsCount
					 && func.ArgsCount > fixCount)
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