using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
	{
	using ConstPair = KeyValuePair<string, double>;
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Manages the collection of pairs from unique names and values of
	/// constants available to an expression.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>

	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(ConstantsDebugView))]
	[Serializable]

	public sealed class ConstantDictionary : IDictionary<string, double>
		{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly List<string> namesList;
		[DebuggerBrowsable(State.Never)]
		private readonly List<double> valuesList;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/> class
		/// that is empty and has the default initial capacity.</summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/> class.
		/// </overloads>
		[DebuggerHidden]
		public ConstantDictionary( )
			{
			namesList  = new List<string>( );
			valuesList = new List<double>( );
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/>
		/// class from the instance of <see cref="ICollection{T}"/> containing
		/// pairs of constant names and values.</summary>
		/// <param name="collection"><see cref="ICollection"/> of the name/value pairs.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="collection"/> is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="collection"/> is already exist in the dictionary.
		/// </exception>
		public ConstantDictionary( ICollection<ConstPair> collection )
			{
			if( collection == null )
				throw new ArgumentNullException("collection");

			namesList  = new List<string>(collection.Count);
			valuesList = new List<double>(collection.Count);

			foreach( ConstPair pair in collection )
				{
				Add(pair.Key, pair.Value);
				}
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/>
		/// class from the other <see cref="ConstantDictionary"/> instance.</summary>
		/// <param name="dictionary"><see cref="ConstantDictionary"/> instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is null.</exception>
		[DebuggerHidden]
		public ConstantDictionary( ConstantDictionary dictionary )
			{
			if( dictionary == null )
				throw new ArgumentNullException("dictionary");

			namesList  = new List<string>(dictionary.namesList);
			valuesList = new List<double>(dictionary.valuesList);
			}

		#endregion
		#region IDictionary<>

		/// <summary>
		/// Adds the constant with the provided name and value
		/// to the <see cref="ConstantDictionary"/>.</summary>
		/// <param name="key">Constant name.</param>
		/// <param name="value">Constant value.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="key"/> is not valid identifier name.<br/>-or-<br/>
		/// <paramref name="key"/> name is already exist in the dictionary.
		/// </exception>
		public void Add( string key, double value )
			{
			Validate.Name(key);

			if( namesList.Contains(key) )
				{
				throw new ArgumentException(
					string.Format(Resources.errConstantExist, key)
					);
				}

			namesList.Add(key);
			valuesList.Add(value);
			}

		/// <summary>
		/// Determines whether the <see cref="ConstantDictionary"/>
		/// contains the specified name.</summary>
		/// <param name="key">Constant name to locate
		/// in the <see cref="ConstantDictionary"/>.</param>
		/// <returns><b>true</b> if name is found in the dictionary;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool ContainsKey( string key )
			{
			return namesList.Contains(key);
			}

		/// <summary>
		/// Gets a collection containing the constant names
		/// of the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable( State.Never )]
		public ICollection<string> Keys
			{
			[DebuggerHidden] get { return namesList.AsReadOnly( ); }
			}

		/// <summary>
		/// Gets a collection containing the constant values
		/// of the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public ICollection<double> Values
			{
			[DebuggerHidden] get { return valuesList.AsReadOnly( ); }
			}

		/// <summary>
		/// Removes the constant specified by name from
		/// the <see cref="ConstantDictionary"/>.</summary>
		/// <param name="key">The function name to be removed.</param>
		/// <returns><b>true</b> if constant is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove( string key )
			{
			int index = namesList.IndexOf(key);
			if( index >= 0 )
				{
				namesList .RemoveAt(index);
				valuesList.RemoveAt(index);

				return true;
				}

			return false;
			}

		/// <summary>
		/// Tries to get the value of constant with the specified name.</summary>
		/// <param name="key">The name of the constant, which value to get.</param>
		/// <param name="value">When this method returns, contains the value
		/// of constant with the specified name, if the name is found;
		/// otherwise, the default value for the type of the value parameter.
		/// This parameter is passed uninitialized.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is null.</exception>
		/// <returns><b>true</b> if the <see cref="ConstantDictionary"/> contains
		/// an element with the specified name; otherwise, <b>false</b>.</returns>
		public bool TryGetValue( string key, out double value )
			{
			if( key == null )
				throw new ArgumentNullException("key");

			int index = namesList.IndexOf(key);
			if( index >= 0 )
				{
				value = valuesList[index];
				return true;
				}

			value = default(double);
			return false;
			}

		/// <summary>
		/// Gets or sets the constant value associated
		/// with the specified name.</summary>
		/// <overloads>Gets or sets the value associated with
		/// the specified constant name or index.</overloads>
		/// <param name="key">The name of the constant,
		/// which value to get or set.</param>
		/// <exception cref="KeyNotFoundException">The property is retrieved
		/// and name does not exist in the dicitonary.</exception>
		/// <exception cref="ArgumentException"> The property is setted
		/// and <paramref name="key"/> is not valid identifier name.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is null.</exception>
		/// <returns>The value associated with the specified name.
		/// If the specified name is not found, a get operation throws
		/// a <see cref="KeyNotFoundException"/>, and a set operation
		/// creates a new element with the specified name.</returns>
		[DebuggerBrowsable( State.Never )]
		public double this[ string key ]
			{
			[DebuggerHidden]
			get
				{
				if( key == null )
					throw new ArgumentNullException("key");

				int index = namesList.IndexOf(key);
				if( index >= 0 ) return valuesList[index];

				throw new KeyNotFoundException(
					string.Format(Resources.errConstantNotExist, key)
					);
				}
			[DebuggerHidden]
			set
				{
				int index = namesList.IndexOf(key);
				if( index < 0 ) Add(key, value);
				else valuesList[index] = value;
				}
			}

		/// <summary>
		/// Gets or sets the constant value at the specified index.</summary>
		/// <param name="index">The list index of the constant,
		/// which value to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.
		/// <br/>-or-<br/>index is equal to or greater than <see cref="Count"/>.
		/// </exception>
		/// <returns>The constant value at the specified index.</returns>
		public double this[int index]
			{
			[DebuggerHidden] get { return valuesList[index]; }
			[DebuggerHidden] set { valuesList[index] = value; }
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
			int index = namesList.IndexOf(item.Key);
			if( index >= 0 )
				{
				return valuesList[index] == item.Value;
				}

			return false;
			}

		[DebuggerHidden]
		void ICollection<ConstPair>.CopyTo( ConstPair[] array, int arrayIndex )
			{
			if( array == null )
				throw new ArgumentNullException("array");

			if( arrayIndex < 0 || arrayIndex > array.Length )
				throw new ArgumentOutOfRangeException("arrayIndex");

			if( array.Length - arrayIndex < namesList.Count )
				throw new ArithmeticException( );

			for(int i = 0; i < namesList.Count; i++)
				{
				array[arrayIndex + i] = new ConstPair(namesList[i], valuesList[i]);
				}
			}

		/// <summary>
		/// Removes all constants from
		/// the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerHidden]
		public void Clear( )
			{
			namesList.Clear( );
			valuesList.Clear( );
			}

		/// <summary>Gets the number of constants actually contained
		/// in the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable( State.Never )]
		public int Count
			{
			[DebuggerHidden]
			get { return namesList.Count; }
			}

		/// <summary>Gets a value indicating whether the 
		/// <see cref="ICollection{T}"/> is read-only.</summary>
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
			int index = namesList.IndexOf(item.Key);
			if( index >= 0
			 && valuesList[index] == item.Value )
				{
				namesList.RemoveAt(index);
				valuesList.RemoveAt(index);
				return true;
				}

			return false;
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through the pairs of names
		/// and values in <see cref="ConstantDictionary"/>.</summary>
		/// <returns>An enumerator object for the pair items
		/// in the <see cref="ConstantDictionary"/>.</returns>
		[DebuggerHidden]
		IEnumerator<ConstPair> IEnumerable<ConstPair>.GetEnumerator( )
			{
			for( int i = 0; i < Count; i++ )
				{
				yield return new ConstPair(namesList[i], valuesList[i]);
				}

			yield break;
			}
		
		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator( )
			{
			return (( IEnumerable<ConstPair> ) this).GetEnumerator( );
			}

		#endregion
		#region Imports

		/// <summary>
		/// Imports standart builtin constants into 
		/// this <see cref="ConstantDictionary"/>.</summary>
		/// <remarks>
		/// Currently this method imports Pi, E, Inf and NaN constants.</remarks>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the dictionary.</exception>
		public void ImportBuiltIn( )
			{
			Add("E", Math.E);
			Add("Pi", Math.PI);

			Add("NaN", Double.NaN);
			Add("Inf", Double.PositiveInfinity);
			}

		/// <summary>
		/// Imports all public static fields of the specified type
		/// into this <see cref="ConstantDictionary"/>.</summary>
		/// <overloads>
		/// Imports static fields of the specified type(s)
		/// into this <see cref="ConstantDictionary"/>.</overloads>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of the importing constants has a name
		/// that is already exist in the dictionary.</exception>
		public void Import( Type type )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			const BindingFlags flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(type, flags);
			}

		/// <summary>
		/// Imports all public static fields of the specified type
		/// into this <see cref="ConstantDictionary"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <param name="nonpublic">
		/// Include non public member methods in search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of the importing constants has a name
		/// that is already exist in the dictionary.</exception>
		public void Import( Type type, bool nonpublic )
			{
			if( type == null )
				throw new ArgumentNullException("type");

			const BindingFlags flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(type, nonpublic? 
				flags | BindingFlags.NonPublic: flags);
			}

		/// <summary>
		/// Imports all public static fields of the specified types
		/// into this <see cref="ConstantDictionary"/>.</summary>
		/// <param name="types">
		/// Params array of <see cref="Type"/> objects.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="types"/> is null.<br>-or-</br>
		/// Some element of <paramref name="types"/> array is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of the importing constants has a name
		/// that is already exist in the dictionary.</exception>
		public void Import( params Type[] types )
			{
			if( types == null )
				throw new ArgumentNullException("types");

			const BindingFlags flags =
				BindingFlags.Static |
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

			foreach( FieldInfo field in type.GetFields(flags) )
				{
				// look for "const double" fields
				if( field.IsLiteral &&
					field.FieldType == FunctionFactory.valueType )
					{
					var value = ( double ) field.GetValue(null);
					Add(field.Name, value);
					}
				}
			}

		#endregion
		}
	
	#region Debug View

	sealed class ConstantsDebugView
		{
		[DebuggerDisplay("{value}", Name = "{name}")]
		private struct ViewItem
			{
			// ReSharper disable UnaccessedField.Local
			[DebuggerBrowsable(State.Never)] public string name;
			[DebuggerBrowsable(State.Never)] public double value;
			// ReSharper restore UnaccessedField.Local
			}

		[DebuggerBrowsable(State.RootHidden)]
		private readonly ViewItem[] items;

		public ConstantsDebugView( ConstantDictionary list )
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