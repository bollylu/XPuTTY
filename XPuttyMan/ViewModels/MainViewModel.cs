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

    public ObservableCollection<TPuttySessionVM> ObservableSessions { get; set; }
    public TPuttySessionVM SelectedSession {
      get {
        if ( !ObservableSessions.Any() && _SelectedSession == null ) {
          return new TPuttySessionVM(new TPuttySession() { Name = "<empty>" });
        }
        return _SelectedSession;
      }
      set {
        _SelectedSession = value;
        NotifyPropertyChanged(nameof(SelectedSession));
      }
    }
    private TPuttySessionVM _SelectedSession;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public MainViewModel() {
      _Initialize();
      _InitializeCommands();
    }

    protected void _Initialize() {
      ObservableSessions = new ObservableCollection<TPuttySessionVM>();
      RefreshSessions();
    }

    protected void _InitializeCommands() {
      CommandFileOpen = new TRelayCommand(() => FileOpen(), _ => { return true; });
      CommandHelpContact = new TRelayCommand(() => HelpContact(), _ => { return true; });
      CommandHelpAbout = new TRelayCommand(() => HelpAbout(), _ => { return true; });
      CommandRefreshSessions = new TRelayCommand(() => RefreshSessions(), _ => { return true; });
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
      Log.Write("Refreshing sessions...");
      NotifyExecutionProgress("Reading sessions...");

      IEnumerable<TPuttySessionVM> CurrentlyRunningSessions = ObservableSessions.Where(x => x.IsRunning);

      TPuttySessionList Sessions = new TPuttySessionList();
      Sessions.ReadFromRegistry();
      ObservableSessions.Clear();
      foreach ( TPuttySession SessionItem in Sessions.Content.Where(x => x.Protocol.IsSSH && !string.IsNullOrWhiteSpace(x.HostName)) ) {
        TPuttySessionVM NewPuttySessionVM = new TPuttySessionVM(SessionItem);
        TPuttySessionVM PotentialRunningSession = CurrentlyRunningSessions.FirstOrDefault(x => x.DisplayName == NewPuttySessionVM.DisplayName);
        if (PotentialRunningSession!=null) {
          NewPuttySessionVM.PID = PotentialRunningSession.PID;
        }
        ObservableSessions.Add(NewPuttySessionVM);
      }
      NotifyExecutionProgress("Done.");
      NotifyExecutionStatus($"{ObservableSessions.Count} session(s)");
      Log.Write("Refresh done.");
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
