using System;
using System.Collections.Generic;
using System.Linq;

using BLTools;
using BLTools.Diagnostic.Logging;

namespace libxputty {
  public abstract class ASourceSession : ASessionBase, ISourceSession, IDisposable {
    #region --- Public properties ------------------------------------------------------------------------------

    public ESourceType SourceType { get; protected set; }

    public string Location { get; set; }

    public virtual string DataSourceName => "";
    #endregion --- Public properties ---------------------------------------------------------------------------

    public ASourceSession() : base() { }

    #region --- Read data --------------------------------------------
    public IEnumerable<(string, TPuttyProtocol)> GetSessionsList() {
      
      if (SourceType == ESourceType.Unknown) {
        LogError("Unable to read sessions : Source type is unknown");
        yield break;
      }

      if (string.IsNullOrWhiteSpace(Location)) {
        LogError("Unable to read sessions : Location is missing or invalid");
        yield break;
      }

      foreach ((string, TPuttyProtocol) Item in _GetSessionList()) {
        yield return Item;
      }
    }

    public ISession GetSession(string name) {
      return GetSessions().FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public IEnumerable<ISession> GetSessions() {
      #region --- MyRegion --------------------------------------------
      if (SourceType == ESourceType.Unknown) {
        LogError("Unable to read sessions : Source type is unknown");
        yield break;
      }
      if (string.IsNullOrWhiteSpace(Location)) {
        LogError("Unable to read sessions : Location is missing or invalid");
        yield break;
      }
      #endregion --- MyRegion --------------------------------------------

      foreach (ISessionPutty PuttySessionItem in _ReadSessions()) {
        yield return PuttySessionItem;
      }
    }

    public TPuttySessionGroup GetGroup(string name) {
      return GetGroups().FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }

    public IEnumerable<TPuttySessionGroup> GetGroups() {
      #region --- MyRegion --------------------------------------------
      if (SourceType == ESourceType.Unknown) {
        LogError("Unable to read sessions : Source type is unknown");
        yield break;
      }
      if (string.IsNullOrWhiteSpace(Location)) {
        LogError("Unable to read sessions : Location is missing or invalid");
        yield break;
      }
      #endregion --- MyRegion --------------------------------------------

      foreach (TPuttySessionGroup PuttySessionGroupItem in _ReadGroups()) {
        yield return PuttySessionGroupItem;
      }
    }
    #endregion --- Read data --------------------------------------------

    #region --- Save data --------------------------------------------
    public void SaveSession(ISession session) {
      #region === Validate parameters ===
      if (SourceType == ESourceType.Unknown) {
        LogError("Unable to save sessions : Source type is unknown");
        return;
      }
      if (string.IsNullOrWhiteSpace(Location)) {
        LogError("Unable to read sessions : Location is missing or invalid");
        return;
      }
      if (session == null) {
        LogError("Unable to save session : session is missing");
        return;
      }
      #endregion === Validate parameters ===
      _SaveSession(session);
    }

    public void SaveSessions(IEnumerable<ISession> sessions) {
      #region === Validate parameters ===
      if (SourceType == ESourceType.Unknown) {
        LogError("Unable to save sessions : Source type is unknown");
        return;
      }
      if (string.IsNullOrWhiteSpace(Location)) {
        LogError("Unable to read sessions : Location is missing or invalid");
        return;
      }
      if (sessions == null || !sessions.Any()) {
        LogError("Unable to save session : session list is missing or empty");
        return;
      }
      #endregion === Validate parameters ===
      _SaveSessions(sessions);
    }
    #endregion --- Save data --------------------------------------------

    #region --- Abstract read data --------------------------------------------
    protected abstract IEnumerable<(string, TPuttyProtocol)> _GetSessionList();

    protected abstract IEnumerable<ISessionPutty> _ReadSessions();
    protected abstract ISessionPutty _ReadSession(string name, TPuttyProtocol protocol);

    protected abstract IEnumerable<TPuttySessionGroup> _ReadGroups();
    #endregion --- Abstract read data --------------------------------------------

    #region --- Abstract save data --------------------------------------------
    protected abstract void _SaveSession(ISession session);
    protected abstract void _SaveSessions(IEnumerable<ISession> sessions);
    #endregion --- Abstract save data --------------------------------------------


    /// <summary>
    /// Build a source of session from a prefixed string (url alike)
    /// </summary>
    /// <param name="sourceUri">Url to the source (registry://..., xml://..., json://...)</param>
    /// <param name="logger">The logger for the operations</param>
    /// <returns>One ISourceSession or null if SourceUri is invalid</returns>
    public static ISourceSession BuildSourceSession(string sourceUri, ILogger logger) {

      if (logger is null) {
        logger = ALogger.DEFAULT_LOGGER;
      }

      if (string.IsNullOrWhiteSpace(sourceUri)) {
        return null;
      }

      try {
        switch (sourceUri.Before("://").ToLower()) {

          case TSourceSessionPuttyRegistry.DATASOURCE_PREFIX: {
              if (OperatingSystem.IsWindows()) {
                ISourceSession RetVal = new TSourceSessionPuttyRegistry();
                RetVal.SetLogger(logger);
                return RetVal;
              } else {
                logger.LogError("Unable to build a source for sessions : registry is only available on Windows");
                return null;
              }
            }

          case TSourceSessionPuttyXml.DATASOURCE_PREFIX: {
              ISourceSession RetVal = new TSourceSessionPuttyXml(sourceUri.After("://"));
              RetVal.SetLogger(logger);
              return RetVal;
            }

          case TSourceSessionPuttyJson.DATASOURCE_PREFIX: {
              ISourceSession RetVal = new TSourceSessionPuttyJson(sourceUri.After("://"));
              RetVal.SetLogger(logger);
              return RetVal;
            }

          default:
            logger.LogError($"Unable to build a source for sessions : sourceUri is invalid : {sourceUri}");
            return null;

        }
      } catch (Exception ex) {
        logger.LogError($"Unable to build a source for sessions : {ex.Message}");
        return null;
      }
    }

  }
}
