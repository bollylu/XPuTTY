using System;
using System.Collections.Generic;
using System.Linq;

using BLTools;

using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public abstract class TPuttySessionSource : TPuttyBase, IPuttySessionSource, IDisposable {
    #region --- Public properties ------------------------------------------------------------------------------

    public ESourceType SourceType { get; protected set; }

    public string Location { get; set; }

    public virtual string DataSourceName => "";
    #endregion --- Public properties ---------------------------------------------------------------------------

    public TPuttySessionSource() : base() { }

    protected override void _Initialize() {
      
    }

    #region --- Read data --------------------------------------------
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

    public IPuttySession GetSession(string name) {
      return GetSessions().FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }

    public IEnumerable<IPuttySession> GetSessions() {
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

    public TPuttySessionGroup GetGroup(string name) {
      return GetGroups().FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }

    public IEnumerable<TPuttySessionGroup> GetGroups() {
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

      foreach ( TPuttySessionGroup PuttySessionGroupItem in _ReadGroups() ) {
        yield return PuttySessionGroupItem;
      }
    }
    #endregion --- Read data --------------------------------------------

    #region --- Save data --------------------------------------------
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
    #endregion --- Save data --------------------------------------------

    #region --- Abstract read data --------------------------------------------
    protected abstract IEnumerable<(string, TPuttyProtocol)> _GetSessionList();

    protected abstract IEnumerable<IPuttySession> _ReadSessions();
    protected abstract IPuttySession _ReadSession(string name, TPuttyProtocol protocol);

    protected abstract IEnumerable<TPuttySessionGroup> _ReadGroups();
    #endregion --- Abstract read data --------------------------------------------

    #region --- Abstract save data --------------------------------------------
    protected abstract void _SaveSession(IPuttySession session);
    protected abstract void _SaveSessions(IEnumerable<IPuttySession> sessions); 
    #endregion --- Abstract save data --------------------------------------------

    public static TPuttySessionSource GetPuttySessionSource(string sourceUri) {
      if ( string.IsNullOrWhiteSpace(sourceUri) ) {
        return null;
      }
      switch ( sourceUri.Before("://").ToLower() ) {
        case TPuttySessionSourceRegistry.DATASOURCE_PREFIX:
          return new TPuttySessionSourceRegistry();
        case TPuttySessionSourceXml.DATASOURCE_PREFIX:
          return new TPuttySessionSourceXml(sourceUri.After("://"));
        case TPuttySessionSourceJson.DATASOURCE_PREFIX:
          return new TPuttySessionSourceJson(sourceUri.After("://"));
        default:
          Log.Write($"Unable to get PuttySessionSource : sourceUri is invalid : {sourceUri}");
          return null;
      }
    }

  }
}
