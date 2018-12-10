using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20.Interfaces {
  public interface IPuttySessionSource {

    string Name { get; set; }
    ESourceType SourceType { get; }

    string DataSourceName { get; }

    IEnumerable<IPuttySessionsGroup> GetGroupsFrom(string groupId = "", bool recurse = false);

    IPuttySessionsGroup GetGroup(string groupId = "", bool recurse = true);

    IEnumerable<(string, TPuttyProtocol)> GetSessionsList(string groupId, bool recurse);
    IEnumerable<(string, TPuttyProtocol)> GetSessionsList(IPuttySessionsGroup group, bool recurse);

    IEnumerable<IPuttySession> GetSessions(string groupId = "", bool recurse = false);
    IEnumerable<IPuttySession> GetSessions(IPuttySessionsGroup group, bool recurse = false);

    IPuttySession GetSession(IPuttySessionsGroup group, string sessionId, bool recurse = true);

    void SaveGroup(IPuttySessionsGroup group);

    void UpdateSession(IPuttySession session);



  }
}
