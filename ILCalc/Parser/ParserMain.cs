using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ILCalc
{
	internal sealed partial class Parser
	{
		#region Fields

		private readonly CalcContext context;
		private IExpressionOutput output;

		private string expr;
		private int exprLen;

		private char dotSymbol;
		private char sepSymbol;

		private List<SearchItem> idenList;
		private NumberFormatInfo numFormat;
		
		#endregion
		#region Methods

		public Parser(CalcContext context)
		{
			Debug.Assert(context != null);

			this.context = context;
			
			this.InitCulture();
			this.InitIdens();
		}

		// TODO: put optimizer logic here
		public void Parse(string expression, IExpressionOutput exprOutput)
		{
			Debug.Assert(expression != null);
			Debug.Assert(exprOutput != null);

			this.expr = expression;
			this.exprLen = expression.Length;
			this.output = exprOutput;
			this.exprDepth = 0;
			this.prePos = 0;

			int i = 0;
			this.Parse(ref i, false);
		}

		public void InitCulture()
		{
			CultureInfo culture = this.context.parseCulture;
			if (culture == null)
			{
				this.dotSymbol = '.';
				this.sepSymbol = ',';
				this.numFormat = new NumberFormatInfo();
			}
			else
			{
				try
				{
					this.dotSymbol = culture.NumberFormat.NumberDecimalSeparator[0];
					this.sepSymbol = culture.TextInfo.ListSeparator[0];
				}
				catch (IndexOutOfRangeException)
				{
					throw new ArgumentException(Resource.errCultureExtract);
				}

				this.numFormat = culture.NumberFormat;
			}
		}

		public void InitIdens()
		{
			var list = new List<SearchItem>(2);

			if (this.context.arguments != null)
			{
				list.Add(new SearchItem(
					IdenType.Argument, this.context.arguments));
			}

			if (this.context.constants != null)
			{
				list.Add(new SearchItem(
					IdenType.Constant, this.context.constants.Keys));
			}

			if (this.context.functions != null)
			{
				list.Add(new SearchItem(
					IdenType.Function, this.context.functions.Keys));
			}

			this.idenList = list;
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
		
		private const string Operators = "-+*/%^";

		private static readonly int[] Priority = { 0, 0, 1, 1, 1, 3, 2 };

		#endregion
	}
}
