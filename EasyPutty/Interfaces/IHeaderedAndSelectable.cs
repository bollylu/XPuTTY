using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyPutty.Interfaces {
  public interface IHeaderedAndSelectable {
    string Header {
      get;
    }
    ICommand CommandSelectItem {
      get;
    }
    bool DisplaySelectionButton {
      get;
    }

    void Clear();

    ObservableCollection<IHeaderedAndSelectable> Items { get; }
  }
}
