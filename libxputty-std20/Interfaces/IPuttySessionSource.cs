using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20.Interfaces {
  public interface IPuttySessionSource {

    string Name { get; set; }
    ESourceType SourceType { get; }

    string DataSourceName { get; }

    IEnumerable<IPuttySessionGroup> GetGroupsFrom(string groupId = "", bool recurse = false);

    IPuttySessionGroup GetGroup(string groupId = "", bool recurse = true);

    IEnumerable<(string, TPuttyProtocol)> GetSessionsList(string groupId, bool recurse);

    IEnumerable<IPuttySession> GetSessions(string groupId = "", bool recurse = false);

    void SaveGroup(IPuttySessionGroup group);

    void UpdateSession(IPuttySession session);

  }
}
