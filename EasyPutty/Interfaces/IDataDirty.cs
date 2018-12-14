using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPutty.Interfaces {
  public interface IDataDirty {
    bool IsDataDirty { get; }
    void ResetDataDirty(bool recurse = false);
    void SetDataDirty();
  }
}
