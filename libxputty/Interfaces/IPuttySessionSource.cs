using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty.Interfaces {
  public interface IPuttySessionSource {

    string Name { get; set; }
    ESourceType SourceType { get; }

    string Location { get; }
    string DataSourceName { get; }


    IEnumerable<IPuttySession> GetSessions();
    IPuttySession GetSession(string name);

    void SaveSessions(IEnumerable<IPuttySession> sessions);
    void SaveSession(IPuttySession session);

    IEnumerable<(string, TPuttyProtocol)> GetSessionsList();

  }
}
