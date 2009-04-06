using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
	{
	using ConstPair = KeyValuePair< string, double >;
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Manages the pairs list of unique names and values of constants
	/// available to an expression.<br/>
	/// This class cannot be inherited.</summary>
	/// <threadsafety instance="false"/>
	
	[DebuggerDisplay( "Count = {Count}" )]
	[DebuggerTypeProxy( typeof( ConstantsDebugView ) )]
	[Serializable]
	
	public sealed class ConstantCollection : IDictionary<string, double>
		{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly List<string> _names;

		[DebuggerBrowsable(State.Never)]
		private readonly List<double> _values;

		[DebuggerBrowsable(State.Never)]
		private const BindingFlags _flags = 
			BindingFlags.Static |
			BindingFlags.Public |
			BindingFlags.FlattenHierarchy;
		
		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantCollection"/> class
		/// that is empty and has the default initial capacity.
		/// </summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="ConstantCollection"/> class.
		/// </overloads>
		[DebuggerHidden]
		public ConstantCollection( )
			{
			_names  = new List<string>( );
			_values = new List<double>( );
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantCollection"/>
		/// class from the instance of <see cref="ICollection{T}"/> containing
		/// pairs of constant names and values.
		/// </summary>
		/// <param name="list"><see cref="ICollection"/> of the name/value pairs.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="list"/> is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="list"/> is already exist in the list.
		/// </exception>
		public ConstantCollection( ICollection<ConstPair> list )
			{
			if( list == null )
				throw new ArgumentNullException("list");

			_names  = new List<string>(list.Count);
			_values = new List<double>(list.Count);

			foreach( ConstPair item in list )
				{
				Add(item.Key, item.Value);
				}
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantCollection"/>
		/// class from the other <see cref="ConstantCollection"/> instance.
		/// </summary>
		/// <param name="list"><see cref="ConstantCollection"/> instance.</param>
		/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
		[DebuggerHidden]
		public ConstantCollection( ConstantCollection list )
			{
			if( list == null )
				throw new ArgumentNullException("list");

			_names  = new List<string>(list._names);
			_values = new List<double>(list._values);
			}

		#endregion
		#region Imports

		/// <summary>
		/// Imports standart builtin constants into 
		/// this <see cref="ConstantCollection"/>.</summary>
		/// <remarks>
		/// Currently this method imports Pi, E, Inf and NaN constants.</remarks>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the list.</exception>
		public void ImportBuiltin( )
			{
			Add("E", Math.E);
			Add("Pi", Math.PI);

			Add("NaN", Double.NaN);
			Add("Inf", Double.PositiveInfinity);
			}

		/// <overloads>
		/// Imports static fields of the specified type(s)
		/// into this <see cref="ConstantCollection"/>.
		/// </overloads>
		/// <summary>
		/// Imports all public static fields of the specified type
		/// into this <see cref="ConstantCollection"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the list.</exception>
		public void Import( Type type )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			InternalImport(type, _flags);
			}

		/// <summary>
		/// Imports all public static fields of the specified type
		/// into this <see cref="ConstantCollection"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <param name="nonPublic">
		/// Include non public member methods in search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the list.</exception>
		public void Import( Type type, bool nonPublic )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			var flags = _flags;
			
			if( nonPublic ) 
				flags |= BindingFlags.NonPublic;

			InternalImport(type, flags);
			}

		/// <summary>
		/// Imports all public static fields of the specified types
		/// into this <see cref="ConstantCollection"/>.</summary>
		/// <param name="types">
		/// Params array of <see cref="Type"/> objects.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="types"/> is null.<br>-or-</br>
		/// Some Type of <paramref name="types"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the list.</exception>
		public void Import( params Type[] types )
			{
			if( types == null )
				throw new ArgumentNullException("types");

			foreach( Type type in types )
				{
				InternalImport(type, _flags);
				}
			}

		private void InternalImport( Type type, BindingFlags flags )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			foreach( FieldInfo field in type.GetFields(flags) )
				{
				// look for "const double" fields
				if( field.IsLiteral &&
					field.FieldType == Validator.T_type )
					{
					var value = ( double ) field.GetValue(null);
					Add(field.Name, value);
					}
				}
			}

		#endregion
		#region IDictionary<>

		/// <summary>
		/// Adds constant to the end of the 
		/// <see cref="ConstantCollection"/>.</summary>
		/// <param name="key">Constant name.</param>
		/// <param name="value">Constant value.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="key"/> is not valid identifier name.<br/>-or-<br/>
		/// <paramref name="key"/> name is already exist in the list.</exception>
		public void Add( string key, double value )
			{
			Validator.IdentifierName(key);

			if( _names.Contains(key) )
				{
				throw new ArgumentException(
					string.Format(Resources.errConstantExist, key)
					);
				}

			_names.Add(key);
			_values.Add(value);
			}

		/// <summary>
		/// Determines whether the <see cref="ConstantCollection"/>
		/// contains the specified name.</summary>
		/// <param name="key">
		/// Constant name to locate in the <see cref="ConstantCollection"/>.</param>
		/// <returns>
		/// <b>true</b> if name is found in the list;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool ContainsKey( string key )
			{
			return _names.Contains(key);
			}

		/// <summary>
		/// Gets a collection containing the names of the
		/// <see cref="ConstantCollection"/>.</summary>
		[DebuggerBrowsable( State.Never )]
		public ICollection<string> Keys
			{
			[DebuggerHidden]
			get {
				return _names.AsReadOnly( );
				}
			}

		/// <summary>
		/// Removes the constant specified by name
		/// from the <see cref="ConstantCollection"/>.</summary>
		/// <param name="key">The function name to be removed.</param>
		/// <returns>
		/// <b>true</b> if constant is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Remove( string key )
			{
			return _names.Remove(key);
			}

		/// <summary>
		/// Gets the value of constant with the specified name.</summary>
		/// <param name="key">
		/// The name of the constant, which value to get.</param>
		/// <param name="value">
		/// When this method returns, contains the value of constant
		/// with the specified name, if the name is found; otherwise,
		/// the default value for the type of the value parameter.
		/// This parameter is passed uninitialized.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is null.</exception>
		/// <returns>
		/// <b>true</b> if the <see cref="ConstantCollection"/> contains an
		/// element with the specified name; otherwise, <b>false</b>.</returns>
		public bool TryGetValue( string key, out double value )
			{
			if( key == null )
				throw new ArgumentNullException("key");

			int index = _names.IndexOf(key);
			if( index >=0 )
				{
				value = _values[index];
				return true;
				}

			value = default(double);
			return false;
			}

		/// <summary>
		/// Gets a collection containing the values of the
		/// <see cref="ConstantCollection"/>.</summary>
		[DebuggerBrowsable( State.Never )]
		public ICollection<double> Values
			{
			[DebuggerHidden]
			get { return _values.AsReadOnly( ); }
			}

		/// <summary>Gets or sets the value associated
		/// with the specified constant name.</summary>
		/// <overloads>Gets or sets the value associated
		/// with the specified constant name or index.</overloads>
		/// <param name="key">
		/// The name of the constant, which value to get or set.</param>
		/// <exception cref="KeyNotFoundException">
		/// The property is retrieved and name does not exist in the collection.</exception>
		/// <exception cref="ArgumentException">
		/// The property is setted and <paramref name="key"/>
		/// is not valid identifier name.</exception>
		/// <exception cref="ArgumentNullException">
		/// The property is setted and<paramref name="key"/> is null.</exception>
		/// <returns>
		/// The value associated with the specified name. If the specified name is not found,
		/// a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation 
		/// creates a new element with the specified name.</returns>
		[DebuggerBrowsable( State.Never )]
		public double this[ string key ]
			{
			[DebuggerHidden]
			get
				{
				double value;
				if(TryGetValue(key, out value)) return value;

				throw new KeyNotFoundException(
					string.Format(Resources.errConstantNotExist, key)
					);
				}
			[DebuggerHidden]
			set
				{
				int index = _names.IndexOf(key);
				if( index >= 0 )
					{
					_values[index] = value;
					}
				else
					{
					_names.Add(key);
					_values.Add(value);
					}
				}
			}

		/// <summary>
		/// Gets or sets the constant value at the specified index.</summary>
		/// <param name="index">
		/// The name of the constant, which value to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>
		/// index is equal to or greater than <see cref="Count"/></exception>
		/// <returns>The constant value at the specified index.</returns>
		public double this[int index]
			{
			[DebuggerHidden] get { return _values[index]; }
			[DebuggerHidden] set { _values[index] = value; }
			}

		#endregion
		#region ICollection<>

		[DebuggerHidden]
		void ICollection<ConstPair>.Add( ConstPair item )
			{
			Add(item.Key, item.Value);
			}

		[DebuggerHidden]
		bool ICollection<ConstPair>.Contains( ConstPair item )
			{
			int index = _names.IndexOf(item.Key);
			if( index >= 0 )
				{
				return _values[index] == item.Value;
				}

			return false;
			}

		[DebuggerHidden]
		void ICollection<ConstPair>.CopyTo( ConstPair[] array, int arrayIndex )
			{
			if( array == null )
				throw new ArgumentNullException("array");

			if( (arrayIndex < 0) || (arrayIndex > array.Length) )
				throw new ArgumentOutOfRangeException("arrayIndex");

			if( (array.Length - arrayIndex) < _names.Count )
				throw new ArithmeticException( );

			for(int i = 0; i < _names.Count; i++)
				{
				array[arrayIndex + i] = new ConstPair(_names[i], _values[i]);
				}
			}

		/// <summary>
		/// Removes all constants from the <see cref="ConstantCollection"/>.
		/// </summary>
		[DebuggerHidden]
		public void Clear( )
			{
			_names.Clear( );
			_values.Clear( );
			}

		/// <summary>
		/// Gets the number of constants actually contained
		/// in the <see cref="ConstantCollection"/>.
		/// </summary>
		[DebuggerBrowsable( State.Never )]
		public int Count
			{
			[DebuggerHidden]
			get { return _names.Count; }
			}

		/// <summary>
		/// Gets a value indicating whether the 
		/// <see cref="ICollection{T}"/> is read-only.
		/// </summary>
		/// <value>Always <b>false</b>.</value>
		[DebuggerBrowsable( State.Never )]
		public bool IsReadOnly
			{
			[DebuggerHidden]
			get { return false; }
			}

		[DebuggerHidden]
		bool ICollection<ConstPair>.Remove( ConstPair item )
			{
			int index = _names.IndexOf(item.Key);
			if( index >= 0 )
				{
				_names.RemoveAt(index);
				_values.RemoveAt(index);
				return true;
				}

			return false;
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through
		/// the names in <see cref="ConstantCollection"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="List{T}.Enumerator"/> for the names in <see cref="ConstantCollection"/>.
		/// </returns>
		[DebuggerHidden]
		IEnumerator<ConstPair> IEnumerable<ConstPair>.GetEnumerator( )
			{
			for( int i = 0; i < _names.Count; i++ )
				{
				yield return new ConstPair(_names[i], _values[i]);
				}
			}
		
		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator( )
			{
			return (( IEnumerable<ConstPair> ) this).GetEnumerator( );
			}

		#endregion
		}
	
	#region Debug View

	sealed class ConstantsDebugView
		{
		[DebuggerDisplay( "{value}", Name = "{name}" )]
		private struct ViewItem
			{
			// ReSharper disable UnaccessedField.Local
			[DebuggerBrowsable( State.Never )]
			public string name;
			[DebuggerBrowsable( State.Never )]
			public double value;
			// ReSharper restore UnaccessedField.Local
			}

		[DebuggerBrowsable( State.RootHidden )]
		private readonly ViewItem[] items;

		public ConstantsDebugView( ConstantCollection list )
			{
			items = new ViewItem[list.Count];
			int i = 0;
			foreach( ConstPair item in list )
				{
				items[i].name  = item.Key;
				items[i].value = item.Value;
				i++;
				}
			}
		}

	#endregion
	}