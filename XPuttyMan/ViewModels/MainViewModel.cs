using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BLTools;
using BLTools.MVVM;
using libxputty_std20;
using Microsoft.Win32;

namespace XPuttyMan {
  public class MainViewModel : MVVMBase {

    #region RelayCommand
    public TRelayCommand CommandFileOpen { get; private set; }
    public TRelayCommand CommandHelpContact { get; private set; }
    public TRelayCommand CommandHelpAbout { get; private set; }
    public TRelayCommand CommandRefreshSessions { get; private set; }
    public TRelayCommand CommandStartSession { get; private set; }
    #endregion RelayCommand

    public string PuttyIcon => App.GetPictureFullname("putty_icon");

    public VMPuttySessionsList ObservableSessions { get; set; }
    public VMPuttySessionsList ObservableCommandSessions { get; set; }

    public VMPuttySession SelectedSession => ObservableSessions.IsActive ? ObservableSessions.SelectedSession : ObservableCommandSessions.SelectedSession;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public MainViewModel() : base() {
      _InitializeCommands();
      _Initialize();
    }

    private void _Initialize() {
      ObservableSessions = new VMPuttySessionsList();
      ObservableCommandSessions = new VMPuttySessionsList();
      RefreshSessionsFromRegistry();
    }

    private void _InitializeCommands() {
      CommandFileOpen = new TRelayCommand(() => FileOpen(), _ => { return true; });
      CommandHelpContact = new TRelayCommand(() => HelpContact(), _ => { return true; });
      CommandHelpAbout = new TRelayCommand(() => HelpAbout(), _ => { return true; });
      CommandRefreshSessions = new TRelayCommand(() => RefreshSessionsFromRegistry(), _ => { return !WorkInProgress; });
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Menu --------------------------------------------
    public void FileOpen() {
      OpenFileDialog OFD = new OpenFileDialog {
        Title = "Select new parameter file",
        Filter = "Parameter file|*.xml"
      };
      if ( OFD.ShowDialog() != true ) {
        return;
      }
    }
    public void HelpContact() {

    }
    public void HelpAbout() {
      StringBuilder Usage = new StringBuilder();
      Usage.AppendLine(string.Format("xPuttyMan v{0}", Assembly.GetEntryAssembly().GetName().Version.ToString()));
      Usage.AppendLine(@"Usage: xPuttyMan [/config=<config.xml> (default=<none>)]");
      Usage.AppendLine(@"                 [/logbase=<[\\server\share\]path> (default=c:\logs\xputtyman)]");
      Usage.AppendLine(@"                 [/log=<filename.log> (xputtyman.log)]");
      MessageBox.Show(Usage.ToString());
    }
    #endregion --- Menu --------------------------------------------

    public void RefreshSessionsFromRegistry() {

      WorkInProgress = true;
      CommandRefreshSessions.NotifyCanExecuteChanged();
      Log.Write("Refreshing sessions...");
      NotifyExecutionProgress("Reading sessions...");

      ObservableSessions = new VMPuttySessionsList();
      ObservableCommandSessions = new VMPuttySessionsList();

      TPuttySessionList Sessions = new TPuttySessionList();
      Sessions.ReadSessionsFromRegistry();

      ObservableSessions = CreateAndRecoverSessions(Sessions, TPuttyProtocol.SSH, x => !x.Name.StartsWith("zzz"));
      ObservableCommandSessions = CreateAndRecoverSessions(Sessions, TPuttyProtocol.SSH, x => x.Name.StartsWith("zzz"));

      NotifyExecutionStatus($"{ObservableSessions.Count + ObservableCommandSessions.Count} session(s)");
      Log.Write("Refresh done.");
      WorkInProgress = false;
      CommandRefreshSessions.NotifyCanExecuteChanged();

    }

    private VMPuttySessionsList CreateAndRecoverSessions(TPuttySessionList sessions, TPuttyProtocol protocol, Func<IPuttySession, bool> predicate) {
      if ( sessions == null ) {
        return null;
      }

      VMPuttySessionsList RetVal = new VMPuttySessionsList();

      if ( !sessions.Items.Any() ) {
        return new VMPuttySessionsList();
      }

      IEnumerable<Process> CurrentlyRunningSessions = Process.GetProcessesByName(TPuttySession.EXECUTABLE_PUTTY);

      foreach ( IPuttySession SessionItem in sessions.Items.Where(x => x.Protocol == protocol)
                                                           .Where(x => !string.IsNullOrWhiteSpace(((TPuttySessionSSH)x).HostName))
                                                           .Where(x => predicate(x)) ) {

        Process RunningSession = CurrentlyRunningSessions.FirstOrDefault(x => TRunProcess.GetCommandLine(x.Id).EndsWith($"\"{SessionItem.CleanName}\""));

        VMPuttySession NewPuttySessionVM;
        NewPuttySessionVM = new VMPuttySession(SessionItem);
        if ( RunningSession != null ) {
          NewPuttySessionVM.AssignProcess(RunningSession);
        }

        RetVal.Add(NewPuttySessionVM);
      }

      return RetVal;
    }
    public static MainViewModel DesignMainViewModel {
      get {
        if ( _DesignMainViewModel == null ) {
          _DesignMainViewModel = new MainViewModel();
        }
        return _DesignMainViewModel;
      }
    }
    private static MainViewModel _DesignMainViewModel;
  }
}
