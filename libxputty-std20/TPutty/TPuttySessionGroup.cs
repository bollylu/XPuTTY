﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionGroup : TPuttyBase, IPuttySessionsGroup {

    public IList<IPuttySessionsGroup> Groups {
      get {
        lock ( _LockGroups ) {
          return _Groups;
        }
      }
    }
    private readonly IList<IPuttySessionsGroup> _Groups = new List<IPuttySessionsGroup>();

    public IList<IPuttySession> Sessions {
      get {
        lock ( _LockSessions ) {
          return _Sessions;
        }
      }
    }

    public string ID { get; set; }

    private readonly IList<IPuttySession> _Sessions = new List<IPuttySession>();

    private readonly object _LockGroups = new object();
    private readonly object _LockSessions = new object();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionGroup() : base() { }
    public TPuttySessionGroup(string name) : base(name) {

    }
    protected override void _Initialize() {

    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Group {Name} : ");
      RetVal.Append($" {Groups.Count} groups");
      RetVal.Append($", {Sessions.Count} sessions");
      return RetVal.ToString();
    }
    #endregion --- Converters ----------------------------------------------------------------------------------

    #region --- Groups management --------------------------------------------
    public void AddOrUpdateGroup(IPuttySessionsGroup group) {
      if ( group == null ) {
        return;
      }
      lock ( _LockGroups ) {
        _RemoveGroup(group.Name);
        Groups.Add(group);
      }
    }

    public void AddGroups(IEnumerable<IPuttySessionsGroup> groups) {
      if ( groups == null ) {
        return;
      }
      lock ( _LockGroups ) {
        foreach ( IPuttySessionsGroup GroupItem in groups ) {
          Groups.Add(GroupItem);
        }
      }
    }

    public void RemoveGroup(IPuttySessionsGroup group) {
      if ( group == null ) {
        return;
      }
      RemoveGroup(group.Name);
    }

    public void RemoveGroup(string groupName) {
      lock ( _LockGroups ) {
        _RemoveGroup(groupName);
      }
    }

    public IEnumerable<IPuttySessionsGroup> GetAllGroups(bool recurse = true) {
      if ( !Groups.Any() ) {
        yield break;
      }
      foreach ( IPuttySessionsGroup GroupItem in Groups ) {
        yield return GroupItem;
        if ( recurse ) {
          foreach ( IPuttySessionsGroup RecurseGroupItem in GroupItem.GetAllGroups(recurse) ) {
            yield return RecurseGroupItem;
          }
        }
      }
    }

    public IPuttySessionsGroup GetGroup(string groupId, bool recurse = true) {
      #region === Validate parameters ===
      if ( groupId == null ) {
        return null;
      }

      if ( groupId == "" ) {
        return this;
      }
      #endregion === Validate parameters ===

      if ( ID == groupId ) {
        return this;
      }

      foreach ( IPuttySessionsGroup GroupItem in Groups ) {
        if ( ID == groupId ) {
          return GroupItem;
        }
      }

      if ( recurse ) {
        foreach ( IPuttySessionsGroup GroupItem in Groups ) {
          IPuttySessionsGroup SubGroup = GroupItem.GetGroup(groupId, recurse);
          if ( SubGroup != null ) {
            return SubGroup;
          }
        }
      }

      return null;
    }

    public void ClearGroups() {
      lock ( _LockGroups ) {
        _ClearGroups();
      }
    }
    #endregion --- Groups management -----------------------------------------

    #region --- Sessions management --------------------------------------------
    public void AddOrUpdateSession(IPuttySession session) {
      if ( session == null ) {
        return;
      }
      lock ( _LockSessions ) {
        _RemoveSession(session.Name);
        Sessions.Add(session);
      }
    }

    public void AddSessions(IEnumerable<IPuttySession> sessions) {
      if ( sessions == null ) {
        return;
      }
      lock ( _LockSessions ) {
        foreach ( IPuttySession SessionItem in sessions ) {
          Sessions.Add(SessionItem);
        }
      }
    }

    public void RemoveSession(IPuttySession session) {
      if ( session == null ) {
        return;
      }
      RemoveSession(session.Name);
    }

    public void RemoveSession(string sessionName) {
      lock ( _LockSessions ) {
        _RemoveSession(sessionName);
      }
    }

    public IEnumerable<IPuttySession> GetAllSessions(bool recurse = true) {
      foreach ( IPuttySession SessionItem in Sessions ) {
        yield return SessionItem;
      }
      if ( recurse ) {
        if ( !Groups.Any() ) {
          yield break;
        }
        foreach ( IPuttySessionsGroup GroupItem in Groups ) {
          foreach ( IPuttySession SessionItem in GroupItem.GetAllSessions(recurse) ) {
            yield return SessionItem;
          }
        }
      }
    }

    public void ClearSessions() {
      lock ( _LockSessions ) {
        _ClearSessions();
      }
    }
    #endregion --- Sessions management -----------------------------------------

    public void Clear() {
      ClearGroups();
      ClearSessions();
    }

    #region --- Clear without lock --------------------------------------------
    protected void _ClearSessions() {
      foreach ( IPuttySession SessionItem in Sessions ) {
        SessionItem.Dispose();
      }
      Sessions.Clear();
    }

    protected void _ClearGroups() {
      foreach ( IPuttySessionsGroup GroupItem in Groups ) {
        GroupItem.Clear();
        GroupItem.Dispose();
      }
      Groups.Clear();
    }

    protected void _RemoveSession(string sessionName) {
      if ( string.IsNullOrEmpty(sessionName) ) {
        return;
      }
      if ( !Sessions.Any() ) {
        return;
      }
      for ( int i = 0; i < Sessions.Count; i++ ) {
        if ( Sessions[i].Name == sessionName ) {
          Sessions[i].Dispose();
          Sessions.RemoveAt(i);
          return;
        }
      }
    }

    protected void _RemoveGroup(string groupName) {
      if ( string.IsNullOrEmpty(groupName) ) {
        return;
      }
      if ( !Groups.Any() ) {
        return;
      }
      for ( int i = 0; i < Groups.Count; i++ ) {
        if ( Groups[i].Name == groupName ) {
          Groups[i].Clear();
          Groups.RemoveAt(i);
          return;
        }
      }
    }
    #endregion --- Clear without lock -----------------------------------------

  }
}
