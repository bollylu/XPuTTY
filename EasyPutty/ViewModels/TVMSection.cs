using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPutty.ViewModels {
  public class TVMSection : TVMEasyPuttyBase {

    public ObservableCollection<TVMPuttySessionsGroupWithView> Items { get; set; } = new ObservableCollection<TVMPuttySessionsGroupWithView>();

    public override string Name {
      get {
        string RetVal = Items.Any() ? Items.First().Section : "";
        if ( RetVal == "" ) {
          return "<noname>";
        }
        return RetVal;
      }
    }

    public int Count => Items.Count;

    public void Add(TVMPuttySessionsGroupWithView sessionsList) {
      Items.Add(sessionsList);
      NotifyPropertyChanged(nameof(Name));
    }

    public void Clear() {
      Items.Clear();
      NotifyPropertyChanged(nameof(Name));
    }

    public override void Dispose() {
      Items.Clear();
    }

    protected override void _Initialize() {

    }

    protected override void _InitializeCommands() {

    }

    public static TVMSection VMSectionDesign {
      get {
        if ( _VMSectionDesign == null ) {
          _VMSectionDesign = new TVMSection();
          _VMSectionDesign.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupWithView);
          _VMSectionDesign.Add(TVMPuttySessionsGroupWithView.DesignVMPuttySessionsGroupWithView);
        }
        return _VMSectionDesign;
      }
    }
    private static TVMSection _VMSectionDesign;
  }
}
