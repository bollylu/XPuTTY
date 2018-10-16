using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BLTools;
using BLTools.Debugging;

namespace XPuttyMan {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private void Application_Exit(object sender, ExitEventArgs e) {
      ApplicationInfo.ApplicationStop();
    }

    private void Application_Startup(object sender, StartupEventArgs e) {
      SplitArgs Args = new SplitArgs(Environment.GetCommandLineArgs());

      Trace.AutoFlush = true;

      TraceFactory.DefaultLogLocation = @"c:\logs\xputtyman";
      TraceFactory.AddTraceDefaultLogFilename();
      ApplicationInfo.ApplicationStart();
    }

    public static string GetPictureFullname(string name = "default") {
      return $"/xputtyman;component/Pictures/{name}.png";
    }
  }
}
