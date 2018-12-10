using System;
using System.Collections.Generic;
using System.Text;
using BLTools;

namespace libxputty_std20.Interfaces {
  public interface IPuttySessionsGroup : IName, IParent, ICredentialContainer, IDisposable {

    string ID { get; set; }
    IList<IPuttySessionsGroup> Groups { get; }
    IList<IPuttySession> Sessions { get; }

    void AddOrUpdateGroup(IPuttySessionsGroup group);
    void AddOrUpdateSession(IPuttySession session);

    void AddGroups(IEnumerable<IPuttySessionsGroup> groups);
    void AddSessions(IEnumerable<IPuttySession> sessions);

    void RemoveGroup(IPuttySessionsGroup group);
    void RemoveGroup(string groupName);
    void RemoveSession(IPuttySession session);
    void RemoveSession(string sessionName);

    IPuttySessionsGroup GetGroup(string groupId, bool recurse);

    void ClearGroups();
    void ClearSessions();
    void Clear();

    IEnumerable<IPuttySessionsGroup> GetAllGroups(bool recurse = true);
    IEnumerable<IPuttySession> GetAllSessions(bool recurse = true);

  }
}
