using System;
using System.Collections.Generic;
using System.Text;
using BLTools;

namespace libxputty_std20.Interfaces {
  public interface IGroupItem : IName, IParent, ICredentialContainer, IDisposable {

    IList<IGroupItem> Groups { get; }
    IList<IPuttySession> Sessions { get; }

    void AddOrUpdateGroup(IGroupItem group);
    void AddOrUpdateSession(IPuttySession session);

    void AddGroups(IEnumerable<IGroupItem> groups);
    void AddSessions(IEnumerable<IPuttySession> sessions);

    void RemoveGroup(IGroupItem group);
    void RemoveGroup(string groupName);
    void RemoveSession(IPuttySession session);
    void RemoveSession(string sessionName);

    void ClearGroups();
    void ClearSessions();
    void Clear();

    IEnumerable<IGroupItem> GetAllGroups(bool recurse = true);
    IEnumerable<IPuttySession> GetAllSessions(bool recurse = true);

  }
}
