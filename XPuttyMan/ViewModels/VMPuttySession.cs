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
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public sealed class VMPuttySession : TVMEasyPuttyBase {

    public IPuttySession PuttySession => _Data as IPuttySession;

    #region RelayCommand
    public TRelayCommand CommandEditSessionRemoteCommand { get; private set; }
    public TRelayCommand CommandEditSessionRemoteCommandOk { get; private set; }
    public TRelayCommand CommandEditSessionRemoteCommandCancel { get; private set; }
    public TRelayCommand CommandStartSession { get; private set; }
    public TRelayCommand CommandMouseOver { get; private set; }
    public TRelayCommand CommandMouseLeave { get; private set; }
    #endregion RelayCommand

    public string CleanName => Name.Replace("%20", " ");
    public string GroupSection => $"{CleanName.Before('/')}";
    public string DisplayGroupSection => GroupSection == "" ? "<unnamed section>" : $"{CleanName.Before('/')}";
    public string DisplayName => CleanName.Contains('/') ? CleanName.After(GroupSection).After('/') : CleanName.After(GroupSection);

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
    public VMPuttySession(IPuttySession puttySession) : base(puttySession) {
      _InitializeCommands();
      _Initialize();
    }
    private void _InitializeCommands() {
      CommandStartSession = new TRelayCommand(() => _Start(), _ => true);
      CommandEditSessionRemoteCommand = new TRelayCommand(() => _EditRemoteCommand(), _ => !IsRunning && CanEditRemoteCommand);
      CommandEditSessionRemoteCommandOk = new TRelayCommand(() => _EditRemoteCommandOk(), _ => true);
      CommandEditSessionRemoteCommandCancel = new TRelayCommand(() => _EditRemoteCommandCancel(), _ => true);
      CommandMouseOver = new TRelayCommand(() => _MouseOver(), _ => true);
      CommandMouseLeave = new TRelayCommand(() => _MouseLeave(), _ => true);
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
      base.Dispose();
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
      PuttySession.SaveToRegistry();
      NotifyPropertyChanged(nameof(RemoteCommand));
      IsRemoteCommandInProgress = false;
    }
    private void _EditRemoteCommandCancel() {
      RemoteCommand = OldRemoteCommand;
      EditRemoteCommandWindow.Close();
      IsRemoteCommandInProgress = false;
    }

    private void _MouseOver() {
      IsSelected = true;
      NotifyPropertyChanged(nameof(IsSelected));
      //Log.Write($"Over session {CleanName}");
    }

    private void _MouseLeave() {
      IsSelected = false;
      NotifyPropertyChanged(nameof(IsSelected));
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
    public static VMPuttySession DesignVMPuttySession {
      get {
        if ( _DesignVMPuttySession == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Group/Fake session") {
            Port = 22,
            HostName = "server.test.priv",
            RemoteCommand = "tail -n 100 -f /var/log/syslog"
          };
          _DesignVMPuttySession = new VMPuttySession(FakeSession);
        }
        return _DesignVMPuttySession;
      }
    }
    private static VMPuttySession _DesignVMPuttySession;

    public static VMPuttySession DesignVMPuttySession2 {
      get {
        if ( _DesignVMPuttySession2 == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Other/Other session") {
            Port = 22,
            HostName = "server2.test.priv",
            RemoteCommand = "tail -n 200 -f /var/log/syslog"
          };
          _DesignVMPuttySession2 = new VMPuttySession(FakeSession);
        }
        return _DesignVMPuttySession2;
      }
    }
    private static VMPuttySession _DesignVMPuttySession2; 
    #endregion --- For design time --------------------------------------------

    public void AssignProcess(Process process) {
      PuttySession.PuttyProcess.AssignProcess(process);
    }
  }
}
