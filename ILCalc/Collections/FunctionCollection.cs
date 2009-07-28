using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ILCalc
{
	using FuncPair = KeyValuePair<string, FunctionGroup>;
	using State = DebuggerBrowsableState;

	/// <summary>
	/// Manages the pairs list of names and attached function groups
	/// available to an expression. Function names are unique,
	/// but they can be overloaded by arguments count
	/// and the parameters array presence.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(FunctionDebugView))]
	[Serializable]

	public sealed class FunctionCollection
		: ICollection, IEnumerable<FuncPair>, IQuickEnumerable
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
		/// Initializes a new instance of the <see cref="FunctionCollection"/>
		/// class that is empty and has the default initial capacity.</summary>
		/// <overloads>Initializes a new instance of the
		/// <see cref="FunctionCollection"/> class.</overloads>
		public FunctionCollection()
		{
			this.namesList = new List<string>();
			this.funcsList = new List<FunctionGroup>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionCollection"/>
		/// class from the other <see cref="FunctionCollection"/> instance.</summary>
		/// <param name="collection"><see cref="FunctionCollection"/> instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is null.</exception>
		public FunctionCollection(FunctionCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			this.namesList = new List<string>(collection.namesList);
			this.funcsList = new List<FunctionGroup>(collection.Count);

			foreach (FunctionGroup group in collection.funcsList)
			{
				this.funcsList.Add(new FunctionGroup(group));
			}
		}

		#endregion
		#region Properties

		/// <summary>
		/// Gets a collection containing the names
		/// of the <see cref="FunctionCollection"/>.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public ReadOnlyCollection<string> Names
		{
			get { return this.namesList.AsReadOnly(); }
		}

		/// <summary>
		/// Gets a collection containing the function
		/// groups of the <see cref="FunctionCollection"/>.
		/// </summary>
		[DebuggerBrowsable(State.Never)]
		public ReadOnlyCollection<FunctionGroup> Functions
		{
			get { return this.funcsList.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the number of functions actually
		/// contained in the <see cref="FunctionCollection"/>.
		/// </summary>
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
		/// Gets the <see cref="FunctionGroup"/> associated
		/// with the specified function name.</summary>
		/// <overloads>Gets the <see cref="FunctionGroup"/>
		/// with the specified function name or index.</overloads>
		/// <param name="key">The name of the function,
		/// which <see cref="FunctionGroup"/> to get.</param>
		/// <exception cref="KeyNotFoundException">The property is retrieved
		/// and name does not exist in the collection.</exception>
		/// <exception cref="ArgumentNullException">
		/// The property is setted and <paramref name="key"/> is null.</exception>
		/// <returns>The <see cref="FunctionGroup"/> associated with the specified
		/// function name. If the specified name is not found
		/// throws a <see cref="KeyNotFoundException"/>.</returns>
		[DebuggerBrowsable(State.Never)]
		public FunctionGroup this[string key]
		{
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
		}

		/// <summary>
		/// Gets the <see cref="FunctionGroup"/> at the specified index.</summary>
		/// <param name="index">The index of the function,
		/// which <see cref="FunctionGroup"/> to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0.<br/>-or-<br/>
		/// <paramref name="index"/> is equal to or greater
		/// than <see cref="Count"/></exception>
		/// <returns>The <see cref="FunctionGroup"/>
		/// at the specified index.</returns>
		public FunctionGroup this[int index]
		{
			get { return this.funcsList[index];  }
		}

		#endregion
		#region Methods

		#region Add

		/// <summary>
		/// Adds the <see cref="FunctionItem"/> to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <overloads>Adds the function to the <see cref="FunctionCollection"/>.</overloads>
		/// <param name="function"><see cref="FunctionItem"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="function"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <see cref="FunctionItem"/> with same name
		/// and the same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(FunctionItem function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			AddFunc(function);
		}

		/// <summary>
		/// Adds the <see cref="FunctionItem"/> to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Funtion group name.</param>
		/// <param name="function"><see cref="FunctionItem"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null.<br/>-or-<br/>
		/// <paramref name="function"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <see cref="FunctionItem"/> with same name
		/// and the same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(string name, FunctionItem function)
		{
			if (function == null)
				throw new ArgumentNullException("function");

			AddFunc(name, function);
		}

		#region Delegates
#if !CF2

		/// <summary>
		/// Adds the <see cref="EvalFunc0"/> delegate
		/// to the <see cref="FunctionCollection"/> with the
		/// function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc0"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and the same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(EvalFunc0 target)
		{
			AddFunc(FunctionFactory.FromDelegate(target, 0, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc0"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc0"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null.<br/>-or-<br/>
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and the same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(string name, EvalFunc0 target)
		{
			AddFunc(name,
				FunctionFactory.FromDelegate(target, 0, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc1"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc1"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(EvalFunc1 target)
		{
			AddFunc(FunctionFactory.FromDelegate(target, 1, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc1"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc1"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null.<br/>-or-<br/>
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(string name, EvalFunc1 target)
		{
			AddFunc(name,
				FunctionFactory.FromDelegate(target, 1, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc2"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFunc2"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(EvalFunc2 target)
		{
			AddFunc(FunctionFactory.FromDelegate(target, 2, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFunc2"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFunc2"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null.<br/>-or-<br/>
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(string name, EvalFunc2 target)
		{
			AddFunc(name,
				FunctionFactory.FromDelegate(target, 2, false, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFuncN"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="target"><see cref="EvalFuncN"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(EvalFuncN target)
		{
			AddFunc(FunctionFactory.FromDelegate(target, 0, true, true));
		}

		/// <summary>
		/// Adds the <see cref="EvalFuncN"/> delegate
		/// to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="target"><see cref="EvalFuncN"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="target"/> is not valid delegate to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void Add(string name, EvalFuncN target)
		{
			AddFunc(name,
				FunctionFactory.FromDelegate(target, 0, true, true));
		}

#endif
		#endregion

		/// <summary>
		/// Adds the static method reflection to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void AddStatic(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			AddFunc(FunctionFactory.FromReflection(method, null, true));
		}

		/// <summary>
		/// Adds the static method reflection to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void AddStatic(string name, MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			AddFunc(name,
				FunctionFactory.FromReflection(method, null, true));
		}

		/// <summary>
		/// Adds the instance method reflection to the <see cref="FunctionCollection"/>
		/// with the function name, taken from real method name.</summary>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <param name="target">Instance method target object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.<br/>-or-<br/>
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void AddInstance(MethodInfo method, object target)
		{
			if (method == null) throw new ArgumentNullException("method");
			if (target == null) throw new ArgumentNullException("target");

			AddFunc(FunctionFactory.FromReflection(method, target, true));
		}

		/// <summary>
		/// Adds the instance method reflection to the <see cref="FunctionCollection"/>
		/// with the specified function name.</summary>
		/// <param name="name">Function group name.</param>
		/// <param name="method"><see cref="MethodInfo"/> instance to add.</param>
		/// <param name="target">Instance method target object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="method"/> is null.<br/>-or-<br/>
		/// <paramref name="target"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="method"/> is not valid method to be added
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and same arguments count already exist
		/// in the collection (overload impossible).</exception>
		public void AddInstance(string name, MethodInfo method, object target)
		{
			if (method == null) throw new ArgumentNullException("method");
			if (target == null) throw new ArgumentNullException("target");

			AddFunc(name,
				FunctionFactory.FromReflection(method, target, true));
		}

		#endregion
		#region Import

		/// <summary>
		/// Imports standart built-in functions from the <c>System.Math</c>
		/// type to the <see cref="FunctionCollection"/>.</summary>
		/// <remarks>Currently this method imports this methods:<br/>
		/// Abs, Sin, Cos, Tan, Sinh, Cosh, Tanh, Acos, Asin, Atan, Atan2,
		/// Ceil, Floor, Round, Trunc (not available in CF/Silverlight),
		/// Log, Log10, Min, Max, Exp, Pow and Sqrt.</remarks>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name
		/// and the same arguments count as the function that is already
		/// in the collection (overload impossible).</exception>
		public void ImportBuiltIn()
		{
			Type math = typeof(Math);
			Type type = typeof(double);
			var oneArg = new[] { type };
			var twoArg = new[] { type, type };

			AddStatic("Abs", math.GetMethod("Abs", oneArg));

			AddStatic("Sin", math.GetMethod("Sin"));
			AddStatic("Cos", math.GetMethod("Cos"));
			AddStatic("Tan", math.GetMethod("Tan"));

			AddStatic("Sinh", math.GetMethod("Sinh"));
			AddStatic("Cosh", math.GetMethod("Cosh"));
			AddStatic("Tanh", math.GetMethod("Tanh"));

			AddStatic("Acos", math.GetMethod("Acos"));
			AddStatic("Asin", math.GetMethod("Asin"));
			AddStatic("Atan", math.GetMethod("Atan"));
			AddStatic("Atan2", math.GetMethod("Atan2"));

			AddStatic("Ceil", math.GetMethod("Ceiling", oneArg));
			AddStatic("Floor", math.GetMethod("Floor", oneArg));
			AddStatic("Round", math.GetMethod("Round", oneArg));

#if !SILVERLIGHT && !CF
			AddStatic("Trunc", math.GetMethod("Truncate", oneArg));
#endif

			AddStatic("Log", math.GetMethod("Log", oneArg));
#if !CF
			AddStatic("Log", math.GetMethod("Log", twoArg));
#endif
			AddStatic("Log10", math.GetMethod("Log10"));

			AddStatic("Min", math.GetMethod("Min", twoArg));
			AddStatic("Max", math.GetMethod("Max", twoArg));

			AddStatic("Exp", math.GetMethod("Exp"));
			AddStatic("Pow", math.GetMethod("Pow"));
			AddStatic("Sqrt", math.GetMethod("Sqrt"));
		}

		/// <summary>
		/// Imports all public static methods
		/// of the specified type that is suitable to be added
		/// into the <see cref="FunctionCollection"/>.</summary>
		/// <overloads>Imports static methods
		/// of the specified type(s) that is suitable to be added
		/// into this <see cref="FunctionCollection"/>.</overloads>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and
		/// the same arguments count as the function that is already
		/// in the collection (overload impossible).</exception>
		public void Import(Type type)
		{
			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(type, Flags);
		}

		/// <summary>
		/// Imports all static methods of the specified type that is suitable
		/// to be added into the <see cref="FunctionCollection"/>.</summary>
		/// <param name="type">Type object.</param>
		/// <param name="nonpublic">Include non public
		/// member methods in the search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and
		/// the same arguments count as the function that is already
		/// in the collection (overload impossible).</exception>
		public void Import(Type type, bool nonpublic)
		{
			const BindingFlags Flags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			InternalImport(type,
				Flags | (nonpublic ? BindingFlags.NonPublic : 0));
		}

		/// <summary>
		/// Imports all static methods of the specified types that is suitable
		/// to be added into the <see cref="FunctionCollection"/>.</summary>
		/// <param name="types">Array of <see cref="Type"/> objects.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="types"/> is null.</exception><br>-or-</br>
		/// Some Type of <paramref name="types"/> is null.
		/// <exception cref="ArgumentException">
		/// Some of importing methods has the same name and
		/// the same arguments count as the function that is already
		/// in the collection (overload impossible).</exception>
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

		/// <summary>
		/// Adds the static method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// in the <see cref="FunctionCollection"/> with the function name,
		/// taken from real method name.</summary>
		/// <param name="methodName">
		/// Type's method name to be imported.</param>
		/// <param name="type">Type object.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="type"/> is null.<br/>-or-<br/>
		/// <paramref name="methodName"/>is null.</exception>
		/// <exception cref="ArgumentException">Method with
		/// <paramref name="methodName"/> is not founded.<br/>-or-<br/>
		/// Founded method is not valid to be added
		/// into this <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name
		/// and the same arguments count already exist
		/// in the collection (overload impossible).</exception>
		/// <exception cref="System.Reflection.AmbiguousMatchException">
		/// <paramref name="type"/> contains more than one methods
		/// matching the specified <paramref name="methodName"/>.</exception>
		public void Import(string methodName, Type type)
		{
			var method = FunctionFactory.TryResolve(type, methodName, -1);

			AddFunc(FunctionFactory.FromReflection(method, null, true));
		}

		/// <summary>
		/// Adds the static method reflection taken from the specified
		/// <paramref name="type"/> by the <paramref name="methodName"/>
		/// and arguments count to the <see cref="FunctionCollection"/>
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
		/// to the <see cref="FunctionCollection"/>.<br/>-or-<br/>
		/// <see cref="FunctionItem"/> with same name and the same arguments count
		/// already exist in the collection (overload impossible).</exception>
		public void Import(string methodName, Type type, int parametersCount)
		{
			if (parametersCount < 0)
				throw new ArgumentOutOfRangeException("parametersCount");

			var method = FunctionFactory.TryResolve(
				type, methodName, parametersCount);

			AddFunc(FunctionFactory.FromReflection(method, null, true));
		}

		#endregion

		/// <summary>
		/// Removes the function specified by name 
		/// from the <see cref="FunctionCollection"/>.</summary>
		/// <overloads>
		/// Removes the function from the 
		/// <see cref="FunctionCollection"/>.</overloads>
		/// <param name="name">The function name to be removed.</param>
		/// <returns><b>true</b> if function is successfully removed;
		/// otherwise, <b>false</b>.</returns>
		public bool Remove(string name)
		{
			int index = this.namesList.IndexOf(name);
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
		/// and params arguments usage from the <see cref="FunctionCollection"/>.
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
				this.funcsList[index]
					.Remove(argsCount, hasParamArray))
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
		/// Determines whether the <see cref="FunctionCollection"/>
		/// contains the specified name.</summary>
		/// <param name="name">Function name to locate
		/// in the <see cref="FunctionCollection"/>.</param>
		/// <returns><b>true</b> if name is found in the list;
		/// otherwise, <b>false</b>.</returns>
		public bool ContainsName(string name)
		{
			return this.namesList.Contains(name);
		}

		/// <summary>
		/// Removes all functions from the <see cref="FunctionCollection"/>.
		/// </summary>
		public void Clear()
		{
			this.namesList.Clear();
			this.funcsList.Clear();
		}

		// TODO: maybe implement? =)
		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through the pairs
		/// in the <see cref="FunctionCollection"/>.</summary>
		/// <returns>An enumerator object for pair items
		/// in the <see cref="FunctionCollection"/>.</returns>
		IEnumerator<FuncPair> IEnumerable<FuncPair>.GetEnumerator()
		{
			for (int i = 0; i < this.namesList.Count; i++)
			{
				yield return new FuncPair(
					this.namesList[i], this.funcsList[i]);
			}

			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<FuncPair>)this).GetEnumerator();
		}

		#endregion
		#region Internals

		private void AddFunc(string name, FunctionItem function)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			Debug.Assert(function != null);
			Debug.Assert(name != null);

			Validate.Name(name);

			int index = this.namesList.IndexOf(name);
			if (index >= 0)
			{
				if (!this.funcsList[index].Append(function))
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

		private void AddFunc(FunctionItem function)
		{
			// TryResolve name from real method name
			AddFunc(function.MethodName, function);
		}

		private void InternalImport(Type type, BindingFlags flags)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			foreach (var method in type.GetMethods(flags))
			{
				var func = FunctionFactory.FromReflection(method, null, false);
				if (func != null) AddFunc(func);
			}
		}

		private static ArgumentException OverloadImpossible(FunctionItem func)
		{
			return new ArgumentException(string.Format(
				Resource.errOverloadImpossible, func.ArgsString));
		}

		List<string>.Enumerator IQuickEnumerable.GetEnumerator()
		{
			return this.namesList.GetEnumerator();
		}

		#endregion
		#region Debug View

		private sealed class FunctionDebugView
		{
			[DebuggerBrowsable(State.RootHidden)]
			private readonly ViewItem[] items;

			public FunctionDebugView(FunctionCollection list)
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