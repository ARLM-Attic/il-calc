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
	/// <see cref="Function"/> items with different values of
	/// <see cref="Function.ArgsCount"/> and
	/// <see cref="Function.HasParamArray"/> properties.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>

	[DebuggerDisplay("{Count} functions")]
	[Serializable]

	public sealed class FunctionGroup : IEnumerable<Function>
		{
		#region Fields

		[DebuggerBrowsable(State.RootHidden)]
		private readonly List<Function> funcList;
		[DebuggerBrowsable(State.Never)]
		private int paramsFuncsCount;

		#endregion
		#region Properties

		/// <summary>
		/// Gets the count of <see cref="Function">functions</see>
		/// that this group represents.</summary>
		[DebuggerBrowsable(State.Never)]
		public int Count
			{
			[DebuggerHidden] get { return funcList.Count; }
			}

		/// <summary>
		/// Gets the <see cref="Function"/> at the specified index.</summary>
		/// <param name="index">The index of the <see cref="Function"/> to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.
		/// <br/>-or-<br/>index is equal to or greater than <see cref="Count"/></exception>
		/// <returns>The <see cref="Function"/> at the specified index.</returns>
		public Function this[ int index ]
			{
			[DebuggerHidden] get { return funcList[index]; }
			}

		[DebuggerBrowsable(State.Never)]
		internal bool HasParamsMethods
			{
			get { return paramsFuncsCount > 0; }
			}

		#endregion
		#region Methods

		private bool InternalAppend( MethodInfo method )
			{
			return InternalAppend(
				FunctionFactory.CreateInstance(method, true)
				);
			}

		internal bool InternalAppend( Function function )
			{
			foreach( Function f in funcList )
				{
				if( f.ArgsCount == function.ArgsCount
				 && f.HasParamArray == function.HasParamArray )
					{
					return false;
					}
				}

			if( function.HasParamArray ) 
				paramsFuncsCount++;

			funcList.Add(function);
			return true;
			}

		/// <summary>
		/// Adds the <see cref="Function"/> to the <see cref="FunctionGroup"/>.</summary>
		/// <overloads>Adds the function to the <see cref="FunctionGroup"/>.</overloads>
		/// <param name="function"><see cref="Function"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append( Function function )
			{
			if( function == null )
				throw new ArgumentNullException("function");

			return InternalAppend(function);
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
		public bool Append( MethodInfo method )
			{
			if( method == null )
				throw new ArgumentNullException("method");

			return InternalAppend(
				FunctionFactory.CreateInstance(method, true));
			}

		/// <summary>
		/// Adds the method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// in the <see cref="FunctionGroup"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <param name="methodName">Type's method name to be imported.</param>
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
		public bool Append( string methodName, Type type )
			{
			return InternalAppend(
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
		/// <see cref="Function"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		/// <returns><b>true</b>, if group successfully overloaded;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Append( Type type, string methodName, int parametersCount )
			{
			if( parametersCount < 0 )
				throw new ArgumentOutOfRangeException("parametersCount");

			return InternalAppend(
				FunctionFactory.GetHelper(type, methodName, parametersCount)
				);
			}

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
		public bool Append( EvalFunc0 target )
			{
			FunctionFactory.CheckDelegate(target, true);
			return InternalAppend(new Function(target.Method, 0, false));
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
		public bool Append( EvalFunc1 target )
			{
			FunctionFactory.CheckDelegate(target, true);
			return InternalAppend(new Function(target.Method, 1, false));
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
		public bool Append( EvalFunc2 target )
			{
			FunctionFactory.CheckDelegate(target, true);
			return InternalAppend(new Function(target.Method, 2, false));
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
		public bool Append( EvalFuncN target )
			{
			FunctionFactory.CheckDelegate(target, true);
			return InternalAppend(new Function(target.Method, 0, true));
			}

		/// <summary>
		/// Removes the <see cref="Function"/> with the specified
		/// <paramref name="argsCount"/> and <paramref name="hasParams"/>
		/// values from the <see cref="FunctionGroup"/>.</summary>
		/// <param name="argsCount"><see cref="Function"/> arguments count.</param>
		/// <param name="hasParams">Indicates that <see cref="Function"/>
		/// has an parameters array.</param>
		/// <returns><b>true</b> if specified <see cref="Function"/>
		/// is founded in the group and was removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove( int argsCount, bool hasParams )
			{
			for( int i = 0; i < funcList.Count; i++ )
				{
				Function f = funcList[i];

				if( f.ArgsCount     == argsCount
				 && f.HasParamArray == hasParams )
					{
					if( f.HasParamArray )
						paramsFuncsCount--;

					funcList.RemoveAt(i);
					return true;
					}
				}

			return false;
			}

		/// <summary>
		/// Removes the <see cref="Function"/> at the specified
		/// index of the <see cref="FunctionGroup"/>.</summary>
		/// <param name="index">The zero-based index
		/// of the <see cref="Function"/> to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/>
		/// is less than 0, equal to or greater than Count.</exception>
		public void RemoveAt( int index )
			{
			if( funcList[index].HasParamArray )
				paramsFuncsCount--;

			funcList.RemoveAt(index);
			}

		/// <summary>
		/// Removes all <see cref="Function">functions</see>
		/// from the <see cref="FunctionGroup"/>.</summary>
		public void Clear( )
			{
			paramsFuncsCount = 0;
			funcList.Clear( );
			}

		/// <summary>
		/// Determines whether a <see cref="Function"/>
		/// is contains in the <see cref="FunctionGroup"/>.</summary>
		/// <overloads>
		/// Determines whether a specified <see cref="Function"/>
		/// is contains in the <see cref="FunctionGroup"/>.</overloads>
		/// <param name="item"><see cref="Function"/>
		/// to locate in <see cref="FunctionGroup"/>.</param>
		/// <returns><b>true</b> if function is found in the group;
		/// otherwise, <b>false</b>.</returns>
		public bool Contains( Function item )
			{
			return funcList.Contains(item);
			}

		/// <summary>
		/// Determines whether a <see cref="Function"/> with the specified
		/// <paramref name="argsCount"/> and <paramref name="hasParams"/>
		/// values is contains in the <see cref="FunctionGroup"/>.</summary>
		/// <param name="argsCount"><see cref="Function"/> arguments count.</param>
		/// <param name="hasParams">Indicates that <see cref="Function"/>
		/// has an parameters array.</param>
		/// <returns><b>true</b> if function is found in the group;
		/// otherwise, <b>false</b>.</returns>
		public bool Contains( int argsCount, bool hasParams )
			{
			foreach( Function f in funcList )
				{
				if( f.ArgsCount     == argsCount
				 && f.HasParamArray == hasParams )
					{
					return true;
					}
				}

			return false;
			}

		#endregion
		#region Internals

		internal string MakeMethodsArgsList( )
			{
			if( funcList.Count == 0 ) return string.Empty;
			if( funcList.Count == 1 )
				{
				return funcList[0].ArgsString;
				}

			var buf = new StringBuilder( );
			funcList.Sort(ArgsCountComparator);

			// output first:
			buf.Append(funcList[0].ArgsString);

			// and others:
			for( int i = 1, last = funcList.Count - 1;
				 i < funcList.Count; i++ )
				{
				Function func = funcList[i];

				if( i == last )
					{
					buf.Append(' ');
					buf.Append(Resources.sAnd);
					buf.Append(' ');
					}
				else buf.Append(", ");

				buf.Append(func.ArgsString);
				}

			return buf.ToString( );
			}

		internal Function GetOverload( int argsCount )
			{
			if( HasParamsMethods )
				return GetParamsOverload(argsCount);

			foreach( Function f in funcList )
				{
				if( f.ArgsCount == argsCount ) return f;
				}

			return null;
			}

		private Function GetParamsOverload( int argsCount )
			{
			int fixCount = -1;
			Function best = null;

			foreach( Function f in funcList )
				{
				if( f.HasParamArray )
					{
					if( f.ArgsCount <= argsCount
					 && f.ArgsCount >  fixCount )
						{
						best = f;
						fixCount = f.ArgsCount;
						}
					}
				else if( f.ArgsCount == argsCount )
					{
					return f;
					}
				}

			return best;
			}

		internal static List<Function> GetOverloadsList(
				IEnumerable<FunctionGroup> groupList, int argsCount )
			{
			bool hasParams = false;
			int  fixCount = -1;

			var overloads = new List<Function>( );

			foreach( FunctionGroup group in groupList )
				{
				Function func = group.GetOverload(argsCount);

				if( func == null ) continue;
				if( func.ArgsCount > fixCount )
					{
					overloads.Clear( );
					overloads.Add(func);

					fixCount  = func.ArgsCount;
					hasParams = func.HasParamArray;
					}
				else if( func.ArgsCount == fixCount )
					{
					if( func.HasParamArray )
						{
						if( hasParams )
							overloads.Add(func);
						}
					else
						{
						if( hasParams )
							{
							hasParams = false;
							overloads.Clear( );
							}

						overloads.Add(func);
						}
					}
				}

			return overloads;
			}

		private static int ArgsCountComparator( Function a, Function b )
			{
			if( a.ArgsCount == b.ArgsCount )
				{
				return (a.HasParamArray == b.HasParamArray) ? 0 :
					   (a.HasParamArray) ? 1 : -1;
				}
			return (a.ArgsCount < b.ArgsCount) ? -1 : 1;
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through
		/// the <see cref="Function">functions</see>
		/// in <see cref="FunctionGroup"/>.</summary>
		/// <returns>An enumerator for the all <see cref="Function">
		/// functions</see> in <see cref="FunctionGroup"/>.</returns>
		public IEnumerator<Function> GetEnumerator( )
			{
			return funcList.GetEnumerator( );
			}

		IEnumerator IEnumerable.GetEnumerator( )
			{
			return funcList.GetEnumerator( );
			}

		#endregion
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="FunctionGroup"/> class that is empty.</summary>
		/// <overloads>Initializes a new instance
		/// of the <see cref="FunctionGroup"/> class.</overloads>
		public FunctionGroup( )
			{
			funcList = new List<Function>( );
			paramsFuncsCount = 0;
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// class that has one <paramref name="function"/> item inside.</summary>
		/// <param name="function">The <see cref="Function"/> item to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		public FunctionGroup( Function function )
			{
			funcList = new List<Function> { function };
			paramsFuncsCount = function.HasParamArray? 1: 0;
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// taking <see cref="Function"/> items from the other
		/// <see cref="FunctionGroup"/>.</summary>
		/// <param name="other">
		/// Other instance of <see cref="FunctionGroup"/></param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="other"/> is null.</exception>
		public FunctionGroup( FunctionGroup other )
			{
			if( other == null )
				throw new ArgumentNullException("other");

			funcList = new List<Function>(other.funcList);
			paramsFuncsCount = other.paramsFuncsCount;
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionGroup"/>
		/// class by taking <see cref="Function"/> items from the
		/// <paramref name="functions"/> enumerable.</summary>
		/// <param name="functions">
		/// Enumerable of <see cref="Function"/> items.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="functions"/> is null.</exception>
		public FunctionGroup( IEnumerable<Function> functions )
			{
			if( functions == null )
				throw new ArgumentNullException("functions");

			funcList = new List<Function>( );
			paramsFuncsCount = 0;

			foreach( Function f in functions )
				{
				Append(f);
				}
			}

		#endregion
		}
	}
