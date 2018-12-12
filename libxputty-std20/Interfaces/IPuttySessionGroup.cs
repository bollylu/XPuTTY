using System;
using System.Collections.Generic;
using System.Text;
using BLTools;

namespace libxputty_std20.Interfaces {
  public interface IPuttySessionGroup : IName, IParent, ICredentialContainer, IDisposable {

    string ID { get; set; }
    IList<IPuttySessionGroup> Groups { get; }
    IList<IPuttySession> Sessions { get; }

    void AddOrUpdateGroup(IPuttySessionGroup group);
    void AddOrUpdateSession(IPuttySession session);

    void AddGroups(IEnumerable<IPuttySessionGroup> groups);
    void AddSessions(IEnumerable<IPuttySession> sessions);

    void RemoveGroup(IPuttySessionGroup group);
    void RemoveGroup(string groupName);
    void RemoveSession(IPuttySession session);
    void RemoveSession(string sessionName);

    IPuttySessionGroup GetGroup(string groupId, bool recurse);

    void ClearGroups();
    void ClearSessions();
    void Clear();

    IEnumerable<IPuttySessionGroup> GetAllGroups(bool recurse = true);
    IEnumerable<IPuttySession> GetAllSessions(bool recurse = true);

  }
}
