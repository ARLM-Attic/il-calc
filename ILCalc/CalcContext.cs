using System.Globalization;
using System.Diagnostics;
using System;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	//TODO: ImportBuiltIn( ) => FunctionDictionary.GetBuiltIn( )
	//TODO: Syncronize Create methods (parser sync)?

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

		[DebuggerBrowsable(State.Never)] internal ArgumentCollection argsList;
		[DebuggerBrowsable(State.Never)] internal ConstantDictionary constDict;
		[DebuggerBrowsable(State.Never)] internal FunctionDictionary funcsDict;
		[DebuggerBrowsable(State.Never)] internal CultureInfo parseCulture;
		[DebuggerBrowsable(State.Never)] internal bool ignoreCase = true;

		[DebuggerBrowsable(State.Never)] private OptimizeModes optimizeMode;
		[DebuggerBrowsable(State.Never)] private bool checkedMode;
		[DebuggerBrowsable(State.Never), NonSerialized]
		private Parser parser;

		#endregion
		#region Properties

		/// <summary>Gets or sets <see cref="ArgumentCollection"/> available
		/// for use in the expression.</summary>
		public ArgumentCollection Arguments
			{
			[DebuggerHidden]
			get	{
				if( argsList == null ) argsList = new ArgumentCollection( );
				return argsList;
				}
			[DebuggerHidden]
			set {
				argsList = value;
				if( parser != null ) parser.InitIdens( );
				}
			}

		/// <summary>Gets or sets <see cref="ConstantDictionary"/> available
		/// for use in the expression.</summary>
		public ConstantDictionary Constants
			{
			[DebuggerHidden]
			get	{
				if( constDict == null ) constDict = new ConstantDictionary( );
				return constDict;
				}
			[DebuggerHidden]
			set {
				constDict = value;
				if( parser != null ) parser.InitIdens( );
				}
			}

		/// <summary>Gets or sets <see cref="FunctionDictionary"/> available
		/// for use in the expression.</summary>
		public FunctionDictionary Functions
			{
			[DebuggerHidden]
			get	{
				if( funcsDict == null ) funcsDict = new FunctionDictionary( );
				return funcsDict;
				}
			[DebuggerHidden]
			set {
				funcsDict = value;
				if( parser != null ) parser.InitIdens( );
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
			[DebuggerHidden] get { return parseCulture; }
			[DebuggerHidden]
			set {
				if( value != null ) CheckNeutral(value);
				parseCulture = value;
				if( parser != null ) parser.InitCulture();
				}
			}

		/// <summary>Gets or sets ignore case mode for
		/// identifiers names in the expresion.</summary>
		/// <value><b>true</b> by default.</value>
		public bool IgnoreCase
			{
			[DebuggerHidden] get { return ignoreCase; }
			[DebuggerHidden] set { ignoreCase = value; }
			}
		
		/// <summary>Gets or sets checking mode for the expression evaluation.</summary>
		/// <remarks>Using this option will reduce perfomance of evaluation.</remarks>
		/// <value><b>false</b> by default.</value>
		public bool OverflowCheck
			{
			[DebuggerHidden] get { return checkedMode; }
			[DebuggerHidden] set { checkedMode = value; }
			}

		/// <summary>Gets or sets a bitwise OR combination
		/// of <see cref="OptimizeModes"/> enumeration values
		/// that specify optimization modes for expression.</summary>
		/// <value><see cref="OptimizeModes.None"/> by default.</value>
		public OptimizeModes Optimization
			{
			[DebuggerHidden] get { return optimizeMode; }
			[DebuggerHidden] set { optimizeMode = value; }
			}

		#endregion
		#region Helpers

		[DebuggerHidden]
		private static void CheckNeutral( CultureInfo culture )
			{
			if( culture.IsNeutralCulture )
				{
				throw new NotSupportedException(
					string.Format(Resources.errNeutralCulture, culture.Name)
					);
				}
			}

		private static ArgumentException WrongArgsCount( int actual, int expected )
			{
			return new ArgumentException(
				string.Format(Resources.errWrongArgsCount, actual, expected)
				);
			}

		private void ExecuteParse( string expression, IExpressionOutput output )
			{
			if( optimizeMode == OptimizeModes.None )
				{
				parser.Parse(expression, output);
				}
			else
				{
				var optimizer = new OptimizeOutput(output, optimizeMode);
				parser.Parse(expression, optimizer);
				}
			}

		#endregion
		#region Methods

		/// <summary>Evaluates the given <paramref name="expression"/>
		/// using quick interpretation mode.</summary>
		/// <param name="expression">Expression to evaluate.</param>
		/// <param name="arguments">Expression arguments values.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s) and can't be evaluated.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.<br/>-or-<br/>
		/// <paramref name="arguments"/> is null.</exception>
		/// <exception cref="ArgumentException">Wrong arguments count was
		/// specified by the <paramref name="arguments"/> parameter.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Evaluated value.</returns>
		public double Evaluate( string expression, params double[] arguments )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			if( arguments == null )
				throw new ArgumentNullException("arguments");

			int argCount = (argsList == null)? 0: argsList.Count;
			if( arguments.Length != argCount )
				{
				throw WrongArgsCount(arguments.Length, argCount);
				}
			
			var intr = new QuickInterpret(arguments, checkedMode);

			if( parser == null )
				parser = new Parser(this);

			parser.Parse(expression, intr);
			
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
		public Interpret CreateInterpret( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			int argCount = (argsList == null)? 0: argsList.Count;

			var inter = new InterpretCreator( );

			if(	parser == null )
				parser = new Parser(this);

			ExecuteParse(expression, inter);
			
			return new Interpret(expression, argCount, checkedMode, inter);
			}

		/// <summary>Validates the specified <paramref name="expression"/>.</summary>
		/// <param name="expression">Expression to validate.</param>
		/// <exception cref="SyntaxException">
		/// <paramref name="expression"/> contains syntax error(s)</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		public void Validate( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			var nil = new NullWriter();

			if( parser == null )
				parser = new Parser(this);

			parser.Parse(expression, nil);
			}

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CalcContext"/>
		/// class that is contains empty expression context.</summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="CalcContext"/> class.
		/// </overloads>
		[DebuggerHidden]
		public CalcContext( ) { }

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
		public CalcContext( params string[] arguments )
			{
			argsList = new ArgumentCollection(arguments);
			}

		#endregion
		}
	}