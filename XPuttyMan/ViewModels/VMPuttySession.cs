using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BLTools;
using BLTools.MVVM;
using libxputty_std20;

namespace XPuttyMan {
  public sealed class VMPuttySession : MVVMBase, IDisposable {

    private IPuttySession _PuttySession;

    #region RelayCommand
    public TRelayCommand CommandStartSession { get; private set; }
    #endregion RelayCommand

    public string Name => _PuttySession.Name ?? "";
    public string CleanName => Name.Replace("%20", " ");
    public string HostName {
      get {
        if ( _PuttySession is TPuttySessionSSH PuttySessionSSH ) {
          return $"{PuttySessionSSH.HostName ?? ""}:{PuttySessionSSH.Port}";
        }
        return "";
      }
    }
    public string RemoteCommand => _PuttySession is TPuttySessionSSH PuttySessionSSH ? PuttySessionSSH.SSH_RemoteCommand : "";
    public Visibility IsRemoteCommandVisible => RemoteCommand == "" ? Visibility.Collapsed : Visibility.Visible;
    public bool IsRunning => _PuttySession.IsRunning;
    public string PuttyCommandLine => _PuttySession.CommandLine;
    public int PID => _PuttySession.PID;
    public string RunningIcon => IsRunning ? App.GetPictureFullname("putty_icon") : "";

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public VMPuttySession() {
      _InitializeCommands();
      _Initialize(TPuttySession.Empty);
    }
    public VMPuttySession(IPuttySession puttySession) {
      _InitializeCommands();
      _Initialize(puttySession);
    }

    private void _Initialize(IPuttySession puttySession) {
      _PuttySession = puttySession;
      NotifyPropertyChanged(nameof(Name));
      NotifyPropertyChanged(nameof(CleanName));
      NotifyPropertyChanged(nameof(HostName));
      _PuttySession.OnStart += _PuttySession_OnStart;
      _PuttySession.OnExit += _PuttySession_OnExit;
    }

    private void _InitializeCommands() {
      CommandStartSession = new TRelayCommand(() => Start(), _ => { return true; });
    }

    public void Dispose() {
      _PuttySession.OnStart -= _PuttySession_OnStart;
      _PuttySession.OnExit -= _PuttySession_OnExit;
      _PuttySession.Dispose();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Start() {
      _PuttySession.Start();
    }

    public void SetRunningProcess(IPuttySession puttySession) {
      _PuttySession.SetRunningProcess(puttySession.PuttyProcess);
      NotifyPropertyChanged(nameof(PID));
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(PuttyCommandLine));
      NotifyPropertyChanged(nameof(RunningIcon));
    }

    private void _PuttySession_OnExit(object sender, EventArgs e) {
      Log.Write($"Session {CleanName} exited.");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
    }

    private void _PuttySession_OnStart(object sender, EventArgs e) {
      Log.Write($"Starting session {CleanName}");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
    }

    public static VMPuttySession DesignVMPuttySession {
      get {
        if ( _DesignVMPuttySession == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Fake session") {
            Port = 22,
            HostName = "server.test.priv",
            SSH_RemoteCommand = "tail -n -f 100 /var/log/syslog"
          };
          
          _DesignVMPuttySession = new VMPuttySession(FakeSession);
        }
        return _DesignVMPuttySession;
      }
    }
    private static VMPuttySession _DesignVMPuttySession;
  }
}
