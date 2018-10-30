using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty_std20.Interfaces {
  public interface IName {
    string Name { get; }
    string Description { get; set; }
    string Comment { get; set; }
  }
}
