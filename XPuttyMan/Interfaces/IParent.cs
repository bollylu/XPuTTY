using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPuttyMan {
  public interface IParent {
    IParent Parent { get; set; }
    T GetParent<T>();
  }
}
