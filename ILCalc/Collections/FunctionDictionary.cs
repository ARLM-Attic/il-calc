using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
{
	using FuncPair = KeyValuePair<string, FunctionGroup>;
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Manages the pairs list of names and attached function groups available
	/// to an expression. Function names are unique, but they can be overloaded
	/// by arguments count and the parameters array presence.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(FunctionDebugView))]
	[Serializable]

	public sealed class FunctionDictionary : IDictionary<string, FunctionGroup>, ICollection
	{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly List<string> namesList;

		[DebuggerBrowsable(State.Never)]
		private readonly List<FunctionGroup> funcsList;

		[DebuggerBrowsable(State.Never)]
		[NonSerialized]
		private object syncRoot;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionDictionary"/>
		/// class that is empty and has the default initial capacity.</summary>
		/// <overloads>Initializes a new instance of the
		/// <see cref="FunctionDictionary"/> class.</overloads>
		[DebuggerHidden]
		public FunctionDictionary()
		{
			this.namesList = new List<string>();
			this.funcsList = new List<FunctionGroup>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionDictionary"/>
		/// class from the other <see cref="FunctionDictionary"/> instance.</summary>
		/// <param name="list"><see cref="FunctionDictionary"/> instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="list"/> is null.</exception>
		[DebuggerHidden]
		public FunctionDictionary(FunctionDictionary list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			this.namesList = new List<string>(list.namesList);
			this.funcsList = new List<FunctionGroup>(list.Count);

			foreach (FunctionGroup g in list.funcsList)
			{
				this.funcsList.Add(new FunctionGroup(g));
			}
		}

		#endregion
		#region Properties

		/// <summary>Gets a collection containing the names
		/// of the <see cref="FunctionDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public ICollection<string> Keys
		{
			[DebuggerHidden]
			get { return this.namesList.AsReadOnly(); }
		}

		/// <summary>Gets a collection containing the function
		/// groups of the <see cref="FunctionDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public ICollection<FunctionGroup> Values
		{
			[DebuggerHidden]
			get { return this.funcsList.AsReadOnly(); }
		}

		/// <summary>Gets the number of functions actually
		/// contained in the <see cref="FunctionDictionary"/>.</summary>
		[DebuggerBrowsable(State.Never)]
		public int Count
		{
			[DebuggerHidden]
			get { return this.namesList.Count; }
		}

		/// <summary>Gets a value indicating whether the
		/// <see cref="ICollection{T}"/> is read-only.</summary>
		/// <value>Always <b>false</b>.</value>
		[DebuggerBrowsable(State.Never)]
		public bool IsReadOnly
		{
			[DebuggerHidden]
			get { return false; }
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
				System.Threading.Interlocked
					.CompareExchange(ref this.syncRoot, new object(), null);
				}

			return this.syncRoot;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="FunctionGroup"/>
		/// associated with the specified name.</summary>
		/// <overloads>Gets or sets the <see cref="FunctionGroup"/>
		/// with the specified function name or index.</overloads>
		/// <param name="key">The name of the function,
		/// which <see cref="FunctionGroup"/> to get or set.</param>
		/// <exception cref="KeyNotFoundException">The property is retrieved
		/// and name does not exist in the dictionary.</exception>
		/// <exception cref="ArgumentException">The property is setted
		/// and <paramref name="key"/> is not valid identifier name.</exception>
		/// <exception cref="ArgumentNullException">
		/// The property is setted and <paramref name="key"/> is null.</exception>
		/// <returns>The <see cref="FunctionGroup"/> associated with the specified
		/// function name. If the specified name is not found, a get operation
		/// throws a <see cref="KeyNotFoundException"/>, and a set operation
		/// creates a new function with the specified name.</returns>
		[DebuggerBrowsable(State.Never)]
		public FunctionGroup this[string key]
		{
			[DebuggerHidden]
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				int index = this.namesList.IndexOf(key);
				if (index < 0)
				{
					throw new KeyNotFoundException(
						string.Format(Resource.errFunctionNotExist, key));
				}

				return this.funcsList[index];
			}

			[DebuggerHidden]
			set
			{
				int index = this.namesList.IndexOf(key);
				if (index < 0)
				{
					this.Add(key, value);
				}
				else
				{
					this.funcsList[index] = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="FunctionGroup"/>
		/// at the specified index.</summary>
		/// <param name="index">The index of the function,
		/// which <see cref="FunctionGroup"/> to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">index is less than 0.
		/// <br/>-or-<br/>index is equal to or greater than <see cref="Count"/></exception>
		/// <returns>The <see cref="FunctionGroup"/> at the specified index.</returns>
		public FunctionGroup this[int index]
			{
			[DebuggerHidden] get { return this.funcsList[index];  }
			[DebuggerHidden] set { this.funcsList[index] = value; }
			}

		#endregion
		#region Methods

		#region Add

		/// <summary>
		/// Adds the <see cref="FunctionItem"/> to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <overloads>Adds the function to the
		/// <see cref="FunctionDictionary"/>.</overloads>
		/// <param name="function"><see cref="FunctionItem"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(FunctionItem function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			this.InternalAdd(function);
		}

		/// <summary>
		/// Adds the <see cref="FunctionItem"/> to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Funtion group name.</param>
		/// <param name="function"><see cref="FunctionItem"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, FunctionItem function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			this.InternalAdd(name, function);
		}

#if !CF2

		/// <summary>
		/// Adds the <see cref="EvalFunc0"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc0"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(EvalFunc0 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(new FunctionItem(target.Method, 0, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc0"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc0"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, EvalFunc0 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(name, new FunctionItem(target.Method, 0, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc1"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc1"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(EvalFunc1 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(new FunctionItem(target.Method, 1, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc1"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc1"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, EvalFunc1 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(name, new FunctionItem(target.Method, 1, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc2"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc2"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(EvalFunc2 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(new FunctionItem(target.Method, 2, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc2"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc2"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, EvalFunc2 target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(name, new FunctionItem(target.Method, 2, false));
		}

		/// <summary>
		/// Adds the <see cref="EvalFuncN"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFuncN"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(EvalFuncN target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(new FunctionItem(target.Method, 0, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFuncN"/> delegate
		/// to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFuncN"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, EvalFuncN target)
		{
			FunctionFactory.CheckDelegate(target, true);
			this.InternalAdd(name, new FunctionItem(target.Method, 0, true));
		}

#endif

		/// <summary>
		/// Adds the method reflection to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			this.InternalAdd(FunctionFactory.CreateInstance(method, true));
		}

		/// <summary>
		/// Adds the method reflection to the <see cref="FunctionDictionary"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method
		/// to be added to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and same arguments count already
		/// exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string name, MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			this.InternalAdd(name, FunctionFactory.CreateInstance(method, true));
		}

		/// <summary>
		/// Adds the method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// in the <see cref="FunctionDictionary"/> with the function name,
		/// taken from real method name.</summary>
		/// <param name="methodName">Type's method name to be imported.</param>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.<br/>-or-<br/>
		/// <paramref name="methodName"/>is null.</exception>
		/// <exception cref="ArgumentException">
		/// Method with <paramref name="methodName"/> is not founded.
		/// <br/>-or-<br/>Founded method is not valid to be added
		/// into this <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		/// <exception cref="System.Reflection.AmbiguousMatchException">
		/// If <paramref name="type"/> contains more than one methods
		/// matching the specified <paramref name="methodName"/>.</exception>
		[DebuggerHidden]
		public void Add(string methodName, Type type)
		{
			this.Add(FunctionFactory.GetHelper(type, methodName, -1));
		}

		/// <summary>
		/// Adds the method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// and arguments count to the <see cref="FunctionDictionary"/>
		/// with the function name, taken from real method name.</summary>
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
		/// to the <see cref="FunctionDictionary"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(Type type, string methodName, int parametersCount)
		{
			if (parametersCount < 0)
				throw new ArgumentOutOfRangeException("parametersCount");

			this.Add(FunctionFactory.GetHelper(type, methodName, parametersCount));
		}

		/// <summary>
		/// Adds the contents of <see cref="FunctionGroup"/>
		/// to the <see cref="FunctionDictionary"/> with the
		/// specified function group name.</summary>
		/// <param name="key">Funtion group name.</param>
		/// <param name="value"><see cref="FunctionGroup"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is null.<br/>-or-<br/>
		/// <paramref name="value"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing <see cref="FunctionItem"/> has the same name
		/// and the same arguments count as the function that is already
		/// in the dictionary (overload impossible).</exception>
		[DebuggerHidden]
		public void Add(string key, FunctionGroup value)
		{
			int index = this.namesList.IndexOf(key);
			if (index >= 0)
			{
				FunctionGroup group = this.funcsList[index];
				foreach (FunctionItem func in value)
				{
					if (!group.InternalAppend(func))
					{
						throw OverloadImpossible(func);
					}
				}
			}
			else
			{
				this.namesList.Add(key);
				this.funcsList.Add(value);
			}
		}

		#endregion
		#region Import

		/// <summary>
		/// Imports standart built-in functions from the <c>System.Math</c>
		/// type to the <see cref="FunctionDictionary"/>.</summary>
		/// <remarks>Currently this method imports this methods:<br/>
		/// Abs, Sin, Cos, Tan, Sinh, Cosh, Tanh, Acos, Asin, Atan, Atan2,
		/// Ceil, Floor, Round, Trunc (not available in CF/Silverlight),
		/// Log, Log10, Min, Max, Exp, Pow and Sqrt.</remarks>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name
		/// and the same arguments count as the function that is already
		/// in the dictionary (overload impossible).</exception>
		public void ImportBuiltIn()
		{
			var math = typeof(Math);
			var type = typeof(double);
			var oneArg = new[] { type };
			var twoArg = new[] { type, type };

			this.Add("Abs", math.GetMethod("Abs", oneArg));

			this.Add("Sin", math.GetMethod("Sin"));
			this.Add("Cos", math.GetMethod("Cos"));
			this.Add("Tan", math.GetMethod("Tan"));

			this.Add("Sinh", math.GetMethod("Sinh"));
			this.Add("Cosh", math.GetMethod("Cosh"));
			this.Add("Tanh", math.GetMethod("Tanh"));

			this.Add("Acos", math.GetMethod("Acos"));
			this.Add("Asin", math.GetMethod("Asin"));
			this.Add("Atan", math.GetMethod("Atan"));
			this.Add("Atan2", math.GetMethod("Atan2"));

			this.Add("Ceil", math.GetMethod("Ceiling", oneArg));
			this.Add("Floor", math.GetMethod("Floor", oneArg));
			this.Add("Round", math.GetMethod("Round", oneArg));

#if !SILVERLIGHT && !CF
			this.Add("Trunc", math.GetMethod("Truncate", oneArg));
#endif

			this.Add("Log", math.GetMethod("Log", oneArg));
#if !CF
			this.Add("Log", math.GetMethod("Log", twoArg));
#endif
			this.Add("Log10", math.GetMethod("Log10"));

			this.Add("Min", math.GetMethod("Min", twoArg));
			this.Add("Max", math.GetMethod("Max", twoArg));

			this.Add("Exp", math.GetMethod("Exp"));
			this.Add("Pow", math.GetMethod("Pow"));
			this.Add("Sqrt", math.GetMethod("Sqrt"));
		}

		/// <summary>
		/// Imports all public static methods of the specified type that is suitable
		/// to be added to the <see cref="FunctionDictionary"/>.</summary>
		/// <overloads>Imports static methods of the specified type(s) that is suitable
		/// to be added into this <see cref="FunctionDictionary"/>.</overloads>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">Some of importing methods has the same name
		/// and the same arguments count as the function that is already
		/// in the dictionary (overload impossible).</exception>
		public void Import(Type type)
		{
			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			this.InternalImport(type, Flags);
		}

		/// <summary>
		/// Imports all static methods of the specified type that is
		/// suitable to be added to the <see cref="FunctionDictionary"/>.
		/// </summary>
		/// <param name="type">Type object.</param>
		/// <param name="nonpublic">Include non public member methods in the search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and the same arguments count as the
		/// function that is already in the dictionary (overload impossible).</exception>
		public void Import(Type type, bool nonpublic)
		{
			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			this.InternalImport(type, nonpublic ? Flags | BindingFlags.NonPublic : Flags);
			}

		/// <summary>
		/// Imports all static methods of the specified types that is
		/// suitable to be added to the <see cref="FunctionDictionary"/>.
		/// </summary>
		/// <param name="types">Array of <see cref="Type"/> objects.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="types"/> is null.</exception><br>-or-</br>
		/// Some Type of <paramref name="types"/> is null.
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and the same arguments count as the
		/// function that is already in the dictionary (overload impossible).</exception>
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
				this.InternalImport(type, Flags);
			}
		}

		#endregion

		/// <summary>
		/// Removes the function specified by name 
		/// from the <see cref="FunctionDictionary"/>.</summary>
		/// <overloads>
		/// Removes the function from the 
		/// <see cref="FunctionDictionary"/>.</overloads>
		/// <param name="key">The function name to be removed.</param>
		/// <returns><b>true</b> if function is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool Remove(string key)
		{
			int index = this.namesList.IndexOf(key);
			if (index >= 0)
			{
				this.namesList.RemoveAt(index);
				this.funcsList.RemoveAt(index);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes the function overload specified by name, arguments count
		/// and params arguments usage from the <see cref="FunctionDictionary"/>.
		/// </summary>
		/// <param name="name">The function name.</param>
		/// <param name="argsCount">Overload arguments count.</param>
		/// <param name="hasParamArray">Is overload has params.</param>
		/// <returns><b>true</b> if function overload is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove(string name, int argsCount, bool hasParamArray)
		{
			int index = this.namesList.IndexOf(name);
			if (index >= 0 &&
				this.funcsList[index].Remove(argsCount, hasParamArray))
			{
				if (this.funcsList[index].Count == 0)
				{
					this.namesList.RemoveAt(index);
					this.funcsList.RemoveAt(index);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the <see cref="FunctionDictionary"/>
		/// contains the specified name.</summary>
		/// <param name="key">Function name to locate
		/// in the <see cref="FunctionDictionary"/>.</param>
		/// <returns><b>true</b> if name is found in the list;
		/// otherwise, <b>false</b>.</returns>
		[DebuggerHidden]
		public bool ContainsKey(string key)
		{
			return this.namesList.Contains(key);
		}

		/// <summary>
		/// Tries to get the <see cref="FunctionGroup"/>
		/// associated with the specified name.</summary>
		/// <param name="key">The name of the function,
		/// which <see cref="FunctionGroup"/> to get.</param>
		/// <param name="value">When this method returns, contains the
		/// <see cref="FunctionGroup"/> of the function with the specified name,
		/// if the name is found; otherwise, contains <c>null</c>.
		/// This parameter is passed uninitialized.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is null.</exception>
		/// <returns><b>true</b> if the <see cref="FunctionDictionary"/> contains
		/// an function with the specified name; otherwise, <b>false</b>.</returns>
		public bool TryGetValue(string key, out FunctionGroup value)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			int index = this.namesList.IndexOf(key);
			if (index >= 0)
			{
				value = this.funcsList[index];
				return true;
			}

			value = null;
			return false;
		}

		[DebuggerHidden]
		void ICollection<FuncPair>.Add(FuncPair item)
		{
			this.Add(item.Key, item.Value);
		}

		[DebuggerHidden]
		bool ICollection<FuncPair>.Contains(FuncPair item)
		{
			int index = this.namesList.IndexOf(item.Key);
			return index >= 0
				&& ReferenceEquals(this.funcsList[index], item.Value);
		}

		[DebuggerHidden]
		void ICollection<FuncPair>.CopyTo(FuncPair[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (arrayIndex < 0 || arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException("arrayIndex");

			if (array.Length - arrayIndex < this.Count)
			{
				throw new ArithmeticException();
			}

			for (int i = 0; i < this.Count; i++)
			{
				array[arrayIndex + i] = new FuncPair(
					this.namesList[i],
					new FunctionGroup(this.funcsList[i]));
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection<FuncPair>) this).CopyTo((FuncPair[]) array, index);
		}

		/// <summary>Removes all functions
		/// from the <see cref="FunctionDictionary"/>.</summary>
		[DebuggerHidden]
		public void Clear()
		{
			this.namesList.Clear();
			this.funcsList.Clear();
		}

		[DebuggerHidden]
		bool ICollection<FuncPair>.Remove(FuncPair item)
		{
			int index = this.namesList.IndexOf(item.Key);
			if (index >= 0 &&
				item.Value == this.funcsList[index])
			{
				this.namesList.RemoveAt(index);
				this.funcsList.RemoveAt(index);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the pairs
		/// in the <see cref="FunctionDictionary"/>.</summary>
		/// <returns>An enumerator object for pair items
		/// in the <see cref="FunctionDictionary"/>.</returns>
		IEnumerator<FuncPair> IEnumerable<FuncPair>.GetEnumerator()
		{
			for (int i = 0; i < this.namesList.Count; i++)
			{
				yield return new
					FuncPair(this.namesList[i], this.funcsList[i]);
			}

			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<FuncPair>)this).GetEnumerator();
		}

		#endregion
		#region Privates

		private static ArgumentException OverloadImpossible(FunctionItem func)
		{
			return new ArgumentException(
				string.Format(Resource.errOverloadImpossible, func.ArgsString));
		}

		private void InternalAdd(string name, FunctionItem function)
		{
			Validate.Name(name);
			int index = this.namesList.IndexOf(name);
			if (index >= 0)
			{
				if (!this.funcsList[index].InternalAppend(function))
				{
					throw OverloadImpossible(function);
				}
			}
			else
			{
				this.namesList.Add(name);
				this.funcsList.Add(new FunctionGroup(function));
			}
		}

		private void InternalAdd(FunctionItem function)
		{
			this.InternalAdd(function.MethodName, function);
		}

		private void InternalImport(Type type, BindingFlags flags)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			foreach (MethodInfo method in type.GetMethods(flags))
			{
				FunctionItem func = FunctionFactory.CreateInstance(method, false);
				if (func != null)
				{
					this.InternalAdd(func);
				}
			}
		}

		#endregion
		#region Debug View

		private sealed class FunctionDebugView
		{
			[DebuggerBrowsable(State.RootHidden)]
			private readonly ViewItem[] items;

			public FunctionDebugView(FunctionDictionary list)
			{
				this.items = new ViewItem[list.Count];
				int i = 0;
				foreach (FuncPair item in list)
				{
					this.items[i].Name  = item.Key;
					this.items[i].Funcs = item.Value;
					i++;
				}
			}

			[DebuggerDisplay("{Funcs.Count} functions", Name = "{Name}")]
			private struct ViewItem
			{
				// ReSharper disable UnaccessedField.Local
				[DebuggerBrowsable(State.Never)]
				public string Name;
				[DebuggerBrowsable(State.RootHidden)]
				public FunctionGroup Funcs;

				// ReSharper restore UnaccessedField.Local
			}
		}

	#endregion
	}
}