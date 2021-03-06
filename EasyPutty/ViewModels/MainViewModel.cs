﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

using BLTools;

using EasyPutty.Properties;

using libxputty_std20;
using libxputty_std20.Interfaces;

using Microsoft.Win32;

namespace EasyPutty.ViewModels {
  public class MainViewModel : TVMEasyPuttyBase {

    #region RelayCommand
    public TRelayCommand CommandFileNew { get; private set; }

    public TRelayCommand CommandFileOpenRegistry { get; private set; }
    public TRelayCommand CommandFileOpenXml { get; private set; }
    public TRelayCommand CommandFileOpenJson { get; private set; }

    public TRelayCommand CommandFileSave { get; private set; }

    public TRelayCommand CommandFileSaveRegistry { get; private set; }
    public TRelayCommand CommandFileSaveXml { get; private set; }
    public TRelayCommand CommandFileSaveJson { get; private set; }

    public TRelayCommand CommandFileQuit { get; private set; }

    public TRelayCommand CommandHelpContact { get; private set; }
    public TRelayCommand CommandHelpAbout { get; private set; }
    //public TRelayCommand CommandStartSession { get; private set; }
    //public TRelayCommand CommandToolsExportAll { get; private set; }
    //public TRelayCommand CommandToolsExportSelected { get; private set; }
    #endregion RelayCommand

    public bool DataIsDirty {
      get {
        return _DataIsDirty;
      }
      set {
        _DataIsDirty = value;
        NotifyPropertyChanged(nameof(DataIsDirty));
      }
    }
    private bool _DataIsDirty;

    public string ApplicationTitle {
      get {
        return $"{_ApplicationTitleBase} : {_ApplicationTitle} : {SessionSourceName}";
      }
      set {
        _ApplicationTitle = value;
        NotifyPropertyChanged(nameof(ApplicationTitle));
      }
    }
    private string _ApplicationTitle;

    private readonly string _ApplicationTitleBase = $"{App.AppName} v0.1";

    public TVMPuttyGroup SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        ContentLocation = _SelectedItem.Header;
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private TVMPuttyGroup _SelectedItem;

    public string ContentLocation {
      get {
        return _ContentLocation;
      }
      set {
        _ContentLocation = value;
        NotifyPropertyChanged(nameof(ContentLocation));
      }
    }
    private string _ContentLocation;

    #region --- Pictures --------------------------------------------
    public string PuttyIcon => App.GetPictureFullname("putty_icon");
    public string FileOpenPicture => App.GetPictureFullname("FileOpen");
    public string FileSavePicture => App.GetPictureFullname("FileSave");
    public string FileQuitPicture => App.GetPictureFullname("FileQuit");
    public string ContactPicture => App.GetPictureFullname("help");
    #endregion --- Pictures --------------------------------------------

    public TVMPuttyGroup PuttyGroup { get; private set; } = new TVMPuttyGroup("Main");

    public IEnumerable<TVMPuttySession> AllVMPuttySessions => PuttyGroup.Groups
                                                                        .SelectMany(x => x.Groups)
                                                                        .SelectMany(x => x.Sessions);
    public IEnumerable<IPuttySession> AllPuttySessions => AllVMPuttySessions.Select(x => x.PuttySession);
    public int TotalSessionsCount => AllVMPuttySessions.Count();

    public string SessionSourceName => SessionSource == null ? "" : SessionSource.DataSourceName;

    public IPuttySessionSource SessionSource {
      get {
        return _SessionSource;
      }
      set {
        _SessionSource = value;
        Settings CurrentSettings = new Settings();
        if ( _SessionSource != null ) {
          if ( CurrentSettings.LastDataSource != _SessionSource.DataSourceName ) {
            CurrentSettings.LastDataSource = $"{_SessionSource.DataSourceName}";
          }
        } else {
          CurrentSettings.LastDataSource = "";
        }
        CurrentSettings.Save();
        NotifyPropertyChanged(nameof(SessionSourceName));
        NotifyPropertyChanged(nameof(ApplicationTitle));
        NotifyPropertyChanged(nameof(IsSessionSourceNotRegistry));
      }
    }
    private IPuttySessionSource _SessionSource;

    public bool IsSessionSourceNotRegistry => !(SessionSource is TPuttySessionSourceRegistry);

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public MainViewModel() : base() {
    }

    protected override void _Initialize() {
      if ( App.CurrentUserCredential != null ) {
        ApplicationTitle = App.AppUsername;
      }
      PuttyGroup.Parent = this;

      if (App.AppArgs.IsDefined(App.PARAM_LOAD)) {
        #region --- Load data from PuttySessionSource --------------------------------------------
        Settings CurrentSettings = new Settings();

        string NewDataSource = App.AppArgs.GetValue<string>(App.PARAM_LOAD, "");
        if (NewDataSource=="") {
          Log.Write("Invalid parameter : /Load");
          return;
        }

        SessionSource = TPuttySessionSource.GetPuttySessionSource(NewDataSource);
        if ( SessionSource != null ) {
          _DispatchSessions(SessionSource.GetSessions().Where(x => x.Protocol.IsSSH));
        }
        CurrentSettings.LastDataSource = NewDataSource;
        CurrentSettings.Save();

        if ( PuttyGroup.Items.Any() ) {
          PuttyGroup.SelectedItem = PuttyGroup.Items.First();
        }
        #endregion --- Load data from PuttySessionSource --------------------------------------------
      } else {
        #region --- Reload data from previous PuttySessionSource --------------------------------------------
        Settings CurrentSettings = new Settings();
        CurrentSettings.Reload();

        if ( string.IsNullOrWhiteSpace(CurrentSettings.LastDataSource) ) {
          return;
        }

        SessionSource = TPuttySessionSource.GetPuttySessionSource(CurrentSettings.LastDataSource);
        if ( SessionSource != null ) {
          _DispatchSessions(SessionSource.GetSessions().Where(x => x.Protocol.IsSSH));
        }

        if ( PuttyGroup.Items.Any() ) {
          PuttyGroup.SelectedItem = PuttyGroup.Items.First();
        }
        #endregion --- Reload data from previous PuttySessionSource --------------------------------------------
      }

    }

    protected override void _InitializeCommands() {
      CommandFileNew = new TRelayCommand(() => _FileNew(), _ => { return !WorkInProgress; });

      CommandFileOpenRegistry = new TRelayCommand(() => _FileOpenRegistry(), _ => { return !WorkInProgress; });
      CommandFileOpenXml = new TRelayCommand(() => _FileOpenXml(), _ => { return !WorkInProgress; });
      CommandFileOpenJson = new TRelayCommand(() => _FileOpenJson(), _ => { return !WorkInProgress; });

      CommandFileSave = new TRelayCommand(() => _FileSave(), _ => { return !WorkInProgress; });

      CommandFileSaveRegistry = new TRelayCommand(() => _FileSaveRegistry(), _ => { return !WorkInProgress; });
      CommandFileSaveXml = new TRelayCommand(() => _FileSaveXml(), _ => { return !WorkInProgress; });
      CommandFileSaveJson = new TRelayCommand(() => _FileSaveJson(), _ => { return !WorkInProgress; });

      CommandFileQuit = new TRelayCommand(() => _FileQuit(), _ => { return true; });

      CommandHelpContact = new TRelayCommand(() => _HelpContact(), _ => { return true; });
      CommandHelpAbout = new TRelayCommand(() => _HelpAbout(), _ => { return true; });
    }

    public override void Dispose() {
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Menu --------------------------------------------
    private void _FileNew() {
      DataIsDirty = false;
      PuttyGroup.Clear();
      SessionSource = null;
    }

    #region --- File Open --------------------------------------------
    private void _FileOpenXml() {
      WorkInProgress = true;
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
        WorkInProgress = false;
        return;
      }
      SessionSource = new TPuttySessionSourceXml(OFD.FileName);

      Log.Write($"Refreshing sessions from XML file {OFD.FileName}");
      NotifyExecutionProgress("Reading sessions ...");
      PuttyGroup.Clear();

      _DispatchSessions(SessionSource.GetSessions().Where(x => x.Protocol.IsSSH));

      if (PuttyGroup.Items.Any()) {
        PuttyGroup.SelectedItem = PuttyGroup.Items.First();
      }

      NotifyExecutionStatus($"{TotalSessionsCount} session(s)");
      Log.Write("Refresh done.");
      WorkInProgress = false;
      NotifyExecutionCompleted("Done.", true);
    }

    private void _FileOpenJson() {

      //OpenFileDialog OFD = new OpenFileDialog {
      //  Title = "Select new sessions file",
      //  Filter = "Parameter file|*.json",
      //  CheckFileExists = true,
      //  CheckPathExists = true,
      //  AddExtension = true,
      //  DefaultExt = ".json",
      //  ValidateNames = true
      //};
      //if ( OFD.ShowDialog() != true ) {
      //  return;
      //}

    }

    private void _FileOpenRegistry() {
      WorkInProgress = true;
      SessionSource = new TPuttySessionSourceRegistry();
      NotifyPropertyChanged(nameof(ApplicationTitle));
      CommandFileOpenRegistry.NotifyCanExecuteChanged();
      Log.Write("Refreshing sessions from registry ...");
      NotifyExecutionProgress("Reading sessions ...");

      PuttyGroup.Clear();

      _DispatchSessions(SessionSource.GetSessions().Where(x => x.Protocol.IsSSH));

      NotifyExecutionStatus($"{TotalSessionsCount} session(s)");
      Log.Write("Refresh done.");
      WorkInProgress = false;
      CommandFileOpenRegistry.NotifyCanExecuteChanged();
      NotifyExecutionCompleted("Done.", true);
    }
    #endregion --- File Open --------------------------------------------

    #region --- File Save --------------------------------------------
    private void _FileSave() {
      if ( DataIsDirty ) {
        WorkInProgress = true;
        SessionSource.SaveSessions(AllPuttySessions);
        DataIsDirty = false;
        WorkInProgress = false;
      }
    }

    private void _FileSaveXml() {
      WorkInProgress = true;

      Log.Write("Saving all sessions to XML");
      NotifyExecutionProgress("Save sessions to XML");

      SaveFileDialog SFD = new SaveFileDialog {
        DefaultExt = ".xml",
        Title = "Select a filename to export your data",
        OverwritePrompt = true,
        AddExtension = true
      };
      SFD.DefaultExt = ".xml";
      SFD.Filter = "XML files (.xml)|*.xml";

      if ( SFD.ShowDialog() == true ) {
        SessionSource = new TPuttySessionSourceXml(SFD.FileName);
        SessionSource.SaveSessions(AllPuttySessions);
        DataIsDirty = false;
      }

      NotifyExecutionCompleted("Done.");
      DataIsDirty = false;
      WorkInProgress = false;
    }

    private void _FileSaveJson() {
      //WorkInProgress = true;
      //Log.Write("Saving all sessions to Json...");

      //SaveFileDialog SFD = new SaveFileDialog {
      //  DefaultExt = ".json",
      //  Title = "Select a filename to export your data",
      //  OverwritePrompt = true,
      //  AddExtension = true
      //};
      //SFD.DefaultExt = ".json";
      //SFD.Filter = "JSON files (.json)|*.json";

      //if ( SFD.ShowDialog() == true ) {
      //  //TPuttySessionList SessionsToSave = new TPuttySessionList(SessionsTabs.SelectMany(x => x.PuttyGroups).SelectMany(x => x.PuttySessions).Select(x => x.PuttySession));
      //  //SessionsToSave.SaveToJson(SFD.FileName);
      //}

      //NotifyExecutionCompleted("Done.");
      //MainWindow.DataIsDirty = false;
      //WorkInProgress = false;
    }

    private void _FileSaveRegistry() {
      WorkInProgress = true;

      Log.Write("Saving all sessions to XML");
      NotifyExecutionProgress("Save sessions to XML");
      SessionSource = new TPuttySessionSourceRegistry();
      SessionSource.SaveSessions(AllPuttySessions);
      NotifyExecutionCompleted("Done.");
      DataIsDirty = false;

      WorkInProgress = false;
    }
    #endregion --- File Save --------------------------------------------

    private void _FileQuit() {
      Application.Current.Shutdown();
    }

    private void _HelpContact() {

    }

    private void _HelpAbout() {
      StringBuilder Usage = new StringBuilder();
      Usage.AppendLine($"EasyPutty v{Assembly.GetEntryAssembly().GetName().Version.ToString()}");
      Usage.AppendLine(@"Usage: EasyPutty [/config=<config.xml> (default=<none>)]");
      Usage.AppendLine(@"                 [/logbase=<[\\server\share\]path> (default=c:\logs\easyputty)]");
      Usage.AppendLine(@"                 [/log=<filename.log> (default=easyputty.log)]");
      MessageBox.Show(Usage.ToString());
    }
    #endregion --- Menu --------------------------------------------

    private void _DispatchSessions(IEnumerable<IPuttySession> sessions) {

      IEnumerable<IGrouping<string, IPuttySession>> SessionsByGroupL1 = sessions.Where(x => !(string.IsNullOrWhiteSpace(x.GroupLevel1) && string.IsNullOrWhiteSpace(x.GroupLevel2)))
                                                                                .GroupBy(x => x.GroupLevel1);

      foreach ( IGrouping<string, IPuttySession> SessionsByGroupL1Item in SessionsByGroupL1 ) {

        string L1Header = SessionsByGroupL1Item.First().GroupLevel1 ?? "<empty>";
        TVMPuttyGroup GroupL1 = new TVMPuttyGroup(L1Header) {
          Parent = PuttyGroup
        };

        foreach ( IGrouping<string, IPuttySession> SessionsByGroupL2Item in SessionsByGroupL1Item.OrderBy(x => x.GroupLevel2).GroupBy(x => x.GroupLevel2) ) {

          string L2Header = SessionsByGroupL2Item.First().GroupLevel2 ?? "<empty>";
          TVMPuttySessionsGroupedBy GroupL2 = new TVMPuttySessionsGroupedBy(L2Header) {
            Parent = GroupL1
          };

          GroupL2.Add(_CreateAndRecoverSessions(SessionsByGroupL2Item, EPuttyProtocol.SSH));
          GroupL1.Add(GroupL2);

        }

        PuttyGroup.Add(GroupL1);
      }
      NotifyExecutionStatus($"{TotalSessionsCount} session(s)");
    }

    private IEnumerable<TVMPuttySession> _CreateAndRecoverSessions(IEnumerable<IPuttySession> sessions, EPuttyProtocol protocol) {
      if ( sessions == null || !sessions.Any() ) {
        yield break;
      }

      IEnumerable<Process> CurrentlyRunningSessions = TPuttySession.GetAllPuttyProcess();

      foreach ( IPuttySession SessionItem in sessions.Where(x => x.Protocol.Value == protocol)
                                                     .OfType<IHostAndPort>()
                                                     .Where(x => !string.IsNullOrWhiteSpace(x.HostName))
                                                     ) {
        IHostAndPort SessionHAP = SessionItem as IHostAndPort;
        string SessionCommandLineWithoutRemoteCommand = string.Join(" ", CommandLineBuilder.BuildSSHCommandLine()
                                                                                           .AddCredentialsToCommandLine(SessionItem.Credential)
                                                                                           .AddHostnameAndPort(SessionHAP.HostName, SessionHAP.Port));
        Process RunningSession = CurrentlyRunningSessions.FirstOrDefault(x => TRunProcess.GetCommandLine(x.Id).Contains(SessionCommandLineWithoutRemoteCommand));

        TVMPuttySession NewPuttySessionVM;
        NewPuttySessionVM = new TVMPuttySession(SessionItem);
        if ( RunningSession != null ) {
          NewPuttySessionVM.AssignProcess(RunningSession);
        }

        yield return NewPuttySessionVM;
      }

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
