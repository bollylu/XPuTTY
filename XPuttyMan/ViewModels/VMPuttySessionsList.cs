using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools;
using BLTools.MVVM;
using libxputty_std20;
using Microsoft.Win32;

namespace XPuttyMan {
  public class VMPuttySessionsList : MVVMBase, IDisposable {

    public ObservableCollection<VMPuttySession> PuttySessions { get; private set; } = new ObservableCollection<VMPuttySession>();

    #region RelayCommand
    public TRelayCommand CommandReadFromFile { get; private set; }
    public TRelayCommand CommandExport { get; private set; }
    public TRelayCommand CommandImport { get; private set; }
    #endregion RelayCommand

    public bool IsActive { get; set; }

    public VMPuttySession SelectedSession {
      get {
        if ( !PuttySessions.Any() && _SelectedSession == null ) {
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

    public int Count => PuttySessions.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public VMPuttySessionsList() {
      _InitializeCommands();
      _Initialize(new List<VMPuttySession>());
    }

    public VMPuttySessionsList(IEnumerable<VMPuttySession> vmPuttySessions) {
      _InitializeCommands();
      _Initialize(vmPuttySessions);
    }

    private void _InitializeCommands() {
      CommandReadFromFile = new TRelayCommand(() => _ReadFromFile(), _ => true);
      CommandExport = new TRelayCommand(() => _Export(), _ => true);
      CommandImport = new TRelayCommand(() => _Import(), _ => true);

    }

    private void _Initialize(IEnumerable<VMPuttySession> vmPuttySessions) {
      Clear();
    }

    public void Dispose() {
      Clear();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Add(VMPuttySession vmPuttySession) {
      PuttySessions.Add(vmPuttySession);
      vmPuttySession.PropertyChanged += VmPuttySession_PropertyChanged;
      NotifyPropertyChanged(nameof(Count));
    }

    private void VmPuttySession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      SelectedSession = PuttySessions.FirstOrDefault(x => x.IsSelected);
      NotifyPropertyChanged(nameof(SelectedSession));
    }

    public void Clear() {
      foreach ( VMPuttySession PuttySessionItem in PuttySessions ) {
        PuttySessionItem.PropertyChanged -= VmPuttySession_PropertyChanged;
      }
      PuttySessions.Clear();
      NotifyPropertyChanged(nameof(Count));
    }

    private void _ReadFromFile() { }
    private void _Export() {
      WorkInProgress = true;
      Log.Write("Refreshing sessions...");
      NotifyExecutionProgress("Exporting sessions...");

      SaveFileDialog SFD = new SaveFileDialog();
      SFD.DefaultExt = ".xml";
      SFD.Title = "Select a filename to export your data";
      SFD.OverwritePrompt = true;
      if ( SFD.ShowDialog() == true ) {
        TPuttySessionList SessionsToSave = new TPuttySessionList(PuttySessions.Select(x => x.ReadOnlySession));
        SessionsToSave.Export(SFD.SafeFileName);
      }
      NotifyExecutionCompleted("Done.");
      WorkInProgress = false;
    }

    private void _Import() { }



    public static VMPuttySessionsList DesignVMPuttySessionsList {
      get {
        if ( _DesignVMPuttySessionList == null ) {
          _DesignVMPuttySessionList = new VMPuttySessionsList();
          _DesignVMPuttySessionList.Add(VMPuttySession.DesignVMPuttySession);
          _DesignVMPuttySessionList.Add(VMPuttySession.DesignVMPuttySession);
        }
        return _DesignVMPuttySessionList;
      }
    }
    private static VMPuttySessionsList _DesignVMPuttySessionList;


  }
}
