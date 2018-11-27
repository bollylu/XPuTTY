using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPutty.Interfaces {
  public interface IHeaderedAndSelectable {
    string Header {
      get;
    }
    TRelayCommand CommandSelectItem {
      get;
    }
    bool DisplaySelectionButton {
      get;
    }

    void Clear();

    ObservableCollection<IHeaderedAndSelectable> Items { get; }
  }
}
