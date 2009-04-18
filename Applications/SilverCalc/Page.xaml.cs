using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ILCalc;

namespace SilverCalc
	{
	public partial class Page
		{
		#region Fields

		private readonly List<string> exprList = new List<string>( );
		private readonly CalcContext calc;
		private int listPos = -1;

		#endregion
		#region Constructor

		public Page( )
			{
			InitializeComponent( );

			HideInfo .Completed += delegate {  infoPanel.Visibility = Visibility.Collapsed; };
			HideError.Completed += delegate { errorPanel.Visibility = Visibility.Collapsed; };

			calc = new CalcContext();
			calc.Culture = CultureInfo.CurrentCulture;
			calc.Functions.ImportBuiltIn( );
			calc.Constants.ImportBuiltIn( );
			}

		#endregion
		#region Event Handlers

		private void launchEvaluate_Click( object sender, RoutedEventArgs e )
			{
			if( infoPanel.Visibility == Visibility.Visible )
				{
				console.IsEnabled = true;
				HideInfo.Begin( );
				}

			string expr = expressionBox.Text;
			double res;
			try
				{
				res = calc.Evaluate(expr);
				}
			catch( SyntaxException err )
				{
				expressionBox.Select(err.Position, err.Length);
				errorText.Text = err.Message;
				
				if( errorPanel.Visibility == Visibility.Collapsed )
					{
					errorPanel.Visibility = Visibility.Visible;
					ShowError.Begin( );
					}
				
				return;
				}

			if( errorPanel.Visibility == Visibility.Visible )
				{
				HideError.Begin( );
				}

			var buf = new StringBuilder( );
			
			buf.Append(expressionBox.Text);
			buf.Append(" = ");
			buf.Append(res);
			buf.AppendLine( );

			console.Text += buf.ToString( );
			console.Select(console.Text.Length - 1, 0);

			expressionBox.Text = "";
			expressionBox.Focus( );
			
			exprList.Add(expr);
			listPos = exprList.Count;
			}

		private void expressionBox_KeyDown( object sender, KeyEventArgs e )
			{
			if( e.Key == Key.Enter )
				{
				launchEvaluate_Click(null, null);
				}
			else if( exprList.Count != 0 )
				{
				if( e.Key == Key.Up )
					{
					if( --listPos < 0 )
						listPos = exprList.Count - 1;
					expressionBox.Text = exprList[listPos];
					}
				else if( e.Key == Key.Down )
					{
					if( ++listPos >= exprList.Count )
						listPos = 0;
					expressionBox.Text = exprList[listPos];
					}
				}
			}

		private void consoleClear_Click( object sender, RoutedEventArgs e )
			{
			if( infoPanel.Visibility == Visibility.Visible )
				{
				console.IsEnabled = true;
				HideInfo.Begin( );
				}

			console.Text = "";
			exprList.Clear( );
			expressionBox.Focus( );
			}

		private void listFunctions_Click( object sender, RoutedEventArgs e )
			{
			ListMembers("Available functions:", calc.Functions.Keys);
			}

		private void listConstants_Click( object sender, RoutedEventArgs e )
			{
			ListMembers("Available constants:", calc.Constants.Keys);
			}

		#endregion
		#region Methods

		private void ListMembers( string str, IEnumerable<string> names )
			{
			var buf = new StringBuilder( );
			buf.AppendLine(str);

			bool comma = false;
			foreach( string name in names )
				{
				if( comma ) buf.Append(", ");
				else comma = true;

				buf.Append(name);
				}

			lbInfoText.Text = buf.ToString( );

			if( infoPanel.Visibility == Visibility.Collapsed )
				{
				console.IsEnabled = false;
				ShowInfo.Begin( );
				infoPanel.Visibility = Visibility.Visible;
				}
			}

		#endregion
		}
	}
