using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty {
  /// <summary>
  /// What kind of protocol do we use in Putty
  /// </summary>
  public enum EPuttyProtocol {
    Unknown,
    SSH,
    Serial,
    Telnet,
    Raw,
    RLogin
  }
}
