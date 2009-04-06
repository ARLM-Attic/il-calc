using System.Collections.Generic;
using System;
using System.Globalization;

namespace ILCalc
	{
	sealed partial class Parser
		{
		#region Fields

		private readonly CalcContext _context;
		private IExpressionOutput _output;

		private string _expr;
		private int _len;

		private char _dot;
		private char _sep;

		private IEnumerable<SearchItem> _idens;
		private NumberFormatInfo _numf;
		
		#endregion
		#region Members

		public Parser( CalcContext context )
			{
			_context = context;
			
			InitCulture( );
			InitIdens( );
			}

		public void Parse( string expr, IExpressionOutput output )
			{
			_output = output;

			_expr = expr;
			_len  = expr.Length;

			_prePos = 0;
			_depth = 0;

			int i = 0;
			Parse(ref i, false);
			}

		public void InitCulture( )
			{
			CultureInfo culture = _context._culture;
			if( culture != null )
				{
				try {
					_dot = culture.NumberFormat.NumberDecimalSeparator[0];
					_sep = culture.TextInfo.ListSeparator[0];
					}
				catch( IndexOutOfRangeException )
					{
					throw new ArgumentException(Resources.errCultureExtract);
					}

				_numf = culture.NumberFormat;
				}
			else
				{
				_dot = '.';
				_sep = ',';
				_numf = new NumberFormatInfo();
				}
			}

		public void InitIdens( )
			{
			var list = new List< SearchItem >(4);
			
			if(_context._args != null)
				{
				list.Add(new SearchItem(Iden.Argument, _context._args));
				}

			if(_context._consts != null)
				{
				list.Add(new SearchItem(Iden.Constant, _context._consts.Keys));
				}

			if(_context._funcs != null)
				{
				list.Add(new SearchItem(Iden.Function, _context._funcs));
				}

			_idens = list;
			}

		#endregion
		#region Static Data

		// WARNING:
		// parser depends on the position
		// of the enumeration elements:
		private enum Item
			{
			Operator	= 0,
			Separator	= 1,
			Begin		= 2,
			Number		= 3,
			End			= 4,
			Identifier	= 5
			}
		
		const string _operators = "-+*/%^";

		static readonly int[] _prior = { 0, 0, 1, 1, 1, 3, 2 };

		#endregion
		}
	}
