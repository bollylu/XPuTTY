using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools.MVVM;
using libxputty_std20;

namespace XPuttyMan {
  public class VMPuttySessionsList : MVVMBase, IDisposable {

    public ObservableCollection<VMPuttySession> PuttySessions { get; private set; } = new ObservableCollection<VMPuttySession>();

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
    }

    public void Dispose() {
      Clear();
    }

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
        if ( _DesignVMPuttySessionList == null ) {
          _DesignVMPuttySessionList = new VMPuttySessionsList();
          _DesignVMPuttySessionList.Add(VMPuttySession.DesignVMPuttySession);
        }
        return _DesignVMPuttySessionList;
      }
    }
    private static VMPuttySessionsList _DesignVMPuttySessionList;


  }
}
