using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BLTools;
using EasyPutty.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMPuttySessionsGroup : TVMEasyPuttyBase, IHeaderedItem {

    public TRelayCommand CommandSelectItem { get; private set; }

    public TVMPuttySessionsGroup ParentGroup => GetParent<TVMPuttySessionsGroup>();

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

    public ObservableCollection<IHeaderedItem> Items { get; private set; } = new ObservableCollection<IHeaderedItem>();

    public IHeaderedItem SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private IHeaderedItem _SelectedItem;

    public int Count => Items.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySessionsGroup(string header = "") : base() {
      Header = header;
    }

    protected override void _InitializeCommands() {
      CommandSelectItem = new TRelayCommand(() => _SelectItem(), _ => true);
    }

    protected override void _Initialize() {
    }

    public override void Dispose() {
      Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------


    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{Header} - {Count} item(s)");
      return RetVal.ToString();
    }

    public void Add(IHeaderedItem item) {
      if ( item == null ) {
        return;
      }
      if ( item is IParent ItemWithParent ) {
        ItemWithParent.Parent = this;
      }
      Items.Add(item);
      NotifyPropertyChanged(nameof(Count));
    }

    public void Add(IEnumerable<IHeaderedItem> items) {
      if ( items == null ) {
        return;
      }
      foreach ( IHeaderedItem ItemItem in items ) {
        Add(ItemItem);
      }
      NotifyPropertyChanged(nameof(Count));
    }

    public void Clear() {
      Items.Clear();
      NotifyPropertyChanged(nameof(Count));
    }

    protected void _SelectItem() {
      ParentGroup.SelectedItem = this;
      NotifyExecutionProgress($"Selecting {Header}");
    }

    #region --- Statics for design --------------------------------------------
    public static TVMPuttySessionsGroup DesignVMPuttySessionsNestedGroup {
      get {
        if ( _DesignVMPuttySessionsNestedGroup == null ) {
          _DesignVMPuttySessionsNestedGroup = new TVMPuttySessionsGroup();
          _DesignVMPuttySessionsNestedGroup.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupsL1);
          _DesignVMPuttySessionsNestedGroup.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupsL2);
        }
        return _DesignVMPuttySessionsNestedGroup;
      }
    }
    protected static TVMPuttySessionsGroup _DesignVMPuttySessionsNestedGroup;

    public static TVMPuttySessionsGroup DesignVMPuttySessionsGroup {
      get {
        if ( _DesignVMPuttySessionsGroup == null ) {
          _DesignVMPuttySessionsGroup = new TVMPuttySessionsGroup();
          _DesignVMPuttySessionsGroup.Add(TVMPuttySessionsGroup.DesignVMPuttySessionsNestedGroup);
        }
        return _DesignVMPuttySessionsGroup;
      }
    }
    protected static TVMPuttySessionsGroup _DesignVMPuttySessionsGroup;

    public static IEnumerable<TVMPuttySessionsGroup> DesignVMPuttySessionsGroupsL1 {
      get {
        TVMPuttySessionsGroup L1Group1 = new TVMPuttySessionsGroup("Groupe 1 L1");
        TVMPuttySessionsGroup L2Group1 = new TVMPuttySessionsGroup("Groupe 1 L2");
        L2Group1.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupsWithView);
        L1Group1.Add(L2Group1);
        yield return L1Group1;
        TVMPuttySessionsGroup L1Group2 = new TVMPuttySessionsGroup("Groupe 2 L1");
        TVMPuttySessionsGroup L2Group2 = new TVMPuttySessionsGroup("Groupe 2 L2");
        L2Group2.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupsWithView);
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
    #endregion --- Statics for design --------------------------------------------

  }
}
