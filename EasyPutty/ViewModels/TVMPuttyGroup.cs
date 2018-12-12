using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using BLTools;

using EasyPutty.Interfaces;
using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMPuttyGroup : TVMEasyPuttyBase {

    public TVMPuttyGroup ParentGroup => GetParent<TVMPuttyGroup>();
    public MainViewModel ParentRoot => GetParent<MainViewModel>();

    #region --- IHeaderedAndSelectable --------------------------------------------
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
    public bool DisplaySelectionButton {
      get {
        if ( ParentGroup == null ) {
          return true;
        }
        if ( !ParentGroup.Groups.Any() ) {
          return false;
        }

        if ( ParentGroup.Groups.Count() == 1 ) {
          ParentGroup.SelectedGroup = this;
          return false;
        }

        return true;
      }
    }
    public ICommand CommandSelectItem {
      get; protected set;
    }
    #endregion --- IHeaderedAndSelectable -----------------------------------------

    public CollectionView ItemsView { get; protected set; }

    public ObservableCollection<TVMPuttyGroup> Groups { get; private set; } = new ObservableCollection<TVMPuttyGroup>();
    public ObservableCollection<TVMPuttySession> Sessions { get; private set; } = new ObservableCollection<TVMPuttySession>();

    public TVMPuttyGroup SelectedGroup {
      get {
        if ( _SelectedGroup == null && Groups.Any() ) {
          _SelectedGroup = Groups.First();
        }
        return _SelectedGroup;
      }
      set {
        _SelectedGroup = value;
        if ( ParentRoot != null ) {
          StringBuilder Display = new StringBuilder();
          if ( ParentRoot.PuttyGroup.SelectedGroup is TVMPuttyGroup GroupIterator ) {
            while ( GroupIterator != null ) {
              if ( !GroupIterator.Header.IsEmpty() ) {
                Display.AppendFormat($" > {GroupIterator.Header}");
              }
              GroupIterator = GroupIterator.SelectedGroup;
            }
          }
          ParentRoot.ContentLocation = Display.ToString();
        }
        NotifyPropertyChanged(nameof(SelectedGroup));
      }
    }
    private TVMPuttyGroup _SelectedGroup;

    public TVMPuttySession SelectedSession {
      get {
        return _SelectedSession;
      }
      set {
        _SelectedSession = value;
        NotifyPropertyChanged(nameof(SelectedSession));
      }
    }
    private TVMPuttySession _SelectedSession;

    public int GroupCount => Groups.Count;
    public int SessionCount => Sessions.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttyGroup(string header = "") : base() {
      Header = header;
    }

    public TVMPuttyGroup(IPuttySessionGroup group, string header = "") : base() {
      Header = header;
      Add(group.Groups);
      Add(group.Sessions);
    }

    protected override void _InitializeCommands() {
      CommandSelectItem = new TRelayCommand(() => _SelectGroup(), _ => true);
    }

    protected override void _Initialize() {
      ItemsView = (CollectionView)CollectionViewSource.GetDefaultView(Sessions);
      if ( ItemsView.CanGroup ) {
        PropertyGroupDescription GroupDescription = new PropertyGroupDescription("Section");
        ItemsView.GroupDescriptions.Add(GroupDescription);
      }
    }

    public override void Dispose() {
      lock ( _Lock ) {
        foreach ( TVMPuttyGroup GroupItem in Groups ) {
          GroupItem.Dispose(true);
        }
        foreach ( TVMPuttySession SessionItem in Sessions ) {
          SessionItem.Dispose();
        }
        Clear();
      }
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------


    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{Header} - {GroupCount} item(s)");
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public void Add(IPuttySessionGroup item) {
      if ( item == null ) {
        return;
      }
      TVMPuttyGroup NewGroup = new TVMPuttyGroup(item) {
        Parent = this
      };
      Groups.Add(NewGroup);
      NotifyPropertyChanged(nameof(GroupCount));
    }

    public void Add(IEnumerable<IPuttySessionGroup> items) {
      #region === Validate parameters ===
      if ( items == null ) {
        return;
      }
      #endregion === Validate parameters ===
      foreach ( IPuttySessionGroup GroupItem in items ) {
        Add(GroupItem);
      }
      NotifyPropertyChanged(nameof(GroupCount));
    }

    public void Add(IPuttySession item) {
      if ( item == null ) {
        return;
      }
      TVMPuttySession NewSession = new TVMPuttySession(item) {
        Parent = this
      };
      Sessions.Add(NewSession);
      NotifyPropertyChanged(nameof(SessionCount));
    }

    public void Add(IEnumerable<IPuttySession> items) {
      #region === Validate parameters ===
      if ( items == null ) {
        return;
      }
      #endregion === Validate parameters ===
      foreach ( IPuttySession SessionItem in items ) {
        Add(SessionItem);
      }
      NotifyPropertyChanged(nameof(SessionCount));
    }

    public void Clear() {
      foreach ( TVMPuttyGroup GroupItem in Groups ) {
        GroupItem.Clear();
      }
      Groups.Clear();
      Sessions.Clear();
      NotifyPropertyChanged(nameof(GroupCount));
      NotifyPropertyChanged(nameof(SessionCount));
    }

    protected void _SelectGroup() {
      ParentGroup.SelectedGroup = this;
      NotifyExecutionProgress($"Selecting {Header}");
    }

    #region --- Statics for design --------------------------------------------
    public static TVMPuttyGroup DesignVMPuttyGroupWithNestedGroups {
      get {
        if ( _DesignVMPuttyGroupWithNestedGroups == null ) {
          _DesignVMPuttyGroupWithNestedGroups = new TVMPuttyGroup(TPuttySessionGroup.DemoPuttySessionGroup2, TPuttySessionGroup.DemoPuttySessionGroup2.Name);
        }
        return _DesignVMPuttyGroupWithNestedGroups;
      }
    }
    protected static TVMPuttyGroup _DesignVMPuttyGroupWithNestedGroups;

    public static TVMPuttyGroup DesignVMPuttyGroup {
      get {
        if ( _DesignVMPuttyGroup == null ) {
          _DesignVMPuttyGroup = new TVMPuttyGroup(TPuttySessionGroup.DemoPuttySessionGroup1, TPuttySessionGroup.DemoPuttySessionGroup1.Name);
        }
        return _DesignVMPuttyGroup;
      }
    }
    protected static TVMPuttyGroup _DesignVMPuttyGroup;

    //public static IEnumerable<TVMPuttyGroup> DesignVMPuttyGroupsL1 {
    //  get {
    //    TVMPuttyGroup L1Group1 = new TVMPuttyGroup("Groupe 1 L1");
    //    TVMPuttyGroup L2Group1 = new TVMPuttyGroup("Groupe 1 L2");
    //    L2Group1.Add(TVMPuttySessionsGroupedBy.DesignMultiVMPuttySessionsGroupedBy);
    //    L1Group1.Add(L2Group1);
    //    yield return L1Group1;
    //    TVMPuttyGroup L1Group2 = new TVMPuttyGroup("Groupe 2 L1");
    //    TVMPuttyGroup L2Group2 = new TVMPuttyGroup("Groupe 2 L2");
    //    L2Group2.Add(TVMPuttySessionsGroupedBy.DesignSingleVMPuttySessionsGroupedBy);
    //    L1Group2.Add(L2Group2);
    //    yield return L1Group2;
    //  }
    //}

    //public static IEnumerable<TVMPuttyGroup> DesignVMPuttyGroupsL2 {
    //  get {
    //    TVMPuttyGroup L2Group1 = new TVMPuttyGroup("Groupe 1 L2");
    //    L2Group1.Add(TVMPuttySessionsGroupedBy.DesignSingleVMPuttySessionsGroupedBy);
    //    yield return L2Group1;
    //    TVMPuttyGroup L2Group2 = new TVMPuttyGroup("Groupe 2 L2");
    //    L2Group2.Add(TVMPuttySessionsGroupedBy.DesignMultiVMPuttySessionsGroupedBy);
    //    yield return L2Group2;
    //  }
    //}
    #endregion --- Statics for design --------------------------------------------

  }
}
