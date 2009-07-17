using System;
using System.Diagnostics;
using System.Globalization;

namespace ILCalc
{
	// TODO: Parser should not know about CalcContext
	// TODO: Trigger to use|not use output

	internal sealed partial class Parser
	{
		#region Fields

		private readonly CalcContext context;
		private IExpressionOutput output;

		private string expr;
		private int xlen;

		private char dotSymbol;
		private char sepSymbol;

		private NumberFormatInfo numFormat;
		
		#endregion
		#region Methods

		public Parser(CalcContext context)
		{
			Debug.Assert(context != null);

			this.context = context;
			InitCulture();
		}

		private CalcContext Context      { get { return this.context; } }
		private IExpressionOutput Output { get { return this.output;  } }

		public void Parse(string expression, IExpressionOutput exprOutput)
		{
			Debug.Assert(expression != null);
			Debug.Assert(exprOutput != null);

			this.expr = expression;
			this.xlen = expression.Length;
			this.output = exprOutput;
			this.exprDepth = 0;
			this.prePos = 0;

			int i = 0;
			Parse(ref i, false);
		}

		public void InitCulture()
		{
			CultureInfo culture = Context.Culture;
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
