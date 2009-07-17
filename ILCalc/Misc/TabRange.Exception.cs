using System;

namespace ILCalc
{
	// NOTE: enum for extended info?
	// NOTE: override Message?

	/// <summary>
	/// The exception that is thrown when the <see cref="TabRange"/>
	/// instance validation is failed.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// Not available in the .NET CF / Silverlight versions.
	/// </remarks>
	[Serializable]
	public sealed class InvalidRangeException : Exception
	{
		#region Constructors

		/// <summary>Initializes a new instance of the
		/// <see cref="InvalidRangeException"/> class.</summary>
		/// <overloads>Initializes a new instance of the
		/// <see cref="InvalidRangeException"/> class.</overloads>
		public InvalidRangeException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRangeException"/>
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public InvalidRangeException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRangeException"/>
		/// class with a specified error message and a reference to the
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception, or
		/// a <c>null</c> reference if no inner exception is specified.
		/// </param>
		public InvalidRangeException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if SERIALIZE

		private InvalidRangeException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}

#endif

		#endregion
	}
}