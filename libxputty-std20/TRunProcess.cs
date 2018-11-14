using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLTools;
using libxputty_std20.DllImports;
using static System.Management.ManagementObjectCollection;

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

    public bool IsRunning => Process.GetProcesses().Any(x => x.Id == PID);
    public int PID => _Process == null ? NO_PROCESS_PID : _Process.Id;
    public ProcessStartInfo StartInfo { get; set; }

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

    public void Start() {
      if ( IsRunning ) {
        return;
      }

      _Process = new Process();
      if ( IsDebug ) {
        Log.Write($"Process = {StartInfo.FileName}, Args = {StartInfo.Arguments}");
      }

      _Process.StartInfo = StartInfo;
      _Process.Start();

      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }

      _StartWatcher();

      Log.Write("Start completed.");
    }

    protected void _StartWatcher() {
      if ( Watcher == null || Watcher.IsCanceled || Watcher.IsCompleted ) {
        Log.Write("Starting the watcher...");
        Watcher = Task.Run(async () => {
          while ( PID != NO_PROCESS_PID || WatcherCancellation.IsCancellationRequested ) {
            try {
              Process WatchedProcess = Process.GetProcessById(PID);
              await Task.Delay(WATCH_DELAY_MS);
              SetProcessTitle(ProcessTitle);
            } catch {
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
        return "";
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

    public void SetProcessTitle(string title = "") {
      if ( _Process != null ) {
        IntPtr handle = _Process.MainWindowHandle;
        User32dll.SetWindowText(handle, title);
      }
    }
  }
}
