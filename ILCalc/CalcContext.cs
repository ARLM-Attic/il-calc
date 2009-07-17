using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ILCalc
{
	using State = DebuggerBrowsableState;

	// TODO: away from here
	internal interface IQuickEnumerable
	{
		List<string>.Enumerator GetEnumerator();
	}

	// TODO: ImportBuiltIn( ) => FunctionCollection.GetBuiltIn( )
	// TODO: Syncronize Create methods (parser sync)?
	// TODO: try to make structs somewhere =)
	// TODO: extract comments from code

	// TODO: OnDeserialize fill namesList

	/// <summary>
	/// Represents the expression context (arguments, constants and functions
	/// available to use in expression, parsing settings) and provides methods
	/// to compile, evaluate and validate expressions in runtime.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <threadsafety instance="false"/>
	[Serializable]

	public sealed partial class CalcContext
	{
		#region Fields

		// TODO: check all usages for null comparsion

		// Imports collections
		[DebuggerBrowsable(State.Never)] private readonly ArgumentCollection arguments;
		[DebuggerBrowsable(State.Never)] private readonly ConstantDictionary constants;
		[DebuggerBrowsable(State.Never)] private readonly FunctionCollection functions;

		// Context settings
		[DebuggerBrowsable(State.Never)] private CultureInfo parseCulture;
		[DebuggerBrowsable(State.Never)] private OptimizeModes optimizeMode;
		[DebuggerBrowsable(State.Never)] private bool implicitMul = true;
		[DebuggerBrowsable(State.Never)] private bool ignoreCase = true;
		[DebuggerBrowsable(State.Never)] private bool checkedMode;

		// Literals lookup list
		[DebuggerBrowsable(State.Never), NonSerialized]
		private IQuickEnumerable[] literalsList;

		// Parser instance
		[DebuggerBrowsable(State.Never), NonSerialized]
		private Parser parser;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CalcContext"/>
		/// class that is contains empty expression context.</summary>
		/// <overloads>Initializes a new instance
		/// of the <see cref="CalcContext"/> class.</overloads>
		public CalcContext()
		{
			this.arguments = new ArgumentCollection();
			this.constants = new ConstantDictionary();
			this.functions = new FunctionCollection();
			this.literalsList = new IQuickEnumerable[]
			{
				this.arguments,
				this.constants,
				this.functions
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CalcContext"/>
		/// class that is contains specified arguments list.</summary>
		/// <param name="arguments">Arguments names.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="arguments"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="arguments"/> is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="arguments"/> is already exist in the list.</exception>
		public CalcContext(params string[] arguments) : this()
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			this.arguments.AddRange(arguments);
		}

		#endregion
		#region Properties

		/// <summary>Gets the <see cref="ArgumentCollection"/>
		/// available for use in the expression.</summary>
		public ArgumentCollection Arguments
		{
			get { return this.arguments; }
		}

		/// <summary>Gets the <see cref="ConstantDictionary"/>
		/// available for use in the expression.</summary>
		public ConstantDictionary Constants
		{
			get { return this.constants; }
		}

		/// <summary>Gets the <see cref="FunctionCollection"/>
		/// available for use in the expression.</summary>
		public FunctionCollection Functions
		{
			get { return this.functions; }
		}

		/// <summary>
		/// Gets or sets <see cref="CultureInfo"/> instance
		/// used for expression parsing.</summary>
		/// <exception cref="NotSupportedException"><paramref name="value"/>
		/// is neutral culture, that can't be used as parse culture.</exception>
		/// <remarks>
		/// Can be <c>null</c> for ignoring culture-sensitive
		/// characters and using ordinal compare for strings.</remarks>
		public CultureInfo Culture
		{
			get { return this.parseCulture; }
			set
			{
				if (value != null) CheckNeutral(value);

				this.parseCulture = value;
				if (this.parser != null)
				{
					this.parser.InitCulture();
				}
			}
		}

		/// <summary>Gets or sets a value indicating whether ignore case
		/// mode is will be used for identifiers names in the expresion.</summary>
		/// <value><b>true</b> by default.</value>
		public bool IgnoreCase
		{
			get { return this.ignoreCase; }
			set { this.ignoreCase = value; }
		}

		/// <summary>Gets or sets a value indicating whether implicit
		/// multiplication will be allowed in the expression.</summary>
		/// <value><b>true</b> by default.</value>
		public bool ImplicitMul
		{
			get { return this.implicitMul; }
			set { this.implicitMul = value; }
		}

		/// <summary>Gets or sets a value indicating whether arithmetic
		/// checks are enabled while the expression evaluation.</summary>
		/// <remarks>Using this option will reduce perfomance of evaluation.</remarks>
		/// <value><b>false</b> by default.</value>
		public bool OverflowCheck
		{
			get { return this.checkedMode; }
			set { this.checkedMode = value; }
		}

		/// <summary>Gets or sets a bitwise OR combination
		/// of <see cref="OptimizeModes"/> enumeration values
		/// that specify optimization modes for expression.</summary>
		/// <value><see cref="OptimizeModes.None"/> by default.</value>
		public OptimizeModes Optimization
		{
			get { return this.optimizeMode; }
			set { this.optimizeMode = value; }
		}

		internal IQuickEnumerable[] Literals
		{
			get { return this.literalsList; }
		}

		#endregion
		#region Methods

		/// <summary>
		/// Evaluates the given <paramref name="expression"/>
		/// using quick interpretation mode.</summary>
		/// <param name="expression">Expression to evaluate.</param>
		/// <param name="arguments">Expression arguments values.</param>
		/// <exception cref="SyntaxException"><paramref name="expression"/>
		/// contains syntax error(s) and can't be evaluated.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.<br/>-or-<br/>
		/// <paramref name="arguments"/> is null.</exception>
		/// <exception cref="ArgumentException">Wrong arguments count was
		/// specified by the <paramref name="arguments"/> parameter.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Evaluated value.</returns>
		public double Evaluate(string expression, params double[] arguments)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			if (arguments  == null) throw new ArgumentNullException("arguments");

			if (arguments.Length != ArgsCount)
				throw WrongArgsCount(arguments.Length, ArgsCount);

			var interp = new QuickInterpret(arguments, OverflowCheck);
			ParseSimple(expression, interp);

			return interp.Result;
		}

		/// <summary>Generates the <see cref="Interpret"/> object
		/// for evaluating the specified <paramref name="expression"/>.</summary>
		/// <param name="expression">
		/// Expression to create <see cref="Interpret"/> from.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s)
		/// and can't be used for <see cref="Interpret"/> creation.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		/// <returns><see cref="Interpret"/> object for evaluating expression.</returns>
		public Interpret CreateInterpret(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			var creator = new InterpretCreator();
			ParseOptimized(expression, creator);

			return new Interpret(expression, ArgsCount, OverflowCheck, creator);
		}

		/// <summary>Validates the specified <paramref name="expression"/>.</summary>
		/// <param name="expression">Expression to validate.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s)</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		public void Validate(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			ParseSimple(expression, new NullWriter());
		}

		#endregion
		#region Helpers

		private int ArgsCount
		{
			get { return this.arguments.Count; }
		}

		private static void CheckNeutral(CultureInfo culture)
		{
			if (culture.IsNeutralCulture)
			{
				throw new NotSupportedException(
					string.Format(Resource.errNeutralCulture, culture.Name));
			}
		}

		private static ArgumentException WrongArgsCount(int actual, int expected)
		{
			return new ArgumentException(
				string.Format(Resource.errWrongArgsCount, actual, expected));
		}

		private void ParseSimple(string expression, IExpressionOutput output)
		{
			if (this.parser == null)
			{
				this.parser = new Parser(this);
			}

			this.parser.Parse(expression, output);
		}

		private void ParseOptimized(string expression, IExpressionOutput output)
		{
			if (this.parser == null)
			{
				this.parser = new Parser(this);
			}

			if (this.optimizeMode == OptimizeModes.None)
			{
				this.parser.Parse(expression, output);
			}
			else
			{
				var optimizer = new OptimizeOutput(output, this.optimizeMode);
				this.parser.Parse(expression, optimizer);
			}
		}

		#endregion
	}
}