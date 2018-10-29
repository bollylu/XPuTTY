using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BLTools;
using BLTools.MVVM;
using libxputty_std20;
using Microsoft.Win32;

namespace XPuttyMan {
  public class VMPuttySessionsList : MVVMBase, IDisposable {

    public ObservableCollection<VMPuttySession> PuttySessions { get; private set; } = new ObservableCollection<VMPuttySession>();

    public CollectionView PuttySessionsView { get; private set; }

    #region RelayCommand
    #endregion RelayCommand

    public bool IsActive { get; set; }

    public IEnumerable<VMPuttySession> SelectedSessions => PuttySessions.Where(x => x.IsSelected);

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
    }

    private void _Initialize(IEnumerable<VMPuttySession> vmPuttySessions) {
      Clear();
      PuttySessionsView = (CollectionView)CollectionViewSource.GetDefaultView(PuttySessions);
      if ( PuttySessionsView.CanGroup ) {
        PropertyGroupDescription GroupDescription = new PropertyGroupDescription("GroupSection");
        PuttySessionsView.GroupDescriptions.Add(GroupDescription);
      }
    }

    public void Dispose() {
      Clear();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Add(VMPuttySession vmPuttySession) {
      PuttySessions.Add(vmPuttySession);
      NotifyPropertyChanged(nameof(Count));
    }

    public void Clear() {
      PuttySessions.Clear();
      NotifyPropertyChanged(nameof(Count));
    }

    public static VMPuttySessionsList DesignVMPuttySessionsList {
      get {
        if ( _DesignVMPuttySessionsList == null ) {
          _DesignVMPuttySessionsList = new VMPuttySessionsList();
          _DesignVMPuttySessionsList.Add(VMPuttySession.DesignVMPuttySession);
          _DesignVMPuttySessionsList.Add(VMPuttySession.DesignVMPuttySession2);
        }
        return _DesignVMPuttySessionsList;
      }
    }
    private static VMPuttySessionsList _DesignVMPuttySessionsList;


  }
}
