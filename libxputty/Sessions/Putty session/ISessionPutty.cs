using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using BLTools;
using BLTools.Diagnostic.Logging;
using BLTools.Json;

namespace libxputty {
  public interface ISessionPutty : ISession, ILoggable {

    ESessionPuttyType SessionType { get; set; }
    string GroupLevel1 { get; set; }
    string GroupLevel2 { get; set; }
    string Section { get; set; }
    string RemoteCommand { get; set; }

    string CleanName { get; }
    
    TRunProcess PuttyProcess { get; }

    TPuttyProtocol Protocol { get; set; }
  }
}
