using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EasyPutty.ViewModels {
  public class TVMPuttySessionsGroup : TVMEasyPuttyBase {

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

    public ObservableCollection<TVMPuttySessionsGroup> PuttyGroups { get; private set; } = new ObservableCollection<TVMPuttySessionsGroup>();

    public bool IsActive { get; set; }

    public IEnumerable<TVMPuttySession> SelectedSessions => PuttySessions.Where(x => x.IsSelected);

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

    public int SessionsCount => PuttySessions.Count;
    public int GroupsCount => PuttyGroups.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySessionsGroup(string header = "") : base() {
      Header = header;
      Add(new List<TVMPuttySession>());
    }

    public TVMPuttySessionsGroup(string header, IEnumerable<TVMPuttySession> vmPuttySessions) : base() {
      Header = header;
      Add(vmPuttySessions);
    }

    protected override void _InitializeCommands() {
    }

    protected override void _Initialize() {
    }

    public override void Dispose() {
      Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void Add(TVMPuttySession vmPuttySession) {
      PuttySessions.Add(vmPuttySession);
      NotifyPropertyChanged(nameof(SessionsCount));
    }

    public void Add(IEnumerable<TVMPuttySession> vmPuttySessions) {
      if ( vmPuttySessions == null ) {
        return;
      }
      foreach ( TVMPuttySession VMPuttySessionItem in vmPuttySessions ) {
        PuttySessions.Add(VMPuttySessionItem);
      }
      NotifyPropertyChanged(nameof(SessionsCount));
    }

    public void Add(TVMPuttySessionsGroup vmPuttySessionGroup) {
      if ( vmPuttySessionGroup == null ) {
        return;
      }
      PuttyGroups.Add(vmPuttySessionGroup);
      NotifyPropertyChanged(nameof(GroupsCount));
    }

    public void Add(IEnumerable<TVMPuttySessionsGroup> vmPuttySessionGroups) {
      if ( vmPuttySessionGroups == null ) {
        return;
      }
      foreach ( TVMPuttySessionsGroup VMPuttySessionGroupItem in vmPuttySessionGroups ) {
        PuttyGroups.Add(VMPuttySessionGroupItem);
      }
      NotifyPropertyChanged(nameof(GroupsCount));
    }

    public void Clear() {
      PuttySessions.Clear();
      NotifyPropertyChanged(nameof(SessionsCount));
      PuttyGroups.Clear();
      NotifyPropertyChanged(nameof(GroupsCount));
    }

    public static TVMPuttySessionsGroup DesignVMPuttySessionsGroup {
      get {
        if ( _DesignVMPuttySessionsGroup == null ) {
          _DesignVMPuttySessionsGroup = new TVMPuttySessionsGroup();
          _DesignVMPuttySessionsGroup.Add(TVMPuttySession.DesignVMPuttySession);
          _DesignVMPuttySessionsGroup.Add(TVMPuttySession.DesignVMPuttySession2);
        }
        return _DesignVMPuttySessionsGroup;
      }
    }
    private static TVMPuttySessionsGroup _DesignVMPuttySessionsGroup;

    public static IEnumerable<TVMPuttySessionsGroup> DesignVMPuttySessionsGroupsL1 {
      get {
        TVMPuttySessionsGroup L1Group1 = new TVMPuttySessionsGroup("Groupe 1 L1");
        TVMPuttySessionsGroup L2Group1 = new TVMPuttySessionsGroup("Groupe 1 L2");
        TVMPuttySessionsGroupWithView L3Group1 = new TVMPuttySessionsGroupWithView("Groupe 1 Sections");
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession2);
        L2Group1.Add(L3Group1);
        L1Group1.Add(L2Group1);
        yield return L1Group1;
        TVMPuttySessionsGroup L1Group2 = new TVMPuttySessionsGroup("Groupe 2 L1");
        TVMPuttySessionsGroup L2Group2 = new TVMPuttySessionsGroup("Groupe 2 L2");
        TVMPuttySessionsGroupWithView L3Group2 = new TVMPuttySessionsGroupWithView("Groupe 2 Sections");
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession2);
        L2Group2.Add(L3Group2);
        L1Group2.Add(L2Group2);
        yield return L1Group2;
      }
    }

    public static IEnumerable<TVMPuttySessionsGroup> DesignVMPuttySessionsGroupsL2 {
      get {
        TVMPuttySessionsGroup L2Group1 = new TVMPuttySessionsGroup("Groupe 1 L2");
        TVMPuttySessionsGroupWithView L3Group1 = new TVMPuttySessionsGroupWithView("Groupe 1 Sections");
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession2);
        L2Group1.Add(L3Group1);
        yield return L2Group1;
        TVMPuttySessionsGroup L2Group2 = new TVMPuttySessionsGroup("Groupe 2 L2");
        TVMPuttySessionsGroupWithView L3Group2 = new TVMPuttySessionsGroupWithView("Groupe 2 Sections");
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession2);
        L2Group2.Add(L3Group2);
        yield return L2Group2;
      }
    }

  }
}
