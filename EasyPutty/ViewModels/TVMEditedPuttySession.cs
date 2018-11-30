using System;
using System.Diagnostics;

using BLTools;

using EasyPutty.Interfaces;

using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public sealed class TVMEditedPuttySession : TVMPuttySession {

    #region RelayCommand
    public TRelayCommand CommandOk { get; private set; }
    public TRelayCommand CommandCancel { get; private set; }
    #endregion RelayCommand

    public IView View;
    public event EventHandler<bool> OnEditCompleted;

    public string Username {
      get {
        if ( PuttySession != null ) {
          if ( PuttySession.Credential != null ) {
            return PuttySession.Credential.Username;
          }
        }
        return "";
      }
      set {
        if ( PuttySession != null ) {
          if ( PuttySession.Credential != null ) {
            PuttySession.Credential.Username = value;
          }
        }
      }
    }

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMEditedPuttySession(IPuttySession puttySession, IView view) : base(puttySession) {
      View = view;
    }
    protected override void _InitializeCommands() {
      base._InitializeCommands();
      CommandOk = new TRelayCommand(() => _CommandOk(), _ => true);
      CommandCancel = new TRelayCommand(() => _CommandCancel(), _ => true);
    }

    protected override void _Initialize() {
      base._Initialize();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Commands --------------------------------------------
    private void _CommandOk() {
      if ( View != null ) {
        if ( View is IPassword ViewWithPassword ) {
          if ( PuttySession.Credential == null ) {
            PuttySession.SetLocalCredential(TCredential.Create(Username, ViewWithPassword.GetPassword()));
          } else {
            PuttySession.Credential.SecurePassword = ViewWithPassword.GetPassword();
          }
        }
        View.Close();
      }
      if ( OnEditCompleted != null ) {
        OnEditCompleted(this, true);
      }
    }

    private void _CommandCancel() {
      if ( View != null ) {
        View.Close();
      }
      if ( OnEditCompleted != null ) {
        OnEditCompleted(this, false);
      }
    }
    #endregion --- Commands -----------------------------------------

    #region --- For design time --------------------------------------------
    public static TVMPuttySession DesignVMEditedPuttySession {
      get {
        if ( _DesignVMEditedPuttySession == null ) {
          TPuttySessionSSH FakeSession = new TPuttySessionSSH("Fake session") {
            GroupLevel1 = "Sharenet",
            GroupLevel2 = "CMD",
            Section = "LAN",
            Port = 22,
            HostName = "server.test.priv",
            RemoteCommand = "tail -n 100 -f /var/log/syslog"
          };
          _DesignVMEditedPuttySession = new TVMEditedPuttySession(FakeSession, null);
        }
        return _DesignVMEditedPuttySession;
      }
    }
    private static TVMPuttySession _DesignVMEditedPuttySession;
    #endregion --- For design time --------------------------------------------
  }
}
