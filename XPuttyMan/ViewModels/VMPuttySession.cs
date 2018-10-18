using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools;
using BLTools.MVVM;
using libxputty_std20;

namespace XPuttyMan {
  public class VMPuttySession : MVVMBase, IDisposable {

    private IPuttySession _PuttySession;

    #region RelayCommand
    public TRelayCommand CommandStartSession { get; private set; }
    #endregion RelayCommand

    public string Name => _PuttySession.Name ?? "";
    public string DisplayName => Name.Replace("%20", " ");
    public string HostName {
      get {
        if ( _PuttySession.Protocol.IsSSH ) {
          TPuttySessionSSH TempPuttySessionSSH = _PuttySession as TPuttySessionSSH;
          return $"{TempPuttySessionSSH.HostName ?? ""}:{TempPuttySessionSSH.Port}";
        }
        return "";
      }
    }

    public bool IsRunning => PID != 0;

    public string RunningProcess => IsRunning ? TPuttySession.GetAllRunningSessions().First(x => x.PID == PID).Name : "";

    public int PID {
      get {
        return _PuttySession.PID;
      }
      //set {
      //  _PuttySession.PID = value;
      //}
    }

    public string RunningIcon => IsRunning ? App.GetPictureFullname("putty_icon") : "";

    #region --- Constructor(s) ---------------------------------------------------------------------------------

    public VMPuttySession() {
      _Initialize(TPuttySession.Empty);
      _InitializeCommands();
    }
    public VMPuttySession(IPuttySession puttySession) {
      _Initialize(puttySession);
      _InitializeCommands();
    }

    protected void _Initialize(IPuttySession puttySession) {
      _PuttySession = puttySession;
      NotifyPropertyChanged(nameof(Name));
      NotifyPropertyChanged(nameof(DisplayName));
      NotifyPropertyChanged(nameof(HostName));
      _PuttySession.OnStart += _PuttySession_OnStart;
      _PuttySession.OnExit += _PuttySession_OnExit;
    }

    protected void _InitializeCommands() {
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

    private void _PuttySession_OnExit(object sender, EventArgs e) {
      Log.Write($"Session {DisplayName} exited.");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
    }

    private void _PuttySession_OnStart(object sender, EventArgs e) {
      Log.Write($"Starting session {DisplayName}");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
    }

    public static VMPuttySession Design {
      get {
        if ( _Design == null ) {
          _Design = new VMPuttySession();
        }
        return _Design;
      }
    }
    private static VMPuttySession _Design;
  }
}
