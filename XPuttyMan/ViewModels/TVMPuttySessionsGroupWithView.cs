using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using BLTools;

namespace EasyPutty.ViewModels {
  public class TVMPuttySessionsGroupWithView : TVMPuttySessionsGroup {

    public string Section => PuttySessions.Any() ? PuttySessions.First().Section : "";

    public CollectionView PuttySessionsView { get; private set; }

    #region RelayCommand
    #endregion RelayCommand

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TVMPuttySessionsGroupWithView(string header = "") : base(header) {
    }

    public TVMPuttySessionsGroupWithView(string header, IEnumerable<TVMPuttySession> vmPuttySessions) : base(header, vmPuttySessions) {
    }

    protected override void _InitializeCommands() {
    }

    protected override void _Initialize() {
      PuttySessionsView = (CollectionView)CollectionViewSource.GetDefaultView(PuttySessions);
      if ( PuttySessionsView.CanGroup ) {
        PropertyGroupDescription GroupDescription = new PropertyGroupDescription("Section");
        PuttySessionsView.GroupDescriptions.Add(GroupDescription);
      }
    }

    public override void Dispose() {
      Clear();
      Dispose(true);
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public static TVMPuttySessionsGroupWithView DesignVMPuttySessionsGroupWithView {
      get {
        if ( _DesignVMPuttySessionsGroupWithView == null ) {
          _DesignVMPuttySessionsGroupWithView = new TVMPuttySessionsGroupWithView();
          _DesignVMPuttySessionsGroupWithView.Add(TVMPuttySession.DesignVMPuttySession);
          _DesignVMPuttySessionsGroupWithView.Add(TVMPuttySession.DesignVMPuttySession2);
        }
        return _DesignVMPuttySessionsGroupWithView;
      }
    }
    private static TVMPuttySessionsGroupWithView _DesignVMPuttySessionsGroupWithView;

    public static IEnumerable<TVMPuttySessionsGroupWithView> DesignVMPuttySessionsGroupsWithView {
      get {
        TVMPuttySessionsGroupWithView L3Group1 = new TVMPuttySessionsGroupWithView("Groupe 1 Sections");
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group1.Add(TVMPuttySession.DesignVMPuttySession2);
        yield return L3Group1;
        TVMPuttySessionsGroupWithView L3Group2 = new TVMPuttySessionsGroupWithView("Groupe 2 Sections");
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession);
        L3Group2.Add(TVMPuttySession.DesignVMPuttySession2);
        yield return L3Group2;
      }
    }
  }
}
