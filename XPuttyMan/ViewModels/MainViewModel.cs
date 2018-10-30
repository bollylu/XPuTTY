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
    public TRelayCommand CommandFileNew { get; private set; }

    public TRelayCommand CommandFileOpenRegistry { get; private set; }
    public TRelayCommand CommandFileOpenXml { get; private set; }
    public TRelayCommand CommandFileOpenJson { get; private set; }

    public TRelayCommand CommandFileSave { get; private set; }

    public TRelayCommand CommandFileSaveRegistry { get; private set; }
    public TRelayCommand CommandFileSaveXml { get; private set; }
    public TRelayCommand CommandFileSaveJson { get; private set; }

    public TRelayCommand CommandHelpContact { get; private set; }
    public TRelayCommand CommandHelpAbout { get; private set; }
    public TRelayCommand CommandRefreshSessions { get; private set; }
    public TRelayCommand CommandStartSession { get; private set; }
    public TRelayCommand CommandToolsExportAll { get; private set; }
    public TRelayCommand CommandToolsExportSelected { get; private set; }
    #endregion RelayCommand

    public string ApplicationTitle {
      get {
        return _ApplicationTitle;
      }
      set {
        _ApplicationTitle = value;
        NotifyPropertyChanged(nameof(ApplicationTitle));
      }
    }
    private string _ApplicationTitle;

    public int TabSelectedIndex {
      get {
        return _TabSelectedIndex;
      }
      set {
        _TabSelectedIndex = value;
        NotifyPropertyChanged(nameof(TabSelectedIndex));
      }
    }
    private int _TabSelectedIndex;

    #region --- Pictures --------------------------------------------
    public string PuttyIcon => App.GetPictureFullname("putty_icon");
    public string FileOpenPicture => App.GetPictureFullname("FileOpen");
    public string FileSavePicture => App.GetPictureFullname("FileSave");
    public string FileQuitPicture => App.GetPictureFullname("FileQuit");
    public string ContactPicture => App.GetPictureFullname("help"); 
    #endregion --- Pictures --------------------------------------------

    public VMPuttySessionsList ObservableSessions { get; set; }
    public VMPuttySessionsList ObservableCommandSessions { get; set; }

    private string DataSourceName;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public MainViewModel() : base() {
      ApplicationTitle = "EasyPutty";
      NotifyPropertyChanged(nameof(_ApplicationTitle));
      _InitializeCommands();
      _Initialize();
    }

    private void _Initialize() {
      ObservableSessions = new VMPuttySessionsList();
      ObservableCommandSessions = new VMPuttySessionsList();
      _RefreshSessionsFromRegistry();
    }

    private void _InitializeCommands() {
      CommandFileNew = new TRelayCommand(() => _FileNew(), _ => { return !WorkInProgress && !MainWindow.DataIsDirty; });

      CommandFileOpenRegistry = new TRelayCommand(() => _FileOpenRegistry(), _ => { return !WorkInProgress && !MainWindow.DataIsDirty; });
      CommandFileOpenXml = new TRelayCommand(() => _FileOpenXml(), _ => { return !WorkInProgress && !MainWindow.DataIsDirty; });
      CommandFileOpenJson = new TRelayCommand(() => _FileOpenJson(), _ => { return !WorkInProgress && !MainWindow.DataIsDirty; });

      CommandFileSave = new TRelayCommand(() => _FileSave(), _ => { return !WorkInProgress && MainWindow.DataIsDirty; });

      CommandFileSaveRegistry = new TRelayCommand(() => _FileSaveRegistry(), _ => { return !WorkInProgress && MainWindow.DataIsDirty; });
      CommandFileSaveXml = new TRelayCommand(() => _FileSaveXml(), _ => { return !WorkInProgress && MainWindow.DataIsDirty; });
      CommandFileSaveJson = new TRelayCommand(() => _FileSaveJson(), _ => { return !WorkInProgress && MainWindow.DataIsDirty; });

      CommandHelpContact = new TRelayCommand(() => _HelpContact(), _ => { return true; });
      CommandHelpAbout = new TRelayCommand(() => _HelpAbout(), _ => { return true; });
      CommandRefreshSessions = new TRelayCommand(() => _RefreshSessionsFromRegistry(), _ => { return !WorkInProgress; });
      CommandToolsExportAll = new TRelayCommand(() => _ExportAll(), _ => { return !WorkInProgress; });
      CommandToolsExportSelected = new TRelayCommand(() => _ExportSelected(), _ => { return !WorkInProgress; });
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Menu --------------------------------------------
    private void _FileNew() {
      MainWindow.DataIsDirty = false;
      DataSourceName = "";
      ObservableSessions = new VMPuttySessionsList();
      NotifyPropertyChanged(nameof(ObservableSessions));
      ObservableCommandSessions = new VMPuttySessionsList();
      NotifyPropertyChanged(nameof(ObservableCommandSessions));
      TabSelectedIndex = -1;
    }
    private void _FileOpenXml() {
      OpenFileDialog OFD = new OpenFileDialog {
        Title = "Select new sessions file",
        Filter = "Parameter file|*.xml",
        CheckFileExists = true,
        CheckPathExists = true,
        AddExtension = true,
        DefaultExt = ".xml",
        ValidateNames = true
      };
      if ( OFD.ShowDialog() != true ) {
        return;
      }
    }
    private void _FileOpenJson() {
      OpenFileDialog OFD = new OpenFileDialog {
        Title = "Select new sessions file",
        Filter = "Parameter file|*.json",
        CheckFileExists = true,
        CheckPathExists = true,
        AddExtension = true,
        DefaultExt = ".json",
        ValidateNames = true
      };
      if ( OFD.ShowDialog() != true ) {
        return;
      }
    }
    private void _FileOpenRegistry() {
    }

    private void _FileSave() {
      MainWindow.DataIsDirty = false;
    }

    private void _FileSaveXml() {
      WorkInProgress = true;
      Log.Write("Exporting all sessions...");
      NotifyExecutionProgress("Exporting sessions...");

      SaveFileDialog SFD = new SaveFileDialog {
        DefaultExt = ".xml",
        Title = "Select a filename to export your data",
        OverwritePrompt = true,
        AddExtension = true
      };
      SFD.DefaultExt = ".xml";
      SFD.Filter = "XML files (.xml)|*.xml";

      if ( SFD.ShowDialog() == true ) {
        TPuttySessionList SessionsToSave = new TPuttySessionList(ObservableSessions.PuttySessions.Select(x => x.ReadOnlySession));
        TPuttySessionList CommandSessionsToSave = new TPuttySessionList(ObservableCommandSessions.PuttySessions.Select(x => x.ReadOnlySession));
        TPuttySessionList AllSessionsToSave = new TPuttySessionList(SessionsToSave.Items.Union(CommandSessionsToSave.Items));
        AllSessionsToSave.ExportToXml(SFD.FileName);
      }

      NotifyExecutionCompleted("Done.");
      MainWindow.DataIsDirty = false;
      WorkInProgress = false;
    }
    private void _FileSaveJson() {
      OpenFileDialog OFD = new OpenFileDialog {
        Title = "Select new sessions file",
        Filter = "Parameter file|*.json",
        CheckFileExists = true,
        CheckPathExists = true,
        AddExtension = true,
        DefaultExt = ".json",
        ValidateNames = true
      };
      if ( OFD.ShowDialog() != true ) {
        return;
      }

      MainWindow.DataIsDirty = false;
    }
    private void _FileSaveRegistry() {
      MainWindow.DataIsDirty = false;
    }

    private void _HelpContact() {

    }
    private void _HelpAbout() {
      StringBuilder Usage = new StringBuilder();
      Usage.AppendLine($"xPuttyMan v{Assembly.GetEntryAssembly().GetName().Version.ToString()}");
      Usage.AppendLine(@"Usage: xPuttyMan [/config=<config.xml> (default=<none>)]");
      Usage.AppendLine(@"                 [/logbase=<[\\server\share\]path> (default=c:\logs\xputtyman)]");
      Usage.AppendLine(@"                 [/log=<filename.log> (default=xputtyman.log)]");
      MessageBox.Show(Usage.ToString());
    }
    #endregion --- Menu --------------------------------------------

    private void _ExportAll() {
      WorkInProgress = true;
      Log.Write("Exporting all sessions...");
      NotifyExecutionProgress("Exporting sessions...");

      SaveFileDialog SFD = new SaveFileDialog {
        DefaultExt = ".xml",
        Title = "Select a filename to export your data",
        OverwritePrompt = true,
        AddExtension = true
      };
      SFD.DefaultExt = ".xml";
      SFD.Filter = "XML files (.xml)|*.xml";

      if ( SFD.ShowDialog() == true ) {
        TPuttySessionList SessionsToSave = new TPuttySessionList(ObservableSessions.PuttySessions.Select(x => x.ReadOnlySession));
        TPuttySessionList CommandSessionsToSave = new TPuttySessionList(ObservableCommandSessions.PuttySessions.Select(x => x.ReadOnlySession));
        TPuttySessionList AllSessionsToSave = new TPuttySessionList(SessionsToSave.Items.Union(CommandSessionsToSave.Items));
        AllSessionsToSave.ExportToXml(SFD.FileName);
      }

      NotifyExecutionCompleted("Done.");
      WorkInProgress = false;
    }
    private void _ExportSelected() {
      WorkInProgress = true;
      Log.Write("Exporting selected sessions...");
      NotifyExecutionProgress("Exporting selected sessions...");

      SaveFileDialog SFD = new SaveFileDialog {
        DefaultExt = ".xml",
        Title = "Select a filename to export your data",
        OverwritePrompt = true,
        AddExtension = true
      };
      SFD.DefaultExt = ".xml";
      SFD.Filter = "XML files (.xml)|*.xml";

      if ( SFD.ShowDialog() == true ) {
        TPuttySessionList SessionsToSave = new TPuttySessionList(ObservableSessions.SelectedSessions.Select(x => x.ReadOnlySession));
        TPuttySessionList CommandSessionsToSave = new TPuttySessionList(ObservableCommandSessions.SelectedSessions.Select(x => x.ReadOnlySession));
        TPuttySessionList AllSessionsToSave = new TPuttySessionList(SessionsToSave.Items.Union(CommandSessionsToSave.Items));
        AllSessionsToSave.ExportToXml(SFD.FileName);
      }

      NotifyExecutionCompleted("Done.");
      WorkInProgress = false;
    }

    private void _RefreshSessionsFromRegistry() {

      WorkInProgress = true;
      CommandRefreshSessions.NotifyCanExecuteChanged();
      Log.Write("Refreshing sessions...");
      NotifyExecutionProgress("Reading sessions...");

      TPuttySessionList Sessions = new TPuttySessionList();
      Sessions.ReadSessionsFromRegistry();

      ObservableSessions = _CreateAndRecoverSessions(Sessions, TPuttyProtocol.SSH, x => !x.Name.StartsWith("#CMD/"));
      ObservableCommandSessions = _CreateAndRecoverSessions(Sessions, TPuttyProtocol.SSH, x => x.Name.StartsWith("#CMD/"));

      NotifyExecutionStatus($"{ObservableSessions.Count + ObservableCommandSessions.Count} session(s)");
      Log.Write("Refresh done.");
      WorkInProgress = false;
      CommandRefreshSessions.NotifyCanExecuteChanged();

    }

    private VMPuttySessionsList _CreateAndRecoverSessions(TPuttySessionList sessions, TPuttyProtocol protocol, Func<IPuttySession, bool> predicate) {
      if ( sessions == null ) {
        return null;
      }

      VMPuttySessionsList RetVal = new VMPuttySessionsList();

      if ( !sessions.Items.Any() ) {
        return new VMPuttySessionsList();
      }

      IEnumerable<Process> CurrentlyRunningSessions = TPuttySession.GetAllPuttyProcess();

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

    private bool _LoadConfig(string filename) {

      //if ( SupportContacts != null ) {
      //  SupportContacts.Items.Clear();
      //}

      if ( string.IsNullOrWhiteSpace(filename) ) {
        NotifyExecutionError("Unable to read parameter file : filename is null or empty", ErrorLevel.Warning);
        return false;
      }

      if ( !File.Exists(filename) ) {
        NotifyExecutionError("Unable to read parameter file : filename is invalid or access is denied", ErrorLevel.Warning);
      }

      NotifyExecutionProgress($"Loading parameter file : {filename}");

      //ConfigContent = new TConfigFile(filename);
      //ConfigContent.LoadXml();
      //if ( ConfigContent.Groups.Count == 0 ) {
      //  NotifyExecutionError("Invalid parameter file : at least one group must be defined", ErrorLevel.Warning);
      //  return false;
      //}

      //ConfigContent.CheckSecurity();

      //SupportContacts = new TVMSupportContacts(ConfigContent.SupportContacts);

      //if ( Items.Count > 0 ) {
      //  TabSelectedIndex = 0;
      //} else {
      //  TabSelectedIndex = -1;
      //}

      NotifyExecutionCompleted("Parameters loaded", true);

      return true;
    }

    #region --- For design time --------------------------------------------
    public static MainViewModel DesignMainViewModel {
      get {
        if ( _DesignMainViewModel == null ) {
          _DesignMainViewModel = new MainViewModel();
        }
        return _DesignMainViewModel;
      }
    }
    private static MainViewModel _DesignMainViewModel;
    #endregion --- For design time --------------------------------------------
  }
}
