using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace EasyPutty.ViewModels {
  public class TVMPuttySessionsList : TVMEasyPuttyBase {

    public string Header {
      get {
        return _Header;
      }
      set {
        _Header = value;
        NotifyPropertyChanged(_Header);
      }
    }
    private string _Header;

    public ObservableCollection<TVMPuttySession> PuttySessions { get; private set; } = new ObservableCollection<TVMPuttySession>();

    public CollectionView PuttySessionsView { get; private set; }

    #region RelayCommand
    #endregion RelayCommand

    public bool IsActive { get; set; }

    public IEnumerable<TVMPuttySession> SelectedSessions => PuttySessions.Where(x => x.IsSelected);

    public int Count => PuttySessions.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySessionsList(string header = "") : base() {
      Header = header;
      _Initialize(new List<TVMPuttySession>());
    }

    public TVMPuttySessionsList(string header, IEnumerable<TVMPuttySession> vmPuttySessions) : base() {
      Header = header;
      _Initialize(vmPuttySessions);
    }

    protected override void _InitializeCommands() {
    }

    protected override void _Initialize() {
    }

    protected void _Initialize(IEnumerable<TVMPuttySession> vmPuttySessions) {
      Clear();
      PuttySessionsView = (CollectionView)CollectionViewSource.GetDefaultView(PuttySessions);
      if ( PuttySessionsView.CanGroup ) {
        PropertyGroupDescription GroupDescription = new PropertyGroupDescription("GroupSection");
        PuttySessionsView.GroupDescriptions.Add(GroupDescription);
      }
    }

    public override void Dispose() {
      Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Add(TVMPuttySession vmPuttySession) {
      PuttySessions.Add(vmPuttySession);
      NotifyPropertyChanged(nameof(Count));
    }

    public void Add(TVMPuttySessionsList vmPuttySessions) {
      if ( vmPuttySessions == null ) {
        return;
      }
      foreach ( TVMPuttySession VMPuttySessionItem in vmPuttySessions.PuttySessions ) {
        PuttySessions.Add(VMPuttySessionItem);
      }
      NotifyPropertyChanged(nameof(Count));
    }

    public void Clear() {
      PuttySessions.Clear();
      NotifyPropertyChanged(nameof(Count));
    }

    public static TVMPuttySessionsList DesignVMPuttySessionsList {
      get {
        if ( _DesignVMPuttySessionsList == null ) {
          _DesignVMPuttySessionsList = new TVMPuttySessionsList();
          _DesignVMPuttySessionsList.Add(TVMPuttySession.DesignVMPuttySession);
          _DesignVMPuttySessionsList.Add(TVMPuttySession.DesignVMPuttySession2);
        }
        return _DesignVMPuttySessionsList;
      }
    }
    private static TVMPuttySessionsList _DesignVMPuttySessionsList;


  }
}
