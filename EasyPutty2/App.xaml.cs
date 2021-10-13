using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

using BLTools;
using BLTools.Debugging;
using BLTools.Diagnostic.Logging;

using static EasyPutty.Helpers;

namespace EasyPutty {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ILoggable {

    public const string PARAM_LOGFILE = "log";
    public const string PARAM_LOGBASE = "logbase";
    public const string PARAM_CONFIG = "config";
    public const string PARAM_LOAD = "load";

    public static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;

    public static readonly string DEFAULT_PROD_LOGBASE = $@"c:\Logs\{AppName}";
    public static readonly string DEFAULT_DEV_LOGBASE = $@"c:\Logs\{AppName}";
    public const string DEFAULT_CONFIG = "config.xml";

    public static ISplitArgs AppArgs { get; private set; }
    public static NetworkCredential CurrentUserCredential { get; private set; }

    public static readonly string AppUsername = string.IsNullOrWhiteSpace(Environment.UserDomainName) ? Environment.UserName : $@"{Environment.UserDomainName}\{Environment.UserName}";

    public static bool AppIsStartingUp { get; private set; } = true;

    public ILogger Logger { get; set; }

    private void Application_Exit(object sender, ExitEventArgs e) {
      ApplicationInfo.ApplicationStop();
    }

    private void Application_Startup(object sender, StartupEventArgs e) {
      AppArgs = new SplitArgs();
      AppArgs.Parse(Environment.GetCommandLineArgs());

      Trace.AutoFlush = true;

      GetLogDestination();
      ApplicationInfo.ApplicationStart(Logger);
      CurrentUserCredential = CredentialCache.DefaultNetworkCredentials;
    }

    public static string GetPictureFullname(string name = "default") => $"/{AppName};component/Pictures/{name}.png";

    public void GetLogDestination() {
      string LogBase = !System.Diagnostics.Debugger.IsAttached
          ? AppArgs.GetValue(PARAM_LOGBASE, DEFAULT_PROD_LOGBASE)
          : AppArgs.GetValue(PARAM_LOGBASE, DEFAULT_DEV_LOGBASE);

      string LogFile = AppArgs.GetValue(PARAM_LOGFILE, "");

      string FullLogFileName = LogFile != ""
          ? Path.Combine(LogBase, LogFile)
          : Path.Combine(LogBase, $"{Path.GetFileNameWithoutExtension(TraceFactory.GetTraceDefaultLogFilename())}.log");

      SetLogger(new TFileLogger(FullLogFileName));
    }

    public void SetLogger(ILogger logger) {
      if (logger is null) {
        Logger = new TTraceLogger();
      } else {
        Logger = ALogger.Create(logger);
      }
    }
  }
}
