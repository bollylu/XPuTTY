using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

using BLTools;

using EasyPutty.Interfaces;

using libxputty;
using libxputty.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMPuttySession : AVMEasyPuttyBase, IHeaderedAndSelectable {

    public IPuttySession PuttySession => _Data as IPuttySession;

    public ISerial PuttySessionSerial => PuttySession as ISerial;
    public IHostAndPort PuttySessionHAP => PuttySession as IHostAndPort;

    public bool DisplaySelectionButton => true;

    public MainViewModel RootVM => GetParent<MainViewModel>() as MainViewModel;

    #region RelayCommand
    public TRelayCommand CommandStartSession { get; private set; }
    public TRelayCommand CommandSelectItem { get; private set; }
    public TRelayCommand CommandEditSession { get; private set; }
    #endregion RelayCommand

    public string CleanName => PuttySession == null ? "" : PuttySession.CleanName;

    public string Header => CleanName.Replace($"[{string.Join("//", GroupLevel1, GroupLevel2).TrimEnd('/')}]", "").Replace($"{{{Section}}}", "");

    public string TooltipComment => string.IsNullOrWhiteSpace(base.Comment) ? null : base.Comment;

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

    public string RemoteCommand {
      get {
        if ( PuttySession != null ) {
          return PuttySession.RemoteCommand;
        }
        return "";
      }
      set {
        if ( PuttySession != null ) {
          PuttySession.RemoteCommand = value;
          NotifyPropertyChanged(nameof(RemoteCommand));
          NotifyPropertyChanged(nameof(HasRemoteCommand));
        }
      }
    }
    public bool HasRemoteCommand => !string.IsNullOrEmpty(RemoteCommand);

    public bool IsRunning => PuttySession.IsRunning;
    public string PuttyCommandLine => PuttySession.CommandLine;
    public int PID => PuttySession.PID;
    public string RunningIcon => IsRunning ? App.GetPictureFullname("putty_icon") : App.GetPictureFullname("connect_icon");

    public bool IsSelected { get; set; }

    #region --- Session IHostAndPort --------------------------------------------
    public bool SessionIsHAP => PuttySessionHAP != null;
    public string DisplayHostnameAndPort => $"{HostName}:{Port}";
    public string HostName {
      get {
        if ( SessionIsHAP ) {
          return PuttySessionHAP.HostName ?? "";
        }
        return "";
      }
      set {
        if ( SessionIsHAP ) {
          PuttySessionHAP.HostName = value;
          NotifyPropertyChanged(nameof(HostName));
          NotifyPropertyChanged(nameof(DisplayHostnameAndPort));
        }
      }
    }
    public int Port {
      get {
        if ( SessionIsHAP ) {
          return PuttySessionHAP.Port;
        }
        return 0;
      }
      set {
        if ( SessionIsHAP ) {
          PuttySessionHAP.Port = value;
          NotifyPropertyChanged(nameof(Port));
          NotifyPropertyChanged(nameof(DisplayHostnameAndPort));
        }
      }
    }
    #endregion --- Session IHostAndPort --------------------------------------------

    #region --- Serial session --------------------------------------------
    public bool SessionIsSerial => PuttySession is ISerial;
    public string SerialLine {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialLine ?? "";
        }
        return "";
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialLine = value;
          NotifyPropertyChanged(nameof(SerialLine));
        }
      }
    }
    public int SerialSpeed {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialSpeed;
        }
        return 0;
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialSpeed = value;
          NotifyPropertyChanged(nameof(SerialSpeed));
        }
      }
    }
    public byte SerialDataBits {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialDataBits;
        }
        return 0;
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialDataBits = value;
          NotifyPropertyChanged(nameof(SerialDataBits));
        }
      }
    }
    public byte SerialStopBits {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialStopBits;
        }
        return 0;
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialStopBits = value;
          NotifyPropertyChanged(nameof(SerialStopBits));
        }
      }
    }
    public string SerialParity {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialParity;
        }
        return "";
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialParity = value;
          NotifyPropertyChanged(nameof(SerialParity));
        }
      }
    }
    public string SerialFlowControl {
      get {
        if ( SessionIsSerial ) {
          return PuttySessionSerial.SerialFlowControl ?? "";
        }
        return "";
      }
      set {
        if ( SessionIsSerial ) {
          PuttySessionSerial.SerialFlowControl = value;
          NotifyPropertyChanged(nameof(SerialFlowControl));
        }
      }
    }
    #endregion --- Serial session --------------------------------------------

    private IView EditSessionWindow;

    public bool IsEditSessionInProgress { get; private set; }
    protected TVMEditedPuttySession VMEditedPuttySession;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySession(IPuttySession puttySession) : base(puttySession) {
    }

    protected override void _InitializeCommands() {
      CommandStartSession = new TRelayCommand(() => _Start(), _ => true);
      CommandEditSession = new TRelayCommand(() => _EditSession(), _ => true);
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

    #region --- Start session and events --------------------------------------------
    private void _Start() {
      PuttySession.Start();
    }
    private void _PuttySession_OnExit(object sender, EventArgs e) {
      Log($"Session {CleanName} exited.");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
      NotifyPropertyChanged(nameof(PuttyCommandLine));
    }

    private void _PuttySession_OnStart(object sender, EventArgs e) {
      Log($"=> Started session {CleanName}");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
      NotifyPropertyChanged(nameof(PuttyCommandLine));
    }
    #endregion --- Start session and events -----------------------------------------

    #region --- Edit session and events --------------------------------------------
    private void _EditSession() {
      if ( IsEditSessionInProgress ) {
        return;
      }
      IsEditSessionInProgress = true;

      IPuttySession EditedPuttySession = PuttySession.Duplicate();

      EditSessionWindow = new Views.ViewEditSession();
      
      VMEditedPuttySession = new TVMEditedPuttySession(EditedPuttySession, EditSessionWindow);
      VMEditedPuttySession.OnEditCompleted += VMEditedPuttySession_OnEditCompleted;

      EditSessionWindow.DataContext = VMEditedPuttySession;

      EditSessionWindow.Show();
      if ( EditedPuttySession.Credential != null && EditedPuttySession.Credential.HasValue ) {
        if ( EditSessionWindow is IPassword EditSessionWithPassword ) {
          EditSessionWithPassword.SetPassword(EditedPuttySession.Credential.SecurePassword);
        }
      }
    }

    private void VMEditedPuttySession_OnEditCompleted(object sender, bool e) {
      VMEditedPuttySession.OnEditCompleted -= VMEditedPuttySession_OnEditCompleted;
      if ( e ) {
        RootVM.DataIsDirty = true;
        Description = VMEditedPuttySession.Description;
        Comment = VMEditedPuttySession.Comment;
        RemoteCommand = VMEditedPuttySession.RemoteCommand;
        if ( VMEditedPuttySession.PuttySessionHAP != null ) {
          HostName = VMEditedPuttySession.PuttySessionHAP.HostName;
          Port = VMEditedPuttySession.PuttySessionHAP.Port;
        }
        if ( VMEditedPuttySession.PuttySessionSerial != null ) {
          SerialLine = VMEditedPuttySession.PuttySessionSerial.SerialLine;
          SerialSpeed = VMEditedPuttySession.PuttySessionSerial.SerialSpeed;
          SerialDataBits = VMEditedPuttySession.PuttySessionSerial.SerialDataBits;
          SerialStopBits = VMEditedPuttySession.PuttySessionSerial.SerialStopBits;
          SerialParity = VMEditedPuttySession.PuttySessionSerial.SerialParity;
          SerialFlowControl = VMEditedPuttySession.PuttySessionSerial.SerialFlowControl;
        }
        if (VMEditedPuttySession.PuttySession.Credential!=null) {
          PuttySession.Credential.Username = VMEditedPuttySession.PuttySession.Credential.Username;
          PuttySession.Credential.SecurePassword = VMEditedPuttySession.PuttySession.Credential.SecurePassword;
        }
      }

      VMEditedPuttySession.Dispose();
      IsEditSessionInProgress = false;
    }

    #endregion --- Edit session and events -----------------------------------------

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

    public static TVMPuttySession DesignVMPuttySessionSerial {
      get {
        if ( _DesignVMPuttySessionSerial == null ) {
          TPuttySessionSerial FakeSession = new TPuttySessionSerial("Serial session") {
            GroupLevel1 = "Sharenet",
            GroupLevel2 = "CMD",
            Section = "DMZ",
            SerialLine = "COM4",
            SerialSpeed = 9600,
            SerialDataBits = 8,
            SerialStopBits = 1,
            SerialParity = "None",
            RemoteCommand = "tail -n 200 -f /var/log/syslog"
          };
          _DesignVMPuttySessionSerial = new TVMPuttySession(FakeSession);
        }
        return _DesignVMPuttySessionSerial;
      }
    }

    

    private static TVMPuttySession _DesignVMPuttySessionSerial;
    #endregion --- For design time --------------------------------------------

    public void AssignProcess(Process process) {
      PuttySession.PuttyProcess.AssignProcess(process);
    }

    public ObservableCollection<IHeaderedAndSelectable> Items { get; }
    public void Clear() { }
  }
}
