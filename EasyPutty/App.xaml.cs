using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

using BLTools;
using BLTools.Debugging;

using static EasyPutty.Helpers;

namespace EasyPutty {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    public const string PARAM_LOGFILE = "log";
    public const string PARAM_LOGBASE = "logbase";
    public const string PARAM_CONFIG = "config";
    public const string PARAM_LOAD = "load";

    public static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;

    public static readonly string DEFAULT_PROD_LOGBASE = $@"c:\Logs\{AppName}";
    public static readonly string DEFAULT_DEV_LOGBASE = $@"c:\Logs\{AppName}";
    public const string DEFAULT_CONFIG = "config.xml";

    public static ISplitArgs AppArgs;
    public static NetworkCredential CurrentUserCredential;
    
    public static readonly string AppUsername = string.IsNullOrWhiteSpace(Environment.UserDomainName) ? Environment.UserName : $@"{Environment.UserDomainName}\{Environment.UserName}";

    public static bool AppIsStartingUp = true;

    private void Application_Exit(object sender, ExitEventArgs e) {
      ApplicationInfo.ApplicationStop();
    }

    private void Application_Startup(object sender, StartupEventArgs e) {
      AppArgs = ReadAppArgs();

      Trace.AutoFlush = true;

      SetLogDestination();
      ApplicationInfo.ApplicationStart();
      CurrentUserCredential = CredentialCache.DefaultNetworkCredentials;
    }

    public static string GetPictureFullname(string name = "default") => $"/{AppName};component/Pictures/{name}.png";

    public static void SetLogDestination() {
      string LogBase;
      if ( !System.Diagnostics.Debugger.IsAttached ) {
        LogBase = AppArgs.GetValue<string>(PARAM_LOGBASE, DEFAULT_PROD_LOGBASE);
      } else {
        LogBase = AppArgs.GetValue<string>(PARAM_LOGBASE, DEFAULT_DEV_LOGBASE);
      }

      string LogFile = AppArgs.GetValue<string>(PARAM_LOGFILE, "");

      Trace.Listeners.Clear();

      if ( LogFile != "" ) {
        Trace.Listeners.Add(new TimeStampTraceListener(Path.Combine(LogBase, LogFile), "default"));
      } else {
        string LogFilename = Path.Combine(LogBase, $"{Path.GetFileNameWithoutExtension(TraceFactory.GetTraceDefaultLogFilename())}.log");
        Trace.Listeners.Add(new TimeStampTraceListener(LogFilename,"default"));
      }
      //foreach ( TimeStampTraceListener TraceListenerItem in Trace.Listeners.OfType<TimeStampTraceListener>() ) {
      //  TraceListenerItem.DisplayUserId = true;
      //  TraceListenerItem.DisplayComputerName = true;
      //}
    }

    public static TimeStampTraceListener DefaultLog {
      get {
        return Trace.Listeners["default"] as TimeStampTraceListener;
      }
    }
  }
}
