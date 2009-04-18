using System.Diagnostics;
using System.Windows;
using System.Windows.Browser;

namespace SilverCalc
	{
	public partial class App
		{
		public App( )
			{
			Startup += Application_Startup;
			UnhandledException += Application_UnhandledException;

			InitializeComponent( );
			}

		#region Methods

		private void Application_Startup( object sender, StartupEventArgs e )
			{
			RootVisual = new Page( );
			}

		private void Application_UnhandledException(object sender,
		                                            ApplicationUnhandledExceptionEventArgs e )
			{
			if( !Debugger.IsAttached )
				{
				e.Handled = true;
				Deployment.Current.Dispatcher.BeginInvoke(( ) => ReportErrorToDOM(e));
				}
			}

		private void ReportErrorToDOM( ApplicationUnhandledExceptionEventArgs e )
			{
			try
				{
				string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
				errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

				HtmlPage.Window.Eval(
					"throw new Error(\"Unhandled Error in Silverlight 2 Application "
					+ errorMsg + "\");"
					);
				}
			catch { }
			}

		#endregion
		}
	}