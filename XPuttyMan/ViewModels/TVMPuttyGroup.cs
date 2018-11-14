using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BLTools;
using EasyPutty.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMPuttyGroup : TVMEasyPuttyBase, IHeader {

    public TRelayCommand CommandSelectItem { get; private set; }

    public TVMPuttyGroup ParentGroup => GetParent<TVMPuttyGroup>();
    public MainViewModel ParentRoot => GetParent<MainViewModel>();

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

    public ObservableCollection<IHeader> Items { get; private set; } = new ObservableCollection<IHeader>();

    public IEnumerable<TVMPuttyGroup> Groups => Items.OfType<TVMPuttyGroup>();
    public IEnumerable<TVMPuttySession> Sessions => Items.OfType<TVMPuttySession>();

    public TVMPuttyGroup SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
        if ( ParentRoot != null ) {
          StringBuilder Display = new StringBuilder();
          TVMPuttyGroup GroupIterator = ParentRoot.PuttyGroup.SelectedItem;
          while ( GroupIterator != null ) {
            Display.AppendFormat($" > {GroupIterator.Header}");
            GroupIterator = GroupIterator.SelectedItem;
          }
          ParentRoot.ContentLocation = Display.ToString();
        }
      }
    }
    private TVMPuttyGroup _SelectedItem;

    public int Count => Items.Count;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttyGroup(string header = "") : base() {
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


    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{Header} - {Count} item(s)");
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public void Add(IHeader item) {
      if ( item == null ) {
        return;
      }
      if ( item is IParent ItemWithParent ) {
        ItemWithParent.Parent = this;
      }
      Items.Add(item);
      NotifyPropertyChanged(nameof(Count));
    }

    public void Add(IEnumerable<IHeader> items) {
      #region === Validate parameters ===
      if ( items == null ) {
        return;
      }
      #endregion === Validate parameters ===
      foreach ( IHeader ItemItem in items ) {
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
    public static TVMPuttyGroup DesignVMPuttyGroupWithNestedGroups {
      get {
        if ( _DesignVMPuttyGroupWithNestedGroups == null ) {
          _DesignVMPuttyGroupWithNestedGroups = new TVMPuttyGroup("Nested groups");
          _DesignVMPuttyGroupWithNestedGroups.Add(TVMPuttySessionsGroupedBy.DesignVMPuttyGroupsL1);
          _DesignVMPuttyGroupWithNestedGroups.Add(TVMPuttySessionsGroupedBy.DesignVMPuttyGroupsL2);
        }
        return _DesignVMPuttyGroupWithNestedGroups;
      }
    }
    protected static TVMPuttyGroup _DesignVMPuttyGroupWithNestedGroups;

    public static TVMPuttyGroup DesignVMPuttyGroup {
      get {
        if ( _DesignVMPuttyGroup == null ) {
          _DesignVMPuttyGroup = new TVMPuttyGroup("Single group");
          _DesignVMPuttyGroup.Add(new TVMPuttyGroup("Inner empty group"));
        }
        return _DesignVMPuttyGroup;
      }
    }
    protected static TVMPuttyGroup _DesignVMPuttyGroup;

    public static IEnumerable<TVMPuttyGroup> DesignVMPuttyGroupsL1 {
      get {
        TVMPuttyGroup L1Group1 = new TVMPuttyGroup("Groupe 1 L1");
        TVMPuttyGroup L2Group1 = new TVMPuttyGroup("Groupe 1 L2");
        L2Group1.Add(TVMPuttySessionsGroupedBy.DesignMultiVMPuttySessionsGroupedBy);
        L1Group1.Add(L2Group1);
        yield return L1Group1;
        TVMPuttyGroup L1Group2 = new TVMPuttyGroup("Groupe 2 L1");
        TVMPuttyGroup L2Group2 = new TVMPuttyGroup("Groupe 2 L2");
        L2Group2.Add(TVMPuttySessionsGroupedBy.DesignSingleVMPuttySessionsGroupedBy);
        L1Group2.Add(L2Group2);
        yield return L1Group2;
      }
    }

    public static IEnumerable<TVMPuttyGroup> DesignVMPuttyGroupsL2 {
      get {
        TVMPuttyGroup L2Group1 = new TVMPuttyGroup("Groupe 1 L2");
        L2Group1.Add(TVMPuttySessionsGroupedBy.DesignSingleVMPuttySessionsGroupedBy);
        yield return L2Group1;
        TVMPuttyGroup L2Group2 = new TVMPuttyGroup("Groupe 2 L2");
        L2Group2.Add(TVMPuttySessionsGroupedBy.DesignMultiVMPuttySessionsGroupedBy);
        yield return L2Group2;
      }
    }
    #endregion --- Statics for design --------------------------------------------

  }
}
