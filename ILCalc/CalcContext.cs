using System.Globalization;
using System.Diagnostics;
using System;

namespace ILCalc
	{
	using State = DebuggerBrowsableState;

	// TODO: Postfix writer for DEBUG

	/// <summary>
	/// Represents the expression context (arguments, constants and functions
	/// available to use in expression, parsing settings) and provides methods
	/// to compile, evaluate and validate expressions in runtime.<br/>
	/// This class cannot be inherited.</summary>
	/// <threadsafety instance="false"/>
	
	[Serializable]
	public sealed partial class CalcContext
		{
		#region Fields

		[DebuggerBrowsable(State.Never)] internal ArgumentCollection _args;
		[DebuggerBrowsable(State.Never)] internal ConstantCollection _consts;
		[DebuggerBrowsable(State.Never)] internal FunctionCollection _funcs;
		[DebuggerBrowsable(State.Never)] internal CultureInfo _culture;
		[DebuggerBrowsable(State.Never)] internal OptimizeModes _optimize;
		[DebuggerBrowsable(State.Never)] internal bool _ignoreCase = true;
		[DebuggerBrowsable(State.Never)] internal bool _checked;
		[DebuggerBrowsable(State.Never), NonSerialized]
		private Parser _parser;

		#endregion
		#region Properties

		/// <summary>Gets or sets <see cref="ArgumentCollection"/> available
		/// for use in the expression.</summary>
		public ArgumentCollection Arguments
			{
			[DebuggerHidden]
			get	{
				if(_args == null) _args = new ArgumentCollection();
				return _args;
				}
			[DebuggerHidden]
			set {
				_args = value;
				if(_parser != null)
					{
					_parser.InitIdens();
					}
				}
			}

		/// <summary>Gets or sets <see cref="ConstantCollection"/> available
		/// for use in the expression.</summary>
		public ConstantCollection Constants
			{
			[DebuggerHidden]
			get	{
				if(_consts == null) _consts = new ConstantCollection();
				return _consts;
				}
			[DebuggerHidden]
			set {
				_consts = value;
				if(_parser != null) _parser.InitIdens();
				}
			}

		/// <summary>Gets or sets <see cref="FunctionCollection"/> available
		/// for use in the expression.</summary>
		public FunctionCollection Functions
			{
			[DebuggerHidden]
			get	{
				if(_funcs == null) _funcs = new FunctionCollection();
				return _funcs;
				}
			[DebuggerHidden]
			set {
				_funcs = value;
				if(_parser != null) _parser.InitIdens();
				}
			}

		/// <summary>Gets or sets <see cref="CultureInfo"/> instance
		/// used for expression parsing. Can be <c>null</c> for ignoring
		/// culture-sensitive characters and using ordinal compare for strings.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <paramref name="value"/> is neutral culture,
		/// that can't be used as parse culture.</exception>
		// TODO: fix debugger view
		[DebuggerDisplay("{(Сulture != null? Culture.DisplayName : \"Ordinal (null)\")}")]
		public CultureInfo Culture
			{
			[DebuggerHidden] get { return _culture; }
			[DebuggerHidden]
			set {
				if(value != null) CheckNeutral(value);
				_culture = value;
				if(_parser != null) _parser.InitCulture();
				}
			}

		/// <summary>Gets or sets ignore case mode
		/// for identifiers names in the expresion.</summary>
		/// <value><b>true</b> by default.</value>
		public bool IgnoreCase
			{
			[DebuggerHidden] get { return _ignoreCase; }
			[DebuggerHidden] set { _ignoreCase = value; }
			}
		
		/// <summary>Gets or sets checking mode for the expression evaluation.</summary>
		/// <remarks>Using this option will reduce perfomance of evaluation.</remarks>
		/// <value><b>false</b> by default.</value>
		public bool OverflowCheck
			{
			[DebuggerHidden] get { return _checked; }
			[DebuggerHidden] set { _checked = value; }
			}

		/// <summary>Gets or sets a bitwise OR combination of <see cref="OptimizeModes"/>
		/// enumeration values that specify optimization modes for expression.</summary>
		/// <remarks>Using this option will reduce perfomance of compilation.</remarks>
		/// <value><see cref="OptimizeModes.None"/> by default.</value>
		public OptimizeModes Optimization
			{
			[DebuggerHidden] get { return _optimize; }
			[DebuggerHidden] set { _optimize = value; }
			}

		#endregion
		#region Helpers

		[DebuggerHidden]
		private static void CheckNeutral( CultureInfo culture )
			{
			if(culture.IsNeutralCulture)
				{
				throw new NotSupportedException(
					string.Format(Resources.errNeutralCulture, culture.Name)
					);
				}
			}

		private void ExecuteParse( string expression, IExpressionOutput output )
			{
			if( _optimize == OptimizeModes.None )
				{
				_parser.Parse(expression, output);
				}
			else
				{
				var optimizer = new OptimizeOutput(output, _optimize);
				_parser.Parse(expression, optimizer);
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
		/// specified by <paramref name="arguments"/> parameter.</exception>
		/// <exception cref="ArithmeticException">Expression evaluation
		/// thrown the <see cref="ArithmeticException"/>.</exception>
		/// <returns>Evaluated value.</returns>
		public double Evaluate( string expression, params double[] arguments )
			{
			if( expression == null ) throw new ArgumentNullException("expression");
			if( arguments  == null ) throw new ArgumentNullException("arguments");

			int argCount = (_args == null)? 0: _args.Count;
			
			if( arguments.Length != argCount )
				{
				throw new ArgumentException(
					string.Format(
						Resources.errWrongArgsCount,
						arguments.Length, argCount )
					);
				}
			
			var intr = new QuickInterpret(arguments, _checked);

			if( _parser == null )
				_parser = new Parser(this);

			_parser.Parse(expression, intr);
			
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

			int argCount = (_args == null) ? 0 : _args.Count;

			var inter = new InterpretCreator( );

			if(	_parser == null )
				_parser = new Parser(this);

			ExecuteParse(expression, inter);
			
			return new Interpret(expression, argCount, _checked, inter);
			}

		/// <summary>Validates the specified <paramref name="expression"/>.</summary>
		/// <param name="expression">Expression to validate.</param>
		/// <exception cref="SyntaxException"><paramref name="expression"/> contains
		/// syntax error(s) and can't be compiled.</exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="expression"/> is null.</exception>
		public void Validate( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			var nil = new NullWriter();

			if( _parser == null )
				_parser = new Parser(this);

			_parser.Parse(expression, nil);
			}

#if DEBUG && NET20

		public string PostfixForm( string expression )
			{
			if( expression == null )
				throw new ArgumentNullException("expression");

			var postfix = new PostfixWriter(_args);

			if(	_parser == null )
				_parser = new Parser(this);

			ExecuteParse(expression, postfix);

			return postfix.ToString( );
			}

#endif

		#endregion
		#region Constructors

		/// <summary>Initializes a new instance of the ILCalc class
		/// that is contains empty expression context.</summary>
		[DebuggerHidden]
		public CalcContext( ) { }

		/// <summary>Initializes a new instance of the ILCalc class
		/// that is contains specified arguments.</summary>
		/// <param name="arguments">Arguments names.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="arguments"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some name of <paramref name="arguments"/> is not valid identifier name.<br/>-or-<br/>
		/// Some name of <paramref name="arguments"/> is already exist in the list.</exception>
		[DebuggerHidden]
		public CalcContext( params string[] arguments )
			{
			_args = new ArgumentCollection(arguments);
			}

		#endregion
		}
	}