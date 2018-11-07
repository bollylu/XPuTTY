using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public abstract class TPuttySessionSource : IPuttySessionSource {
    public string Name { get; set; }

    public ESourceType SourceType { get; protected set; }

    public string Location { get; set; }

    public virtual string DataSourceName => "";

    public IEnumerable<(string, TPuttyProtocol)> GetSessionsList() {
      #region --- MyRegion --------------------------------------------
      if ( SourceType == ESourceType.Unknown ) {
        Log.Write("Unable to read sessions : Source type is unknown");
        yield break;
      }
      if ( string.IsNullOrWhiteSpace(Location) ) {
        Log.Write("Unable to read sessions : Location is missing or invalid");
        yield break;
      }
      #endregion --- MyRegion --------------------------------------------
      foreach ( (string, TPuttyProtocol) Item in _GetSessionList() ) {
        yield return Item;
      }
    }

    public IPuttySession ReadSession() {
      return ReadSessions().First();
    }

    public IEnumerable<IPuttySession> ReadSessions() {
      #region --- MyRegion --------------------------------------------
      if ( SourceType == ESourceType.Unknown ) {
        Log.Write("Unable to read sessions : Source type is unknown");
        yield break;
      }
      if ( string.IsNullOrWhiteSpace(Location) ) {
        Log.Write("Unable to read sessions : Location is missing or invalid");
        yield break;
      }
      #endregion --- MyRegion --------------------------------------------
      foreach ( IPuttySession PuttySessionItem in _ReadSessions() ) {
        yield return PuttySessionItem;
      }
    }

    public void SaveSession(IPuttySession session) {
      #region === Validate parameters ===
      if ( SourceType == ESourceType.Unknown ) {
        Log.Write("Unable to save sessions : Source type is unknown");
        return;
      }
      if ( string.IsNullOrWhiteSpace(Location) ) {
        Log.Write("Unable to read sessions : Location is missing or invalid");
        return;
      }
      if ( session == null ) {
        Log.Write("Unable to save session : session is missing");
        return;
      }
      #endregion === Validate parameters ===
      _SaveSession(session);
    }

    public void SaveSessions(IEnumerable<IPuttySession> sessions) {
      #region === Validate parameters ===
      if ( SourceType == ESourceType.Unknown ) {
        Log.Write("Unable to save sessions : Source type is unknown");
        return;
      }
      if ( string.IsNullOrWhiteSpace(Location) ) {
        Log.Write("Unable to read sessions : Location is missing or invalid");
        return;
      }
      if ( sessions == null || !sessions.Any() ) {
        Log.Write("Unable to save session : session list is missing or empty");
        return;
      }
      #endregion === Validate parameters ===
      _SaveSessions(sessions);
    }

    protected abstract IEnumerable<(string, TPuttyProtocol)> _GetSessionList();

    protected abstract IEnumerable<IPuttySession> _ReadSessions();
    protected abstract IPuttySession _ReadSession(string name, TPuttyProtocol protocol);

    protected abstract void _SaveSession(IPuttySession session);
    protected abstract void _SaveSessions(IEnumerable<IPuttySession> sessions);
  }
}
