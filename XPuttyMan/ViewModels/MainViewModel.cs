using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public ObservableCollection<VMPuttySession> ObservableSessions { get; set; }
    public ObservableCollection<VMPuttySession> ObservableCommandSessions { get; set; }

    public VMPuttySession SelectedSession {
      get {
        if ( !ObservableSessions.Any() && _SelectedSession == null ) {
          return new VMPuttySession(TPuttySession.Empty);
        }
        return _SelectedSession;
      }
      set {
        _SelectedSession = value;
        NotifyPropertyChanged(nameof(SelectedSession));
      }
    }
    private VMPuttySession _SelectedSession;

    public VMPuttySession SelectedCommandSession {
      get {
        if ( !ObservableCommandSessions.Any() && _SelectedCommandSession == null ) {
          return new VMPuttySession(TPuttySession.Empty);
        }
        return _SelectedCommandSession;
      }
      set {
        _SelectedCommandSession = value;
        NotifyPropertyChanged(nameof(SelectedCommandSession));
      }
    }
    private VMPuttySession _SelectedCommandSession;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public MainViewModel() : base() {
      _InitializeCommands();
      _Initialize();
    }

    private void _Initialize() {
      ObservableSessions = new ObservableCollection<VMPuttySession>();
      ObservableCommandSessions = new ObservableCollection<VMPuttySession>();
      RefreshSessions();
    }

    private void _InitializeCommands() {
      CommandFileOpen = new TRelayCommand(() => FileOpen(), _ => { return true; });
      CommandHelpContact = new TRelayCommand(() => HelpContact(), _ => { return true; });
      CommandHelpAbout = new TRelayCommand(() => HelpAbout(), _ => { return true; });
      CommandRefreshSessions = new TRelayCommand(() => RefreshSessions(), _ => { return !WorkInProgress; });
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

    public void RefreshSessions() {

      WorkInProgress = true;
      CommandRefreshSessions.NotifyCanExecuteChanged();
      Log.Write("Refreshing sessions...");
      NotifyExecutionProgress("Reading sessions...");

      IEnumerable<IPuttySession> CurrentlyRunningSessions = TPuttySession.GetAllRunningSessions();

      TPuttySessionList Sessions = new TPuttySessionList();
      Sessions.ReadSessionsFromRegistry();
      ObservableSessions.Clear();
      ObservableCommandSessions.Clear();

      NotifyInitProgressBar(Sessions.Content.Count);
      int i = 0;
      
      #region --- Sessions --------------------------------------------
      foreach ( IPuttySession SessionItem in Sessions.Content.Where(x => x.Protocol.IsSSH)
                                                             .Where(x => !string.IsNullOrWhiteSpace(((TPuttySessionSSH)x).HostName))
                                                             .Where(x => !x.Name.StartsWith("zzz")) ) {
        NotifyProgressBarNewValue(i++);
        VMPuttySession NewPuttySessionVM = new VMPuttySession(SessionItem);

        IPuttySession Runningsession = CurrentlyRunningSessions.FirstOrDefault(x => x.CleanName.EndsWith($"\"{SessionItem.CleanName}\""));

        if ( Runningsession != null ) {
          NewPuttySessionVM.SetRunningProcess(Runningsession);
        }

        //await Task.Delay(200);
        ObservableSessions.Add(NewPuttySessionVM);
      } 
      #endregion --- Sessions --------------------------------------------

      #region --- Command sessions --------------------------------------------
      foreach ( IPuttySession SessionItem in Sessions.Content.Where(x => x.Protocol.IsSSH)
                                                             .Where(x => !string.IsNullOrWhiteSpace(((TPuttySessionSSH)x).HostName))
                                                             .Where(x => x.Name.StartsWith("zzz")) ) {
        NotifyProgressBarNewValue(i++);
        VMPuttySession NewPuttySessionVM = new VMPuttySession(SessionItem);
        IPuttySession Runningsession = CurrentlyRunningSessions.FirstOrDefault(x => x.CleanName.EndsWith($"\"{SessionItem.CleanName}\""));

        if ( Runningsession != null ) {
          NewPuttySessionVM.SetRunningProcess(Runningsession);
        }
        //await Task.Delay(200);
        ObservableCommandSessions.Add(NewPuttySessionVM);
      }
      #endregion --- Command sessions --------------------------------------------

      NotifyProgressBarCompleted("Done.");
      NotifyExecutionStatus($"{ObservableSessions.Count + ObservableCommandSessions.Count} session(s)");
      Log.Write("Refresh done.");
      WorkInProgress = false;
      CommandRefreshSessions.NotifyCanExecuteChanged();
    }

    public static MainViewModel Design {
      get {
        if ( _Design == null ) {
          _Design = new MainViewModel();
        }
        return _Design;
      }
    }
    private static MainViewModel _Design;
  }
}
