using System;
using System.Diagnostics;

using BLTools;

using EasyPutty.Interfaces;

using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public sealed class TVMPuttySession : TVMEasyPuttyBase, IHeader {

    public IPuttySession PuttySession => _Data as IPuttySession;

    #region RelayCommand
    public TRelayCommand CommandEditSessionRemoteCommand { get; private set; }
    public TRelayCommand CommandEditSessionRemoteCommandOk { get; private set; }
    public TRelayCommand CommandEditSessionRemoteCommandCancel { get; private set; }
    public TRelayCommand CommandStartSession { get; private set; }
    #endregion RelayCommand

    public string CleanName => PuttySession == null ? "" : PuttySession.CleanName;

    public string Header => CleanName.Replace($"[{string.Join("//", GroupLevel1, GroupLevel2).TrimEnd('/')}]", "").Replace($"{{{Section}}}", "");

    public string GroupLevel1 {
      get {
        if ( string.IsNullOrWhiteSpace(_GroupLevel1) ) {
          _GroupLevel1 = PuttySession == null ? "" : PuttySession.GroupLevel1;
        }
        return _GroupLevel1;
      }
      set {
        _GroupLevel1 = value;
        NotifyPropertyChanged(nameof(GroupLevel1));
      }
    }
    private string _GroupLevel1;

    public string GroupLevel2 {
      get {
        if ( string.IsNullOrWhiteSpace(_GroupLevel2) ) {
          _GroupLevel2 = PuttySession == null ? "" : PuttySession.GroupLevel2;
        }
        return _GroupLevel2;
      }
      set {
        _GroupLevel2 = value;
        NotifyPropertyChanged(nameof(GroupLevel2));
      }
    }
    private string _GroupLevel2;

    public string Section {
      get {
        if ( string.IsNullOrWhiteSpace(_Section) ) {
          _Section = PuttySession == null ? "" : PuttySession.Section;
        }
        return _Section;
      }
      set {
        _Section = value;
        NotifyPropertyChanged(nameof(Section));
      }
    }
    private string _Section;

    public string HostName {
      get {
        if ( PuttySession is TPuttySessionSSH PuttySessionSSH ) {
          return $"{PuttySessionSSH.HostName ?? ""}:{PuttySessionSSH.Port}";
        }
        return "";
      }
    }
    public string RemoteCommand {
      get {
        return PuttySession is TPuttySessionSSH PuttySessionSSH ? PuttySessionSSH.RemoteCommand : "";
      }
      set {
        if ( PuttySession is TPuttySessionSSH PuttySessionSSH ) {
          PuttySessionSSH.RemoteCommand = value;
          MainWindow.DataIsDirty = true;
        }
      }
    }
    public bool CanEditRemoteCommand => RemoteCommand != "";
    public bool IsRemoteCommandInProgress { get; private set; }
    public bool IsRunning => PuttySession.IsRunning;
    public string PuttyCommandLine => PuttySession.CommandLine;
    public int PID => PuttySession.PID;
    public string RunningIcon => IsRunning ? App.GetPictureFullname("putty_icon") : App.GetPictureFullname("connect_icon");

    public bool IsSelected { get; set; }

    private Views.WindowEditRemoteCommand EditRemoteCommandWindow;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySession(IPuttySession puttySession) : base(puttySession) {
    }
    protected override void _InitializeCommands() {
      CommandStartSession = new TRelayCommand(() => _Start(), _ => true);
      CommandEditSessionRemoteCommand = new TRelayCommand(() => _EditRemoteCommand(), _ => !IsRunning && CanEditRemoteCommand);
      CommandEditSessionRemoteCommandOk = new TRelayCommand(() => _EditRemoteCommandOk(), _ => true);
      CommandEditSessionRemoteCommandCancel = new TRelayCommand(() => _EditRemoteCommandCancel(), _ => true);
    }

    protected override void _Initialize() {
      NotifyPropertyChanged(nameof(CleanName));
      NotifyPropertyChanged(nameof(HostName));
      PuttySession.OnStart += _PuttySession_OnStart;
      PuttySession.OnExit += _PuttySession_OnExit;
    }


    public override void Dispose() {
      PuttySession.OnStart -= _PuttySession_OnStart;
      PuttySession.OnExit -= _PuttySession_OnExit;
      PuttySession.Dispose();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    private void _Start() {
      if ( RemoteCommand != "" ) {
        PuttySession.StartPlink();
      } else {
        PuttySession.Start();
      }
    }

    private string OldRemoteCommand;

    private void _EditRemoteCommand() {
      if ( !IsRemoteCommandInProgress ) {
        EditRemoteCommandWindow = new Views.WindowEditRemoteCommand();
        IsRemoteCommandInProgress = true;
        OldRemoteCommand = RemoteCommand;
        EditRemoteCommandWindow.DataContext = this;
        EditRemoteCommandWindow.Show();
      }
    }

    private void _EditRemoteCommandOk() {
      EditRemoteCommandWindow.Close();
      //PuttySession.SaveToRegistry();
      NotifyPropertyChanged(nameof(RemoteCommand));
      IsRemoteCommandInProgress = false;
    }
    private void _EditRemoteCommandCancel() {
      RemoteCommand = OldRemoteCommand;
      EditRemoteCommandWindow.Close();
      IsRemoteCommandInProgress = false;
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

    #region --- For design time --------------------------------------------
    public static TVMPuttySession DesignVMPuttySession {
      get {
        if ( _DesignVMPuttySession == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Fake session") {
            GroupLevel1 = "Sharenet",
            GroupLevel2 = "CMD",
            Section = "LAN",
            Port = 22,
            HostName = "server.test.priv",
            RemoteCommand = "tail -n 100 -f /var/log/syslog"
          };
          _DesignVMPuttySession = new TVMPuttySession(FakeSession);
        }
        return _DesignVMPuttySession;
      }
    }
    private static TVMPuttySession _DesignVMPuttySession;

    public static TVMPuttySession DesignVMPuttySession2 {
      get {
        if ( _DesignVMPuttySession2 == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Other session") {
            GroupLevel1 = "Sharenet",
            GroupLevel2 = "CMD",
            Section = "DMZ",
            Port = 22,
            HostName = "server2.test.priv",
            RemoteCommand = "tail -n 200 -f /var/log/syslog"
          };
          _DesignVMPuttySession2 = new TVMPuttySession(FakeSession);
        }
        return _DesignVMPuttySession2;
      }
    }

    private static TVMPuttySession _DesignVMPuttySession2;
    #endregion --- For design time --------------------------------------------

    public void AssignProcess(Process process) {
      PuttySession.PuttyProcess.AssignProcess(process);
    }
  }
}
