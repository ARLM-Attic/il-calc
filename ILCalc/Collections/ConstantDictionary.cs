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

	public sealed class ConstantDictionary
			: IDictionary<string, double>, ICollection,
			  IQuickEnumerable
		{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly List<string> namesList;

		[DebuggerBrowsable(State.Never)]
		private readonly List<double> valuesList;

		[DebuggerBrowsable(State.Never), NonSerialized]
		private object syncRoot;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/> class
		/// that is empty and has the default initial capacity.</summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/> class.
		/// </overloads>
		public ConstantDictionary()
		{
			this.namesList  = new List<string>();
			this.valuesList = new List<double>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantDictionary"/>
		/// class from the instance of <see cref="ICollection{T}"/> containing
		/// pairs of constant names and values.</summary>
		/// <param name="collection"><see cref="ICollection"/>
		/// of the name/value pairs.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="collection"/>
		/// is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="collection"/>
		/// is already exist in the dictionary.</exception>
		public ConstantDictionary(ICollection<ConstPair> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			this.namesList  = new List<string>(collection.Count);
			this.valuesList = new List<double>(collection.Count);

			foreach (var pair in collection)
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
		public ConstantDictionary(ConstantDictionary dictionary)
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			this. namesList = new List<string>(dictionary. namesList);
			this.valuesList = new List<double>(dictionary.valuesList);
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets a collection containing the constant names
		/// of the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public ICollection<string> Keys
		{
			get { return this.namesList.AsReadOnly(); }
		}

		/// <summary>
		/// Gets a collection containing the constant values
		/// of the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public ICollection<double> Values
		{
			get { return this.valuesList.AsReadOnly(); }
		}

		/// <summary>
		/// Gets a value indicating whether the 
		/// <see cref="ICollection{T}"/> is read-only.</summary>
		/// <value>Always <b>false</b>.</value>
		[DebuggerBrowsable(State.Never)]
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the number of constants actually contained
		/// in the <see cref="ConstantDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public int Count
		{
			get { return this.namesList.Count; }
		}

		[DebuggerBrowsable(State.Never)]
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		[DebuggerBrowsable(State.Never)]
		object ICollection.SyncRoot
		{
			get
			{
				if (this.syncRoot == null)
				{
					System.Threading.Interlocked.CompareExchange(
						ref this.syncRoot, new object(), null);
				}

				return this.syncRoot;
			}
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
		[DebuggerBrowsable(State.Never)]
		public double this[string key]
		{
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				int index = this.namesList.IndexOf(key);
				if (index < 0)
				{
					throw new KeyNotFoundException(
						string.Format(Resource.errConstantNotExist, key));
				}

				return this.valuesList[index];
			}

			set
			{
				int index = this.namesList.IndexOf(key);

				if (index < 0) Add(key, value);
				else this.valuesList[index] = value;
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
			get { return this.valuesList[index]; }
			set { this.valuesList[index] = value; }
		}

		#endregion
		#region Methods

		/// <summary>
		/// Adds the constant with the provided name and value
		/// to the <see cref="ConstantDictionary"/>.</summary>
		/// <param name="key">Constant name.</param>
		/// <param name="value">Constant value.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="key"/> is not valid identifier name.<br/>-or-<br/>
		/// <paramref name="key"/> name is already exist in the dictionary.
		/// </exception>
		public void Add(string key, double value)
		{
			Validate.Name(key);
			if (this.namesList.Contains(key))
			{
				throw new ArgumentException(
					string.Format(Resource.errConstantExist, key));
			}

			this. namesList.Add(key);
			this.valuesList.Add(value);
		}

		/// <summary>
		/// Adds the elements of the specified collection
		/// to the end of the <see cref="ConstantDictionary"/>.</summary>
		/// <param name="collection">Enumerable of the name and value pairs.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="collection"/>
		/// is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="collection"/>
		/// is already exist in the dictionary.</exception>
		public void AddRange(IEnumerable<ConstPair> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			foreach (ConstPair pair in collection)
			{
				Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Determines whether the <see cref="ConstantDictionary"/>
		/// contains the specified name.</summary>
		/// <param name="key">Constant name to locate in the
		/// <see cref="ConstantDictionary"/>.</param>
		/// <returns><b>true</b> if name is found in the dictionary;
		/// otherwise, <b>false</b>.</returns>
		public bool ContainsKey(string key)
		{
			return this.namesList.Contains(key);
		}

		/// <summary>
		/// Removes the constant specified by name from the
		/// <see cref="ConstantDictionary"/>.</summary>
		/// <param name="key">The function name to be removed.</param>
		/// <returns><b>true</b> if constant is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove(string key)
		{
			int index = this.namesList.IndexOf(key);
			if (index >= 0)
			{
				this.namesList .RemoveAt(index);
				this.valuesList.RemoveAt(index);
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
		public bool TryGetValue(string key, out double value)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			int index = this.namesList.IndexOf(key);
			if (index < 0)
			{
				value = default(double);
				return false;
			}

			value = this.valuesList[index];
			return true;
		}

		void ICollection<ConstPair>.Add(ConstPair item)
		{
			Add(item.Key, item.Value);
		}

		bool ICollection<ConstPair>.Contains(ConstPair item)
		{
			int index = this.namesList.IndexOf(item.Key);

			return index >= 0
				&& this.valuesList[index] == item.Value;
		}

		void ICollection<ConstPair>.CopyTo(ConstPair[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (arrayIndex < 0 || arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException("arrayIndex");

			if (array.Length - arrayIndex < Count)
				throw new ArgumentOutOfRangeException("arrayIndex");

			for (int i = 0; i < Count; i++)
			{
				array[arrayIndex + i] = new ConstPair(
					this.namesList[i], this.valuesList[i]);
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection<ConstPair>) this)
				.CopyTo((ConstPair[]) array, index);
		}

		/// <summary>
		/// Removes all constants from
		/// the <see cref="ConstantDictionary"/>.</summary>
		public void Clear()
		{
			this. namesList.Clear();
			this.valuesList.Clear();
		}

		bool ICollection<ConstPair>.Remove(ConstPair item)
		{
			int index = this.namesList.IndexOf(item.Key);

			if (index >= 0 &&
				this.valuesList[index] == item.Value)
			{
				this.namesList .RemoveAt(index);
				this.valuesList.RemoveAt(index);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the pairs of names
		/// and values in <see cref="ConstantDictionary"/>.</summary>
		/// <returns>An enumerator object for the pair items
		/// in the <see cref="ConstantDictionary"/>.</returns>
		IEnumerator<ConstPair> IEnumerable<ConstPair>.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return new ConstPair(
					this. namesList[i],
					this.valuesList[i]);
			}

			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ConstPair>) this).GetEnumerator();
		}

		/// <summary>
		/// Imports standart builtin constants into 
		/// this <see cref="ConstantDictionary"/>.</summary>
		/// <remarks>Currently this method imports Pi,
		/// E, Inf and NaN constants.</remarks>
		/// <exception cref="ArgumentException">
		/// Some of names is already exist in the dictionary.</exception>
		public void ImportBuiltIn()
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
		public void Import(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(type, Flags);
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
		public void Import(Type type, bool nonpublic)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(
				type, Flags | (nonpublic ? BindingFlags.NonPublic : 0));
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
		public void Import(params Type[] types)
		{
			if (types == null)
				throw new ArgumentNullException("types");

			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			foreach (Type type in types)
			{
				InternalImport(type, Flags);
			}
		}

		private void InternalImport(Type type, BindingFlags flags)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			foreach (FieldInfo field in type.GetFields(flags))
			{
				// look for "const double" fields:
				if (field.IsLiteral &&
				    field.FieldType == TypeHelper.ValueType)
				{
					var value = (double) field.GetValue(null);
					Add(field.Name, value);
				}
			}
		}

		#endregion
		#region Internals

		List<string>.Enumerator IQuickEnumerable.GetEnumerator()
		{
			return this.namesList.GetEnumerator();
		}

		#endregion
		#region Debug View

		private sealed class ConstantsDebugView
		{
			[DebuggerBrowsable(State.RootHidden)]
			private readonly ViewItem[] items;

			public ConstantsDebugView(ConstantDictionary list)
			{
				this.items = new ViewItem[list.Count];
				int i = 0;
				foreach (ConstPair item in list)
				{
					this.items[i].Name  = item.Key;
					this.items[i].Value = item.Value;
					i++;
				}
			}

			[DebuggerDisplay("{Value}", Name = "{Name}")]
			private struct ViewItem
			{
				// ReSharper disable UnaccessedField.Local

				[DebuggerBrowsable(State.Never)] public string Name;
				[DebuggerBrowsable(State.Never)] public double Value;

				// ReSharper restore UnaccessedField.Local
			}
		}

		#endregion
	}
}