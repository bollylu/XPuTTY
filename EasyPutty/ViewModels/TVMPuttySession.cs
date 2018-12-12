﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using BLTools;

using EasyPutty.Interfaces;

using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMPuttySession : TVMEasyPuttyBase {

    public IPuttySession PuttySession => _Data as IPuttySession;

    public ISessionTypeSerial PuttySessionSerial => PuttySession as ISessionTypeSerial;
    public ISessionTypeNetwork PuttySessionNetwork => PuttySession as ISessionTypeNetwork;

    public bool DisplaySelectionButton => true;

    public MainViewModel RootVM => GetParent<MainViewModel>() as MainViewModel;

    #region RelayCommand
    public ICommand CommandStartSession { get; private set; }
    public ICommand CommandSelectItem { get; private set; }
    public ICommand CommandEditSession { get; private set; }
    #endregion RelayCommand

    public string Header => Name;

    public string TooltipComment => string.IsNullOrWhiteSpace(base.Comment) ? null : base.Comment;

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

    #region --- ISessionTypeNetwork --------------------------------------------
    public bool SessionTypeIsNetwork => PuttySession is ISessionTypeNetwork;

    public string DisplayHostnameAndPort => $"{HostName}:{Port}";
    public string HostName {
      get {
        if ( SessionTypeIsNetwork ) {
          return PuttySessionNetwork.HostName ?? "";
        }
        return "";
      }
      set {
        if ( SessionTypeIsNetwork ) {
          PuttySessionNetwork.HostName = value;
          NotifyPropertyChanged(nameof(HostName));
          NotifyPropertyChanged(nameof(DisplayHostnameAndPort));
        }
      }
    }
    public int Port {
      get {
        if ( SessionTypeIsNetwork ) {
          return PuttySessionNetwork.Port;
        }
        return 0;
      }
      set {
        if ( SessionTypeIsNetwork ) {
          PuttySessionNetwork.Port = value;
          NotifyPropertyChanged(nameof(Port));
          NotifyPropertyChanged(nameof(DisplayHostnameAndPort));
        }
      }
    }
    #endregion --- ISessionTypeNetwork --------------------------------------------

    #region --- Serial session --------------------------------------------
    public bool SessionIsSerial => PuttySession is ISessionTypeSerial;
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
      CommandStartSession = new TRelayCommand(() => _Start());
      CommandEditSession = new TRelayCommand(() => _EditSession());
    }

    protected override void _Initialize() {
      NotifyPropertyChanged(nameof(Name));
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
      Log.Write($"Session {Name} exited.");
      NotifyPropertyChanged(nameof(IsRunning));
      NotifyPropertyChanged(nameof(RunningIcon));
      NotifyPropertyChanged(nameof(PuttyCommandLine));
    }

    private void _PuttySession_OnStart(object sender, EventArgs e) {
      Log.Write($"=> Started session {Name}");
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
        if ( VMEditedPuttySession.PuttySessionNetwork != null ) {
          HostName = VMEditedPuttySession.PuttySessionNetwork.HostName;
          Port = VMEditedPuttySession.PuttySessionNetwork.Port;
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
    public static TVMPuttySession DesignVMPuttySessionSSH1 {
      get {
        if ( _DesignVMPuttySessionSSH1 == null ) {
          _DesignVMPuttySessionSSH1 = new TVMPuttySession(TPuttySessionSSH.DemoPuttySessionSSH1);
        }
        return _DesignVMPuttySessionSSH1;
      }
    }
    private static TVMPuttySession _DesignVMPuttySessionSSH1;

    public static TVMPuttySession DesignVMPuttySessionSSH2 {
      get {
        if ( _DesignVMPuttySessionSSH2 == null ) {
          _DesignVMPuttySessionSSH2 = new TVMPuttySession(TPuttySessionSSH.DemoPuttySessionSSH2);
        }
        return _DesignVMPuttySessionSSH2;
      }
    }
    private static TVMPuttySession _DesignVMPuttySessionSSH2;

    public static TVMPuttySession DesignVMPuttySessionSerial {
      get {
        if ( _DesignVMPuttySessionSerial == null ) {
          _DesignVMPuttySessionSerial = new TVMPuttySession(TPuttySessionSerial.DemoPuttySessionSerial1);
        }
        return _DesignVMPuttySessionSerial;
      }
    }
    private static TVMPuttySession _DesignVMPuttySessionSerial;
    #endregion --- For design time --------------------------------------------

    public void AssignProcess(Process process) {
      PuttySession.PuttyProcess.AssignProcess(process);
    }

  }
}
