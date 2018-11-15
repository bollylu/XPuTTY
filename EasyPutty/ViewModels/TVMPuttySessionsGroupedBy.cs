using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BLTools;

namespace EasyPutty.ViewModels {
  public class TVMPuttySessionsGroupedBy : TVMPuttyGroup {

    public CollectionView ItemsView { get; protected set; }

    public new TVMPuttySessionsGroupedBy SelectedItem {
      get {
        return _SelectedItem;
      }
      set {
        _SelectedItem = value;
        NotifyPropertyChanged(nameof(SelectedItem));
      }
    }
    private TVMPuttySessionsGroupedBy _SelectedItem;

    #region RelayCommand
    #endregion RelayCommand

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySessionsGroupedBy(string header = "") : base(header) {
    }

    public TVMPuttySessionsGroupedBy(string header, IEnumerable<TVMPuttySession> vmPuttySessions) {
      Add(vmPuttySessions);
    }

    protected override void _InitializeCommands() {
    }

    protected override void _Initialize() {
      ItemsView = (CollectionView)CollectionViewSource.GetDefaultView(Items);
      if ( ItemsView.CanGroup ) {
        PropertyGroupDescription GroupDescription = new PropertyGroupDescription("Section");
        ItemsView.GroupDescriptions.Add(GroupDescription);
      }
    }

    public override void Dispose() {
      Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{Header} - {Count} sessions");
      return RetVal.ToString();
    }

    #region --- Design statics --------------------------------------------
    public static TVMPuttySessionsGroupedBy DesignSingleVMPuttySessionsGroupedBy {
      get {
        if ( _DesignSingleVMPuttySessionsGroupedBy == null ) {
          _DesignSingleVMPuttySessionsGroupedBy = new TVMPuttySessionsGroupedBy("Single");
          _DesignSingleVMPuttySessionsGroupedBy.Add(TVMPuttySession.DesignVMPuttySession);
          _DesignSingleVMPuttySessionsGroupedBy.Add(TVMPuttySession.DesignVMPuttySession2);
        }
        return _DesignSingleVMPuttySessionsGroupedBy;
      }
    }
    private static TVMPuttySessionsGroupedBy _DesignSingleVMPuttySessionsGroupedBy;

    public static IEnumerable<TVMPuttySessionsGroupedBy> DesignMultiVMPuttySessionsGroupedBy {
      get {
        TVMPuttySessionsGroupedBy L3Group1 = new TVMPuttySessionsGroupedBy("Multi 1");
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession2);
        yield return L3Group1;
        TVMPuttySessionsGroupedBy L3Group2 = new TVMPuttySessionsGroupedBy("Multi 2");
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession2);
        yield return L3Group2;
      }
    } 
    #endregion --- Design statics --------------------------------------------
  }
}
