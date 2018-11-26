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

    private IView View;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMEditedPuttySession(IPuttySession puttySession, IView view) : base(puttySession) {
      View = view;
    }
    protected override void _InitializeCommands() {
      CommandOk = new TRelayCommand(() => _CommandOk(), _ => true);
      CommandCancel = new TRelayCommand(() => _CommandCancel(), _ => true);
    }

    protected override void _Initialize() {
    }

    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Commands --------------------------------------------
    private void _CommandOk() {
      View.DialogResult = true;
      View.Close();
    }

    private void _CommandCancel() {
      View.DialogResult = false;
      View.Close();
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
