using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	/// <summary>
	/// Represents the imported function that has
	/// ability to be used in some expression.
	/// </summary>
	/// <remarks>
	/// Instance of this class is an immutable.<br/>
	/// Class has no public constructors.</remarks>
	/// <threadsafety instance="true"/>
	[DebuggerDisplay("{ArgsCount} args", Name = "{Method}")]
	[Serializable]

	// TODO: derived for targeting functions?

	public sealed class FunctionItem
	{
		#region Fields

		[Browsable(State.Never)] private readonly int fixCount;
		[Browsable(State.Never)] private readonly bool hasParams;
		[Browsable(State.Never)] private readonly MethodInfo method;
		[Browsable(State.Never)] private readonly object target;

		#endregion
		#region Constructor

		internal FunctionItem(
			MethodInfo method, object target, int argsCount, bool hasParams)
		{
			Debug.Assert(method != null);
			Debug.Assert(method.IsStatic == (target == null));
			Debug.Assert(argsCount >= 0);

			this.hasParams = hasParams;
			this.fixCount = argsCount;
			this.method = method;
			this.target = target;
		}

		#endregion
		#region Properties

		/// <summary>Gets the function arguments count.</summary>
		public int ArgsCount
		{
			get { return this.fixCount; }
		}

		/// <summary>Gets a value indicating whether
		/// function has an parameters array.</summary>
		public bool HasParamArray
		{
			get { return this.hasParams; }
		}

		/// <summary>Gets the method reflection this
		/// <see cref="FunctionItem"/> represents.</summary>
		public MethodInfo Method
		{
			get { return this.method; }
		}

		/// <summary>Gets the method name.</summary>
		public string MethodName
		{
			get { return this.method.Name; }
		}

		/// <summary>Gets the method full name
		/// (including declaring type name).</summary>
		public string FullName
		{
			get
			{
				var buf = new StringBuilder();

				buf.Append(this.method.DeclaringType.FullName);
				buf.Append('.');
				buf.Append(this.method.Name);

				return buf.ToString();
			}
		}

		/// <summary>
		/// Gets the method target for instance methods.
		/// For static methods this property will return null.
		/// </summary>
		public object Target
		{
			get { return this.target; }
		}

		[Browsable(State.Never)]
		internal string ArgsString
		{
			get
			{
				return this.HasParamArray ?
					this.fixCount.ToString() + '+' :
					this.fixCount.ToString();
			}
		}

		#endregion
		#region Methods

		/// <summary>
		/// Determine the ability of function to take specified
		/// <paramref name="count"/> of arguments.</summary>
		/// <param name="count">Arguments count.</param>
		/// <returns><b>true</b> if function can takes
		/// <paramref name="count"/> arguments;
		/// otherwise <b>false</b>.</returns>
		public bool CanTake(int count)
		{
			return count >= 0
				&& this.HasParamArray ?
				count >= this.fixCount :
				count == this.fixCount;
		}

		/// <summary>
		/// Invokes this <see cref="FunctionItem"/> via reflection.</summary>
		/// <param name="arguments">Function arguments.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="arguments"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="arguments"/> doesn't contains
		/// needed arguments count.</exception>
		/// <returns>Returned value.</returns>
		public double Evaluate(params double[] arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			if (!this.CanTake(arguments.Length))
			{
				throw new ArgumentException(string.Format(
					Resource.errWrongArgsCount,
					arguments.Length,
					this.ArgsString));
			}

			return this.Invoke(arguments, arguments.Length);
		}

		#endregion
		#region Invoke

		internal double Invoke(double[] array, int argsCount)
		{
			return this.Invoke(array, argsCount - 1, argsCount);
		}

		internal double Invoke(double[] stack, int pos, int argsCount)
		{
			Debug.Assert(stack != null);
			Debug.Assert(stack.Length > pos);
			Debug.Assert(stack.Length >= argsCount);

			object[] fixArgs;

			if (this.hasParams)
			{
				int varCount = argsCount - this.fixCount;
				var varArgs  = new double[varCount];

				// TODO: perform loop, based on varArgs.Length

				// fill params array:
				for (int i = varCount - 1; i >= 0; i--)
				{
					varArgs[i] = stack[pos--];
				}

				fixArgs = new object[this.fixCount + 1];
				fixArgs[this.fixCount] = varArgs;
			}
			else
			{
				fixArgs = new object[this.fixCount];
			}

			// fill arguments array:
			for (int i = this.fixCount - 1; i >= 0; i--)
			{
				fixArgs[i] = stack[pos--];
			}

			// invoke via reflection
			return (double)
				this.method.Invoke(this.target, fixArgs);
		}

		internal double Invoke(object[] fixedArgs)
		{
			Debug.Assert(fixedArgs != null);

			return (double)
				this.method.Invoke(this.target, fixedArgs);
		}

		#endregion
	}
}