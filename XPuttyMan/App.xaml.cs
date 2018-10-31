using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using BLTools;
using BLTools.Debugging;

namespace EasyPutty {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    public const string PARAM_LOGFILE = "log";
    public const string PARAM_LOGBASE = "logbase";
    public const string PARAM_CONFIG = "config";

    public const string DEFAULT_PROD_LOGBASE = @"c:\Logs";
    public const string DEFAULT_DEV_LOGBASE = @"c:\Logs";
    public const string DEFAULT_CONFIG = "config.xml";

    public static SplitArgs Args;
    public static NetworkCredential CurrentUserCredential;

    private void Application_Exit(object sender, ExitEventArgs e) {
      ApplicationInfo.ApplicationStop();
    }

    private void Application_Startup(object sender, StartupEventArgs e) {
      Args = Helpers.ReadAppArgs();

      Trace.AutoFlush = true;

      SetLogDestination();
      ApplicationInfo.ApplicationStart();
      CurrentUserCredential = CredentialCache.DefaultNetworkCredentials;
    }

    public static string GetPictureFullname(string name = "default") {
      return $"/xputtyman;component/Pictures/{name}.png";
    }

    public static void SetLogDestination() {
      string LogBase;
      if ( !System.Diagnostics.Debugger.IsAttached ) {
        LogBase = Args.GetValue<string>(PARAM_LOGBASE, DEFAULT_PROD_LOGBASE);
      } else {
        LogBase = Args.GetValue<string>(PARAM_LOGBASE, DEFAULT_DEV_LOGBASE);
      }

      string LogFile = Args.GetValue<string>(PARAM_LOGFILE, "");

      Trace.Listeners.Clear();

      if ( LogFile != "" ) {
        Trace.Listeners.Add(new TimeStampTraceListener(Path.Combine(LogBase, LogFile)));
      } else {
        string LogFilename = Path.Combine(LogBase, $"{Path.GetFileNameWithoutExtension(TraceFactory.GetTraceDefaultLogFilename())}.log");
        Trace.Listeners.Add(new TimeStampTraceListener(LogFilename));
      }
      foreach ( TimeStampTraceListener TraceListenerItem in Trace.Listeners.OfType<TimeStampTraceListener>() ) {
        TraceListenerItem.DisplayUserId = true;
        TraceListenerItem.DisplayComputerName = true;
      }
    }
  }
}
