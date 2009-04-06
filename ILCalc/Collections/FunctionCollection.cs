using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Manages the pairs list of function names and attached method reflections
	/// available to an expression. Function names are unique, but they can
	/// be overloaded by arguments count and contains methods with <c>params</c>
	/// arguments, that can be overloaded too.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>
	
	[DebuggerDisplay( "Count = {_funcs.Count}" )]
	[DebuggerTypeProxy( typeof( FunctionDebugView ) )]
	[Serializable]
	
	public sealed class FunctionCollection : IEnumerable<string>
		{
		#region Fields

		[DebuggerBrowsable( State.Never )]
		private readonly List<string> _names;
		[DebuggerBrowsable( State.Never )]
		private readonly List<MethodGroup> _funcs;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionCollection"/> class
		/// that is empty and has the default initial capacity.
		/// </summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="FunctionCollection"/> class.
		/// </overloads>
		[DebuggerHidden]
		public FunctionCollection( )
			{
			_names = new List<string>( );
			_funcs = new List<MethodGroup>( );
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionCollection"/>
		/// class from the other <see cref="FunctionCollection"/> instance.
		/// </summary>
		/// <param name="list"><see cref="FunctionCollection"/> instance.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		[DebuggerHidden]
		public FunctionCollection( FunctionCollection list )
			{
			if( list == null )
				throw new ArgumentNullException("list");

			_names = new List<string>(list._names);
			_funcs = new List<MethodGroup>(list.Count);

			foreach( MethodGroup group in list._funcs )
				{
				_funcs.Add(new MethodGroup(group));
				}
			}

		#endregion
		#region Members

		/// <summary>
		/// Adds the method reflection to the end of the <see cref="FunctionCollection"/>
		/// with the name, taken from real method name.
		/// </summary>
		/// <overloads>Adds the method reflection to the end
		/// of the <see cref="FunctionCollection"/>.</overloads>
		/// <param name="func">MethodInfo to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="func"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="func"/> is not valid method to be added
		/// into this <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// method with same name and same arguments count 
		/// already exist in the list (overload impossible).
		/// </exception>
		[DebuggerHidden]
		public void Add( MethodInfo func )
			{
			if( func == null )
				throw new ArgumentNullException("func");

			Validator.ImportableMethod(func.Name, func);

			InternalAdd(func.Name, func);
			}

		/// <summary>
		/// Adds the method reflection to the end of the <see cref="FunctionCollection"/>
		/// with the specified name.
		/// </summary>
		/// <param name="name">Function name.</param>
		/// <param name="func">MethodInfo to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="func"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="func"/> is not valid
		/// method to be added into this <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <paramref name="name"/> is not valid identifier name.<br/>-or-<br/>
		/// method with same name and same arguments count already
		/// exist in the list (overload impossible).
		/// </exception>
		[DebuggerHidden]
		public void Add( string name, MethodInfo func )
			{
			if( func == null )
				throw new ArgumentNullException("func");

			Validator.IdentifierName(name);
			Validator.ImportableMethod(name, func);

			InternalAdd(name, func);
			}

		/// <summary>
		/// Adds the member method of specified type by name to the end of the
		/// <see cref="FunctionCollection"/> with the name, taken from real method name.
		/// </summary>
		/// <param name="type">Type object.</param>
		/// <param name="methodName">Member method name to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// member method with <paramref name="methodName"/> is not founded.<br/>-or-<br/>
		/// founded method is not valid to be added into this <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// method with same name and same arguments count already exist
		/// in the list (overload impossible).
		/// </exception>
		/// <exception cref="System.Reflection.AmbiguousMatchException">
		/// If <paramref name="type"/> contains more than one member method matching
		/// the specified <paramref name="methodName"/>.
		/// </exception>
		[DebuggerHidden]
		public void Add( Type type, string methodName )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			const BindingFlags flags = 
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			MethodInfo func = type.GetMethod(methodName, flags);

			if( func == null )
				{
				throw new ArgumentException(
					string.Format(Resources.errMethodNotFounded, methodName)
					);
				}

			Validator.ImportableMethod(func.Name, func);

			InternalAdd(func.Name, func);
			}

		private void InternalAdd( string name, MethodInfo func )
			{
			int id = _names.IndexOf(name);
			if( id != -1 )
				{
				if( !_funcs[id].Append(func) )
					{
					throw Validator.FuncError(name, func,
						Resources.errMethodSameParams
						);
					}
				}
			else
				{
				_names.Add(name);
				_funcs.Add(new MethodGroup(func));
				}
			}

		/// <summary>
		/// Gets the number of names actually contained 
		/// in the <see cref="FunctionCollection"/>.
		/// </summary>
		[DebuggerBrowsable( State.Never )]
		public int Count
			{
			[DebuggerHidden]
			get { return _names.Count; }
			}

		/// <summary>
		/// Returns a read-only <see cref="IList{T}"/> wrapper for names
		/// list of the current <see cref="FunctionCollection"/>.
		/// </summary>
		[DebuggerBrowsable( State.Never )]
		public ICollection<string> Names
			{
			[DebuggerHidden]
			get { return _names.AsReadOnly( ); }
			}

		/// <summary>
		/// Removes the function specified by name from the <see cref="FunctionCollection"/>.
		/// </summary>
		/// <overloads>
		/// Removes the function from the <see cref="FunctionCollection"/>.
		/// </overloads>
		/// <param name="name">The function name to be removed.</param>
		/// <returns>
		/// <b>true</b> if function is successfully removed; otherwise, <b>false</b>.
		/// </returns>
		public bool Remove( string name )
			{
			int id = _names.IndexOf(name);
			if( id >= 0 )
				{
				_names.RemoveAt(id);
				_funcs.RemoveAt(id);
				return true;
				}
			return false;
			}

		/// <summary>
		/// Removes the function overload specified by name, arguments count
		/// and params arguments usage from the <see cref="FunctionCollection"/>.
		/// </summary>
		/// <param name="name">The function name.</param>
		/// <param name="argsCount">Overload arguments count.</param>
		/// <param name="isParams">Overload use params.</param>
		/// <returns>
		/// <b>true</b> if function overload is successfully removed; otherwise, <b>false</b>.
		/// </returns>
		public bool Remove( string name, int argsCount, bool isParams )
			{
			int id = _names.IndexOf(name);
			if( id >= 0 && _funcs[id].Remove(argsCount, isParams) )
				{
				if( _funcs[id].Count == 0 )
					{
					_names.RemoveAt(id);
					_funcs.RemoveAt(id);
					}
				return true;
				}
			return false;
			}

		/// <summary>
		/// Removes all names and methods from the <see cref="FunctionCollection"/>.
		/// </summary>
		[DebuggerHidden]
		public void Clear( )
			{
			_names.Clear( );
			_funcs.Clear( );
			}

		/// <summary>
		/// Determines whether an name is in the <see cref="FunctionCollection"/>.
		/// </summary>
		/// <param name="name">Function name to locate in <see cref="FunctionCollection"/>.</param>
		/// <returns>
		/// <b>true</b> if name is found in the list; otherwise, <b>false</b>.
		/// </returns>
		[DebuggerHidden]
		public bool Contains( string name )
			{
			return _names.Contains(name);
			}

		//TODO: CF hasn't some functions

		/// <summary>
		/// Imports standart builtin functions from <c>System.Math</c>
		/// into this <see cref="FunctionCollection"/>.
		/// </summary>
		/// <remarks>
		/// Currently this method imports methods from <see cref="Math"/> class:<br/>
		/// Abs, Sin, Cos, Tan, Sinh, Cosh, Tanh, Acos, Asin, Atan, Atan2,
		/// Ceil, Floor, Round, Trunc (not in CF/Silverlight), Log, Log10,
		/// Min, Max, Exp, Pow and Sqrt.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the list.
		/// </exception>
		public void ImportBuiltin( )
			{
			var math = typeof( Math );
			var type = typeof( double );
			var oneArg = new[] {type};
			var twoArg = new[] {type, type};

			InternalAdd("Abs", math.GetMethod("Abs", oneArg));

			InternalAdd("Sin", math.GetMethod("Sin"));
			InternalAdd("Cos", math.GetMethod("Cos"));
			InternalAdd("Tan", math.GetMethod("Tan"));

			InternalAdd("Sinh", math.GetMethod("Sinh"));
			InternalAdd("Cosh", math.GetMethod("Cosh"));
			InternalAdd("Tanh", math.GetMethod("Tanh"));

			InternalAdd("Acos", math.GetMethod("Acos"));
			InternalAdd("Asin", math.GetMethod("Asin"));
			InternalAdd("Atan", math.GetMethod("Atan"));
			InternalAdd("Atan2", math.GetMethod("Atan2"));

			InternalAdd("Ceil", math.GetMethod("Ceiling", oneArg));
			InternalAdd("Floor", math.GetMethod("Floor", oneArg));
			InternalAdd("Round", math.GetMethod("Round", oneArg));

#if !SILVERLIGHT && !CF
			InternalAdd("Trunc", math.GetMethod("Truncate", oneArg));
#endif

			InternalAdd("Log", math.GetMethod("Log", oneArg));
#if !CF
			InternalAdd("Log", math.GetMethod("Log", twoArg));
#endif
			InternalAdd("Log10", math.GetMethod("Log10"));

			InternalAdd("Min", math.GetMethod("Min", twoArg));
			InternalAdd("Max", math.GetMethod("Max", twoArg));

			InternalAdd("Exp", math.GetMethod("Exp"));
			InternalAdd("Pow", math.GetMethod("Pow"));
			InternalAdd("Sqrt", math.GetMethod("Sqrt"));
			}

		/// <summary>
		/// Imports all public static methods of the specified type
		/// that is able to add into this <see cref="FunctionCollection"/>.
		/// </summary>
		/// <overloads>
		/// Imports static methods of the specified type(s)
		/// that is able to add into this <see cref="FunctionCollection"/>.
		/// </overloads>
		/// <param name="type">Type opbject.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and same 
		/// arguments count already exist in the list (overload impossible).
		/// </exception>
		public void Import( Type type )
			{
			var flags = BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.FlattenHierarchy;

			InternalImport(type, flags);
			}

		/// <summary>
		/// Imports all static methods of the specified type
		/// that is able to add into this <see cref="FunctionCollection"/>.
		/// </summary>
		/// <param name="type">Type opbject.</param>
		/// <param name="nonPublic">Include non public member methods in search.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and same 
		/// arguments count already exist in the list (overload impossible).
		/// </exception>
		public void Import( Type type, bool nonPublic )
			{
			var flags = BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.FlattenHierarchy;

			if( nonPublic ) flags |= BindingFlags.NonPublic;

			InternalImport(type, flags);
			}

		/// <summary>
		/// Imports all static methods of the specified types
		/// thats is able to add into this <see cref="FunctionCollection"/>.
		/// </summary>
		/// <param name="types">Array of <see cref="Type"/> opbjects.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="types"/> is null.</exception><br>-or-</br>
		/// Some Type of <paramref name="types"/> is null.
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and same 
		/// arguments count already exist in the list (overload impossible).
		/// </exception>
		public void Import( params Type[] types )
			{
			if( types == null )
				throw new ArgumentNullException("types");

			var flags = BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.FlattenHierarchy;

			foreach( Type type in types )
				{
				InternalImport(type, flags);
				}
			}

		private void InternalImport( Type type, BindingFlags flags )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			foreach( MethodInfo method in type.GetMethods(flags) )
				{
				if( Validator.IsImportable(method) )
					InternalAdd(method.Name, method);
				}
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through
		/// the names in <see cref="FunctionCollection"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="List{T}.Enumerator"/> for the names in FuctionCollection.
		/// </returns>
		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator( )
			{
			return _names.GetEnumerator( );
			}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator( )
			{
			return _names.GetEnumerator( );
			}

		#endregion
		#region Internal

		internal MethodGroup this[ int id ]
			{
			[DebuggerHidden]
			get { return _funcs[id]; }
			}

		#endregion
		}

	#region Debug View

	sealed class FunctionDebugView
		{
		[DebuggerDisplay( "{name}" )]
		private struct ViewItem
			{
			// ReSharper disable UnaccessedField.Local
			[DebuggerBrowsable( State.Never )]
			public string name;
			[DebuggerBrowsable( State.RootHidden )]
			public MethodGroup methods;
			// ReSharper restore UnaccessedField.Local
			}

		[DebuggerBrowsable( State.RootHidden )]
		private readonly ViewItem[] items;

		public FunctionDebugView( FunctionCollection list )
			{
			items = new ViewItem[list.Count];
			int i = 0;
			foreach( string c in list )
				{
				items[i].name = c;
				items[i].methods = list[i];
				i++;
				}
			}
		}

	#endregion
	}