using System.Collections.Generic;
using System;
using System.Globalization;

namespace ILCalc
	{
	sealed partial class Parser
		{
		#region Fields

		private readonly CalcContext context;
		private IExpressionOutput output;

		private string expr;
		private int exprLen;

		private char dotSymbol;
		private char sepSymbol;

		private IEnumerable<SearchItem> idenList;
		private NumberFormatInfo numFormat;
		
		#endregion
		#region Methods

		public Parser( CalcContext context )
			{
			this.context = context;
			
			InitCulture( );
			InitIdens( );
			}

		public void Parse( string expression, IExpressionOutput exprOutput )
			{
			output = exprOutput;
			
			expr = expression;
			exprLen = expression.Length;

			prePos = 0;
			exprDepth = 0;

			int i = 0;
			Parse(ref i, false);
			}

		public void InitCulture( )
			{
			CultureInfo culture = context.parseCulture;
			if( culture == null )
				{
				dotSymbol = '.';
				sepSymbol = ',';
				numFormat = new NumberFormatInfo( );
				}
			else
				{
				try
					{
					dotSymbol = culture.NumberFormat.NumberDecimalSeparator[0];
					sepSymbol = culture.TextInfo.ListSeparator[0];
					}
				catch( IndexOutOfRangeException )
					{
					throw new ArgumentException(Resources.errCultureExtract);
					}

				numFormat = culture.NumberFormat;
				}
			}

		public void InitIdens( )
			{
			var list = new List<SearchItem>(2);
			
			if(context.argsList != null)
				{
				list.Add(new SearchItem(IdenType.Argument,
					context.argsList));
				}

			if(context.constDict != null)
				{
				list.Add(new SearchItem(IdenType.Constant,
					context.constDict.Keys));
				}

			if(context.funcsDict != null)
				{
				list.Add(new SearchItem(IdenType.Function,
					context.funcsDict.Keys));
				}

			idenList = list;
			}

		#endregion
		#region Static Data

		/////////////////////////////////////////
		// WARNING: do not modify items order! //
		/////////////////////////////////////////
		private enum Item
			{
			Operator	= 0,
			Separator	= 1,
			Begin		= 2,
			Number		= 3,
			End			= 4,
			Identifier	= 5
			}
		
		const string operators = "-+*/%^";

		static readonly int[] opPriority = { 0, 0, 1, 1, 1, 3, 2 };

		#endregion
		}
	}
