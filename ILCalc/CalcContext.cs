using System;
using System.Diagnostics;
using System.Globalization;

namespace ILCalc
{
	using State = DebuggerBrowsableState;
	using Browsable = DebuggerBrowsableAttribute;

	// TODO: ImportBuiltIn( ) => FunctionDictionary.GetBuiltIn( )
	// TODO: Syncronize Create methods (parser sync)?

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

		[Browsable(State.Never)] internal ArgumentCollection arguments;
		[Browsable(State.Never)] internal ConstantDictionary constants;
		[Browsable(State.Never)] internal FunctionDictionary functions;
		[Browsable(State.Never)] internal CultureInfo parseCulture;
		[Browsable(State.Never)] private OptimizeModes optimizeMode;
		[Browsable(State.Never)] internal bool implicitMul = true;
		[Browsable(State.Never)] internal bool ignoreCase = true;
		[Browsable(State.Never)] private bool checkedMode;
		[Browsable(State.Never)][NonSerialized]
		private Parser parser;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CalcContext"/>
		/// class that is contains empty expression context.</summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="CalcContext"/> class.
		/// </overloads>
		public CalcContext()
		{
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
		[DebuggerHidden]
		public CalcContext(params string[] arguments)
		{
			this.arguments = new ArgumentCollection(arguments);
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets <see cref="ArgumentCollection"/>
		/// available for use in the expression.</summary>
		public ArgumentCollection Arguments
		{
			[DebuggerHidden]
			get
			{
				if (this.arguments == null)
				{
					this.arguments = new ArgumentCollection();
				}

				return this.arguments;
			}

			[DebuggerHidden]
			set
			{
				this.arguments = value;
				if (this.parser != null)
				{
					this.parser.InitIdens();
				}
			}
		}

		/// <summary>Gets or sets <see cref="ConstantDictionary"/>
		/// available for use in the expression.</summary>
		public ConstantDictionary Constants
		{
			[DebuggerHidden]
			get
			{
				if (this.constants == null)
				{
					this.constants = new ConstantDictionary();
				}

				return this.constants;
				}

			[DebuggerHidden]
			set
			{
				this.constants = value;
				if (this.parser != null)
				{
					this.parser.InitIdens();
				}
			}
		}

		/// <summary>Gets or sets <see cref="FunctionDictionary"/>
		/// available for use in the expression.</summary>
		public FunctionDictionary Functions
		{
			[DebuggerHidden]
			get
			{
				if (this.functions == null)
				{
					this.functions = new FunctionDictionary();
				}

				return this.functions;
			}

			[DebuggerHidden]
			set
			{
				this.functions = value;
				if (this.parser != null)
				{
					this.parser.InitIdens();
				}
			}
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
			[DebuggerHidden]
			get
			{
				return this.parseCulture;
			}

			[DebuggerHidden]
			set
			{
				if (value != null)
				{
					CheckNeutral(value);
				}

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
			[DebuggerHidden] get { return this.ignoreCase; }
			[DebuggerHidden] set { this.ignoreCase = value; }
		}

		/// <summary>Gets or sets a value indicating whether implicit
		/// multiplication will be allowed in the expression.</summary>
		/// <value><b>true</b> by default.</value>
		public bool ImplicitMul
			{
			[DebuggerHidden] get { return this.implicitMul; }
			[DebuggerHidden] set { this.implicitMul = value; }
			}

		/// <summary>Gets or sets a value indicating whether arithmetic
		/// checks are enabled while the expression evaluation.</summary>
		/// <remarks>Using this option will reduce perfomance of evaluation.</remarks>
		/// <value><b>false</b> by default.</value>
		public bool OverflowCheck
		{
			[DebuggerHidden] get { return this.checkedMode; }
			[DebuggerHidden] set { this.checkedMode = value; }
		}

		/// <summary>Gets or sets a bitwise OR combination
		/// of <see cref="OptimizeModes"/> enumeration values
		/// that specify optimization modes for expression.</summary>
		/// <value><see cref="OptimizeModes.None"/> by default.</value>
		public OptimizeModes Optimization
		{
			[DebuggerHidden] get { return this.optimizeMode; }
			[DebuggerHidden] set { this.optimizeMode = value; }
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
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			if (arguments.Length != this.ArgsCount)
			{
				throw WrongArgsCount(arguments.Length, this.ArgsCount);
			}

			var intr = new QuickInterpret(arguments, this.checkedMode);
			this.ExecuteParse(expression, intr);
			
			return intr.Result;
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
			this.OptimizedParse(expression, creator);
			
			return new Interpret(expression, this.ArgsCount, this.checkedMode, creator);
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

			this.ExecuteParse(expression, new NullWriter());
			}

		#endregion
		#region Helpers

		[DebuggerHidden]
		private int ArgsCount
		{
			get
			{
				return this.arguments == null ? 0 : this.arguments.Count;
			}
		}

		[DebuggerHidden]
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

		private void ExecuteParse(string expression, IExpressionOutput output)
		{
			if (this.parser == null)
			{
				this.parser = new Parser(this);
			}

			this.parser.Parse(expression, output);
		}

		// TODO: away?
		private void OptimizedParse(string expression, IExpressionOutput output)
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