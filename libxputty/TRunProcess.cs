using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

using BLTools;
using BLTools.Diagnostic.Logging;

using libxputty.DllImports;

using static System.Management.ManagementObjectCollection;
using static libxputty.DllImports.Kernel32dll;

namespace libxputty {
  public class TRunProcess : ALoggable {

#if DEBUG
    public static bool IsDebug = true;
#else
    public static bool IsDebug = false;
#endif

    #region --- Constants --------------------------------------------
    protected const int WATCH_DELAY_MS = 200;
    protected const int NO_PROCESS_PID = -1;
    #endregion --- Constants --------------------------------------------

    public bool IsRunning => _Process is not null && !_Process.HasExited;
    public int PID => _Process is null ? NO_PROCESS_PID : _Process.Id;
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
      if (IsRunning) {
        return;
      }

      IsRedirected = redirect;

      _Process = new Process();
      if (IsDebug) {
        LogDebug($"Process = {StartInfo.FileName}, Args = {StartInfo.Arguments}");
      }

      if (IsRedirected) {
        StartInfo.RedirectStandardError = true;
        StartInfo.RedirectStandardOutput = true;
      }
      _Process.StartInfo = StartInfo;
      if (IsRedirected) {
        _Process.ErrorDataReceived += _Process_ErrorDataReceived;
        _Process.OutputDataReceived += _Process_OutputDataReceived;
      }
      _Process.Start();
      if (IsRedirected) {
        _Process.BeginErrorReadLine();
        _Process.BeginOutputReadLine();
      }

      //SetConsoleColor(_Process, Color.Black, Color.White);


      if (OnStart is not null) {
        OnStart(this, EventArgs.Empty);
      }

      _StartWatcher();

      LogDebug("Start completed.");
    }

    private void _Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
      Log(e.Data);
    }

    private void _Process_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
      if (!string.IsNullOrEmpty(e.Data)) {
        LogError(e.Data);
      }
    }

    protected void _StartWatcher() {
      if (Watcher == null || Watcher.IsCanceled || Watcher.IsCompleted) {
        LogDebug("Starting the watcher...");
        Watcher = Task.Run(async () => {
          while (!_Process.HasExited || WatcherCancellation.IsCancellationRequested) {
            try {
              SetProcessTitle(_Process, ProcessTitle);
              await Task.Delay(WATCH_DELAY_MS);
              _Process.Refresh();
            } catch {
              if (IsRedirected) {
                _Process.ErrorDataReceived -= _Process_ErrorDataReceived;
                _Process.OutputDataReceived -= _Process_OutputDataReceived;
              }
              _Process.Dispose();
              _Process = null;
            }
          }
          if (_Process is not null && WatcherCancellation.IsCancellationRequested) {
            LogDebug("Killing process");
            _Process.Kill();
          }
          if (OnExit != null) {
            OnExit(this, EventArgs.Empty);
          }
        }, WatcherCancellation.Token);
      }
    }

    public void Cancel() {
      LogDebug("Cancelling the process...");
      WatcherCancellation.Cancel();
    }

    public void AssignProcess(Process process) {
      LogDebug($"Attempt to assign process to TRunProcess");
      if (process is null) {
        return;
      }
      try {
        _Process = process;
        _StartWatcher();
        LogDebug("Process assigned.");
      } catch (Exception ex) {
        LogError($"Unable to assign process : {ex.Message}");
      }
    }
    public void AssignProcessByPID(int pid) {
      LogDebug($"Attempt to assign process with PID {pid} to TRunProcess {StartInfo.FileName} {StartInfo.Arguments}");
      if (pid <= 0) {
        return;
      }
      try {
        _Process = Process.GetProcessById(pid);
        _StartWatcher();
        LogDebug("Process assigned.");
      } catch (Exception ex) {
        LogError($"Unable to assign process {pid} : {ex.Message}");
      }
    }

    public string GetCommandLine() {
      #region === Validate parameters ===
      if (PID < 1) {
        return null;
      }
      #endregion === Validate parameters ===
      using (ManagementObjectSearcher WmiSearch = new ManagementObjectSearcher($"SELECT CommandLine, ProcessId FROM Win32_Process WHERE ProcessId={PID}")) {
        ManagementObjectCollection Processes = WmiSearch.Get();
        ManagementObjectEnumerator managementObjectEnumerator = Processes.GetEnumerator();
        managementObjectEnumerator.Reset();
        return managementObjectEnumerator.MoveNext() ? managementObjectEnumerator.Current["CommandLine"].ToString() : "";
      }
    }

    public void SetProcessTitle(Process process, string title = "") {
      try {
        if (process is not null) {
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
