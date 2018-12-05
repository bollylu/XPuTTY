using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using BLTools;

using EasyPutty.Interfaces;
using libxputty_std20;

namespace EasyPutty.ViewModels {
  public class TVMPuttyGroup : TVMEasyPuttyBase, IHeaderedAndSelectable {

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
        if ( !ParentGroup.Items.Any() ) {
          return false;
        }

        if ( ParentGroup.Items.Count() == 1 ) {
          ParentGroup.SelectedItem = this;
          return false;
        }

        return true;
      }
    }
    public ICommand CommandSelectItem {
      get; protected set;
    }
    #endregion --- IHeaderedAndSelectable -----------------------------------------

    public ObservableCollection<IHeaderedAndSelectable> Items { get; private set; } = new ObservableCollection<IHeaderedAndSelectable>();

    public IEnumerable<TVMPuttyGroup> Groups => Items.OfType<TVMPuttyGroup>();
    public IEnumerable<TVMPuttySession> Sessions => Items.OfType<TVMPuttySession>();

    public IHeaderedAndSelectable SelectedItem {
      get {
        if ( _SelectedItem == null && Items.Any()) {
          _SelectedItem = Items.First() as IHeaderedAndSelectable;
        }
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        if ( ParentRoot != null ) {
          StringBuilder Display = new StringBuilder();
          if ( ParentRoot.PuttyGroup.SelectedItem is TVMPuttyGroup GroupIterator ) {
            while ( GroupIterator != null ) {
              if ( !GroupIterator.Header.IsEmpty() ) {
                Display.AppendFormat($" > {GroupIterator.Header}");
              }
              GroupIterator = GroupIterator.SelectedItem as TVMPuttyGroup;
            }
          }
          ParentRoot.ContentLocation = Display.ToString();
        }
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private IHeaderedAndSelectable _SelectedItem;

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
      lock ( _Lock ) {
        foreach ( TVMPuttyGroup PuttyGroupItem in Items.OfType<TVMPuttyGroup>() ) {
          PuttyGroupItem.Dispose(true);
        }
        foreach ( TVMPuttySession PuttySessionItem in Items.OfType<TVMPuttySession>() ) {
          PuttySessionItem.Dispose();
        }
        Clear();
      }
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

    public void Add(TVMPuttyGroup item) {
      if ( item == null ) {
        return;
      }
      if ( item is IParent ItemWithParent ) {
        ItemWithParent.Parent = this;
      }
      Items.Add(item);
      NotifyPropertyChanged(nameof(Count));
    }

    public void Add(IEnumerable<TVMPuttyGroup> items) {
      #region === Validate parameters ===
      if ( items == null ) {
        return;
      }
      #endregion === Validate parameters ===
      foreach ( TVMPuttyGroup ItemItem in items ) {
        Add(ItemItem);
      }
      NotifyPropertyChanged(nameof(Count));
    }

    public void Clear() {
      foreach(IHeaderedAndSelectable PuttyGroupItem in Items) {
        PuttyGroupItem.Clear();
      }
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
