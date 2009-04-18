using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

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

	public sealed class Function
		{
		#region Fields

		[DebuggerBrowsable(State.Never)]
		private readonly int argsCount;
		[DebuggerBrowsable(State.Never)]
		private readonly bool hasParams;
		[DebuggerBrowsable(State.Never)]
		private readonly MethodInfo methodInfo;

		#endregion
		#region Properties

		/// <summary>Gets the function arguments count.</summary>
		public int ArgsCount
			{
			[DebuggerHidden] get { return argsCount; }
			}

		/// <summary>Indicates that function has an parameters array.</summary>
		public bool HasParamArray
			{
			[DebuggerHidden] get { return hasParams; }
			}

		/// <summary>Gets the method reflection that
		/// this <see cref="Function"/> represents.</summary>
		public MethodInfo Method
			{
			[DebuggerHidden] get { return methodInfo; }
			}

		/// <summary>Gets the method name.</summary>
		public string MethodName
			{
			[DebuggerHidden] get { return methodInfo.Name; }
			}

		/// <summary>
		/// Gets the method full name (including declaring type name).
		/// </summary>
		public string FullName
			{
			get
				{
				var buf = new StringBuilder( );

				buf.Append(methodInfo.DeclaringType.FullName);
				buf.Append('.');
				buf.Append(methodInfo.Name);

				return buf.ToString( );
				}
			}

		#endregion
		#region Methods

		/// <summary>
		/// Determine the ability of function to take
		/// specified <paramref name="count"/> of arguments.
		/// </summary>
		/// <param name="count">Arguments count.</param>
		/// <returns><b>true</b> if function can takes <paramref name="count"/>
		/// arguments; otherwise <b>false</b>.</returns>
		public bool CanTake( int count )
			{
			return HasParamArray?
				count >= ArgsCount:
				count == ArgsCount;
			}

		[DebuggerBrowsable(State.Never)]
		internal string ArgsString
			{
			get
				{
				return hasParams?
					argsCount.ToString( ) + '+':
					argsCount.ToString( );
				}
			}

		#endregion
		#region Constructor

		internal Function( MethodInfo methodInfo, int argsCount, bool hasParams )
			{
			this.methodInfo = methodInfo;
			this.argsCount = argsCount;
			this.hasParams = hasParams;
			}

		#endregion
		}
	}