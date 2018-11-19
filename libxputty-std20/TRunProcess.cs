using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

using BLTools;

using libxputty_std20.DllImports;

using static System.Management.ManagementObjectCollection;
using static libxputty_std20.DllImports.Kernel32dll;

namespace libxputty_std20 {
  public class TRunProcess {

#if DEBUG
    public static bool IsDebug = true;
#else
    public static bool IsDebug = false;
#endif

    #region --- Constants --------------------------------------------
    protected const int WATCH_DELAY_MS = 200;
    protected const int NO_PROCESS_PID = -1;
    #endregion --- Constants --------------------------------------------

    public bool IsRunning => _Process != null && !_Process.HasExited;
    public int PID => _Process == null ? NO_PROCESS_PID : _Process.Id;
    public ProcessStartInfo StartInfo { get; set; }
    public bool IsRedirected { get; private set; }

    protected Process _Process;

    private Task Watcher;
    private CancellationTokenSource WatcherCancellation = new CancellationTokenSource();

    public string ProcessTitle;
    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TRunProcess() {
      StartInfo = new ProcessStartInfo();
    }

    public TRunProcess(ProcessStartInfo startInfo) {
      StartInfo = startInfo;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    #endregion --- Event handlers --------------------------------------------

    public void Start(bool redirect = false) {
      if ( IsRunning ) {
        return;
      }

      IsRedirected = redirect;

      _Process = new Process();
      if ( IsDebug ) {
        Log.Write($"Process = {StartInfo.FileName}, Args = {StartInfo.Arguments}");
      }

      if ( IsRedirected ) {
        StartInfo.RedirectStandardError = true;
        StartInfo.RedirectStandardOutput = true;
      }
      _Process.StartInfo = StartInfo;
      if ( IsRedirected ) {
        _Process.ErrorDataReceived += _Process_ErrorDataReceived;
        _Process.OutputDataReceived += _Process_OutputDataReceived;
      }
      _Process.Start();
      if ( IsRedirected ) {
        _Process.BeginErrorReadLine();
        _Process.BeginOutputReadLine();
      }

      //SetConsoleColor(_Process, Color.Black, Color.White);
      

      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }

      _StartWatcher();

      Log.Write("Start completed.");
    }

    private void _Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
      Log.Write(e.Data, ErrorLevel.Info);
    }

    private void _Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
      if ( !string.IsNullOrEmpty(e.Data) ) {
        Log.Write(e.Data, ErrorLevel.Error);
      }
    }

    protected void _StartWatcher() {
      if ( Watcher == null || Watcher.IsCanceled || Watcher.IsCompleted ) {
        Log.Write("Starting the watcher...");
        Watcher = Task.Run(async () => {
          while ( !_Process.HasExited || WatcherCancellation.IsCancellationRequested ) {
            try {
              SetProcessTitle(_Process, ProcessTitle);
              await Task.Delay(WATCH_DELAY_MS);
              _Process.Refresh();
            } catch {
              if ( IsRedirected ) {
                _Process.ErrorDataReceived -= _Process_ErrorDataReceived;
                _Process.OutputDataReceived -= _Process_OutputDataReceived;
              }
              _Process.Dispose();
              _Process = null;
            }
          }
          if ( _Process != null && WatcherCancellation.IsCancellationRequested ) {
            Log.Write("Killing process");
            _Process.Kill();
          }
          if ( OnExit != null ) {
            OnExit(this, EventArgs.Empty);
          }
        }, WatcherCancellation.Token);
      }
    }

    public void Cancel() {
      Log.Write("Cancelling the process...");
      WatcherCancellation.Cancel();
    }

    public void AssignProcess(Process process) {
      Log.Write($"Attempt to assign process to TRunProcess");
      if ( process == null ) {
        return;
      }
      try {
        _Process = process;
        _StartWatcher();
        Log.Write("Process assigned.");
      } catch ( Exception ex ) {
        Log.Write($"Unable to assign process : {ex.Message}");
      }
    }
    public void AssignProcessByPID(int pid) {
      Log.Write($"Attempt to assign process with PID {pid} to TRunProcess {StartInfo.FileName} {StartInfo.Arguments}");
      if ( pid <= 0 ) {
        return;
      }
      try {
        _Process = Process.GetProcessById(pid);
        _StartWatcher();
        Log.Write("Process assigned.");
      } catch ( Exception ex ) {
        Log.Write($"Unable to assign process {pid} : {ex.Message}");
      }
    }

    public static string GetCommandLine(int pid) {
      #region === Validate parameters ===
      if ( pid < 1 ) {
        return null;
      }
      #endregion === Validate parameters ===
      using ( ManagementObjectSearcher WmiSearch = new ManagementObjectSearcher($"SELECT CommandLine, ProcessId FROM Win32_Process WHERE ProcessId={pid}") ) {
        ManagementObjectCollection Processes = WmiSearch.Get();
        ManagementObjectEnumerator managementObjectEnumerator = Processes.GetEnumerator();
        managementObjectEnumerator.Reset();
        if ( managementObjectEnumerator.MoveNext() ) {
          return managementObjectEnumerator.Current["CommandLine"].ToString();
        }
        return "";
      }
    }

    public void SetProcessTitle(Process process, string title = "") {
      try {
        if ( process != null ) {
          IntPtr handle = process.MainWindowHandle;
          User32dll.SetWindowText(handle, title);
        }
      } catch { }
    }

    //public void SetConsoleColor(Process process, Color foregroundColor, Color backgroundColor) {
    //  try {
    //    if ( process != null ) {
    //      IntPtr handle = process.MainWindowHandle;
    //      SetConsoleTextAttribute(handle, (ushort)((new COLORREF(foregroundColor)).ColorDWORD| (new COLORREF(backgroundColor)).ColorDWORD));
    //    }
    //  } catch { }
    //}
  }
}
