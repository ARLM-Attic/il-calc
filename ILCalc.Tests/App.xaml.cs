using System.Windows;
using Microsoft.Silverlight.Testing;

namespace ILCalc.Tests
{
  public partial class App : Application
  {
    public App()
    {
      Startup += ApplicationStartup;
      UnhandledException += ApplicationUnhandledException;
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
      RootVisual = (UIElement)
        UnitTestSystem.CreateTestPage(this);
    }

    private static void ApplicationUnhandledException(
      object sender, ApplicationUnhandledExceptionEventArgs e)
    {
      if (!System.Diagnostics.Debugger.IsAttached)
      {
        e.Handled = true;
        Deployment.Current.Dispatcher
          .BeginInvoke(() => ReportErrorToDOM(e));
      }
    }

    private static void ReportErrorToDOM(
      ApplicationUnhandledExceptionEventArgs e)
    {
      try
      {
        string errorMsg =
          e.ExceptionObject.Message +
          e.ExceptionObject.StackTrace;

        errorMsg = errorMsg
          .Replace('"', '\'')
          .Replace("\r\n", @"\n");

        System.Windows.Browser.HtmlPage.Window.Eval(
          "throw new Error(\"Unhandled Error in Silverlight 2 Application "
          + errorMsg + "\");");
      }
      catch { }
    }
  }
}
