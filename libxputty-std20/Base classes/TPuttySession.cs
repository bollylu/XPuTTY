using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BLTools;
using Microsoft.Win32;
using static System.Management.ManagementObjectCollection;

namespace libxputty_std20 {
  public class TPuttySession : IPuttySession, IDisposable, IToXml {

    #region --- Constants --------------------------------------------
    internal const string REG_KEYNAME_BASE = @"Software\SimonTatham\PuTTY\Sessions";

    protected const string EXECUTABLE_PUTTY = "putty.exe";
    protected const string EXECUTABLE_PLINK = "plink.exe";
    protected const string EXECUTABLE_PSCP = "pscp.exe";

    protected const int WATCH_DELAY_MS = 200;
    protected const int NO_PROCESS_PID = -1;

    internal const string REG_PROTOCOL_TYPE = "Protocol";

    protected const string XML_THIS_ELEMENT = "Session";
    protected const string XML_ATTRIBUTE_NAME = "Name";
    protected const string XML_ATTRIBUTE_PROTOCOL = "Protocol";

    #endregion --- Constants --------------------------------------------

    #region --- Public properties ------------------------------------------------------------------------------
    public string Name { get; set; }
    public string CleanName => Name.Replace("%20", " ");
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();
    #endregion --- Public properties ---------------------------------------------------------------------------

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");
      return RetVal.ToString();
    }

    public virtual XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_PROTOCOL, Protocol);
      return RetVal;
    }

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    #endregion --- Event handlers --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySession() {
      Name = "<no name>";
    }
    public TPuttySession(string name) {
      Name = name;
    }

    public virtual void Dispose() {
      if ( WatchTask != null ) {
        WatchTaskCancellation.Cancel();
        WatchTask.Wait(WatchTaskCancellation.Token);
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Registry interactions --------------------------------------------
    public static RegistryKey GetRegistryKey(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.OpenSubKey(PuttySessionKeyName);
    }

    public static RegistryKey GetRegistryKeyRW(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.CreateSubKey(PuttySessionKeyName, true);
    }

    public static TPuttyProtocol GetSessionProtocolFromRegistry(string sessionKeyName = "") {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(sessionKeyName) ) {
        return TPuttyProtocol.Unknown;
      }
      #endregion === Validate parameters ===

      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{sessionKeyName}";
      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {
        return PuttySessionKey.GetValue(REG_PROTOCOL_TYPE, "") as string;
      }
    }

    public virtual IPuttySession LoadFromRegistry() {
      return this;
    }

    public virtual void SaveToRegistry() {

    }
    #endregion --- Registry interactions --------------------------------------------

    public static IPuttySession Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySession("<empty>");
        }
        return _Empty;
      }
    }
    private static IPuttySession _Empty;

    #region --- Windows processes --------------------------------------------
    public Process PuttyProcess { get; protected set; }
    public bool IsRunning => PuttyProcess != null;
    public int PID => PuttyProcess == null ? NO_PROCESS_PID : PuttyProcess.Id;
    public string CommandLine => PuttyProcess == null ? "" : GetCommandLine();

    private Task WatchTask;
    private CancellationTokenSource WatchTaskCancellation = new CancellationTokenSource();

    public virtual void Start() {
      if ( CheckIsRunning() ) {
        return;
      }

      PuttyProcess = new Process();
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = $"-load {"\"" + CleanName + "\""}"
      };
      Log.Write($"Loading {StartInfo.Arguments} ...");

      PuttyProcess.StartInfo = StartInfo;
      PuttyProcess.Start();

      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }

      WatchTask = Task.Run(async () => {
        while ( PID != NO_PROCESS_PID || WatchTaskCancellation.IsCancellationRequested ) {
          try {
            Process WatchedProcess = Process.GetProcessById(PID);
            await Task.Delay(WATCH_DELAY_MS);
          } catch {
            PuttyProcess.Dispose();
            PuttyProcess = null;
          }
        }
        if ( PuttyProcess != null && WatchTaskCancellation.IsCancellationRequested ) {
          PuttyProcess.Kill();
        }
        if ( OnExit != null ) {
          OnExit(this, EventArgs.Empty);
        }
      }, WatchTaskCancellation.Token);

    }

    public virtual void StartPlink() {
      if ( CheckIsRunning() ) {
        return;
      }

      PuttyProcess = new Process();
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = $"{"\"" + CleanName + "\""}",
        UseShellExecute = false
      };
      Log.Write($"Loading {StartInfo.Arguments} ...");

      PuttyProcess.StartInfo = StartInfo;
      PuttyProcess.Start();

      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }

      WatchTask = Task.Run(async () => {
        while ( PID != NO_PROCESS_PID || WatchTaskCancellation.IsCancellationRequested ) {
          try {
            Process WatchedProcess = Process.GetProcessById(PID);
            await Task.Delay(WATCH_DELAY_MS);
          } catch {
            PuttyProcess.Dispose();
            PuttyProcess = null;
          }
        }
        if ( PuttyProcess != null && WatchTaskCancellation.IsCancellationRequested ) {
          PuttyProcess.Kill();
        }
        if ( OnExit != null ) {
          OnExit(this, EventArgs.Empty);
        }
      }, WatchTaskCancellation.Token);

    }
    public virtual void Stop() { }

    public bool CheckIsRunning() {
      return Process.GetProcesses().Any(x => x.Id == PID);
    }

    public string GetCommandLine() {
      using ( ManagementObjectSearcher WmiSearch = new ManagementObjectSearcher($"SELECT Name,CommandLine,ProcessId FROM Win32_Process WHERE ProcessId={PID}") ) {
        ManagementObjectCollection Processes = WmiSearch.Get();
        ManagementObjectEnumerator managementObjectEnumerator = Processes.GetEnumerator();
        managementObjectEnumerator.Reset();
        if ( managementObjectEnumerator.MoveNext() ) {
          return managementObjectEnumerator.Current["CommandLine"].ToString();
        }
        return "";
      }
    }

    public void SetRunningProcess(Process process) {
      PuttyProcess = process;
    }

    public void SetRunningProcess(IPuttySession puttySession) {
      PuttyProcess = puttySession.PuttyProcess;
    }

    public static IEnumerable<IPuttySession> GetAllRunningSessions() {
      using ( ManagementObjectSearcher WmiSearch = new ManagementObjectSearcher($"SELECT Name,CommandLine,ProcessId FROM Win32_Process WHERE Name=\"{EXECUTABLE_PUTTY}\"") ) {
        foreach ( ManagementObject ProcessItem in WmiSearch.Get() ) {
          TPuttySession RetVal = new TPuttySession(ProcessItem["CommandLine"].ToString());
          RetVal.SetRunningProcess(Process.GetProcessById(Convert.ToInt32(ProcessItem["ProcessId"])));
          yield return RetVal;
        }
      }
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowText(IntPtr hWnd, string windowName);

    protected void SetProcessTitle(string title = "") {
      if ( PuttyProcess != null ) {
        IntPtr handle = PuttyProcess.MainWindowHandle;
        SetWindowText(handle, title);
      }
    }

    #endregion --- Windows processes -------------------------------------------- 
  }
}
