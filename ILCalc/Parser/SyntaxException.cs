using System;
using System.Diagnostics;

namespace ILCalc
{
	using State = DebuggerBrowsableState;

	/// <summary>
	/// The exception that is thrown when syntax error
	/// occurs during expression parsing by <see cref="CalcContext"/>.<br/>
	/// This class cannot be inherited.
	/// </summary>
	[Serializable]

	public sealed class SyntaxException : Exception
	{
		#region Fields
		
		[DebuggerBrowsable(State.Never)] private readonly string exprString;
		[DebuggerBrowsable(State.Never)] private readonly int pos;
		[DebuggerBrowsable(State.Never)] private readonly int len;

		#endregion
		#region Constructors

		/// <summary>Initializes a new instance of the
		/// <see cref="SyntaxException"/> class.</summary>
		/// <overloads>Initializes a new instance of the
		/// <see cref="SyntaxException"/> class.</overloads>
		public SyntaxException()
		{
			this.exprString = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxException"/>
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public SyntaxException(string message) : base(message)
		{
			this.exprString = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxException"/>
		/// class with a specified error message and a reference to the
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception, or
		/// a <c>null</c> reference if no inner exception is specified.
		/// </param>
		public SyntaxException(string message, Exception innerException)
			: base(message, innerException)
		{
			this.exprString = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxException"/>
		/// class with a specified error message and information about
		/// the syntax error location in the expression.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="expression">Expression string.</param>
		/// <param name="pos">Position ot the error range.</param>
		/// <param name="len">Length of the error range.</param>
		internal SyntaxException(string message, string expression, int pos, int len)
			: base(message)
		{
			Debug.Assert(message != null);
			Debug.Assert(expression != null);
			Debug.Assert(expression.Length > pos && pos >= 0);
			Debug.Assert(expression.Length >= pos + len && len >= 0);

			this.exprString = expression;
			this.pos = pos;
			this.len = len;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxException"/>
		/// class with a specified error message, information about
		/// the syntax error location in the expression and a reference
		/// to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="expression">Expression string.</param>
		/// <param name="pos">Position ot the error range.</param>
		/// <param name="len">Length of the error range.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception, or
		/// a <c>null</c> reference if no inner exception is specified.
		/// </param>
		internal SyntaxException(
			string message, string expression, int pos, int len, Exception innerException)
			: base(message, innerException)
		{
			Debug.Assert(message != null);
			Debug.Assert(expression != null);
			Debug.Assert(expression.Length > pos && pos >= 0);
			Debug.Assert(expression.Length >= pos + len && len >= 0);

			this.exprString = expression;
			this.pos = pos;
			this.len = len;
		}

#if SERIALIZE

		private SyntaxException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

#endif

		#endregion
		#region Properties

		/// <summary>
		/// Gets the start position of the expression
		/// substring that thrown an exception.
		/// </summary>
		public int Position
		{
			get { return this.pos; }
		}

		/// <summary>
		/// Gets the length of expression substring that thrown an exception.
		/// </summary>
		public int Length
		{
			get { return this.len; }
		}

		/// <summary>
		/// Gets the full expression string that thrown an exception.
		/// </summary>
		public string Expression
		{
			get { return this.exprString; }
		}

		/// <summary>
		/// Gets the expression substring that thrown an exception.
		/// </summary>
		public string Substring
		{
			get { return this.exprString.Substring(this.pos, this.len); }
		}

		#endregion
	}
}