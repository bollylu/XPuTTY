using System;
using System.Collections.Generic;
using System.Linq;

using BLTools;

using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public abstract class TPuttySessionSource : TPuttyBase, IPuttySessionSource, IDisposable {

    public const string ROOT_GROUP_ID = "";

    #region --- Public properties ------------------------------------------------------------------------------

    public ESourceType SourceType { get; protected set; }

    public virtual string DataSourceName => "";
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSource() : base() { }

    protected override void _Initialize() {

    } 
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public static IPuttySessionSource GetPuttySessionSource(string sourceUri) {
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

    public abstract IEnumerable<IPuttySessionGroup> GetGroupsFrom(string groupId, bool recurse = false);
    public abstract IPuttySessionGroup GetGroup(string groupId, bool recurse = true);
    public abstract IEnumerable<(string, TPuttyProtocol)> GetSessionsList(string groupId, bool recurse);
    public abstract IEnumerable<IPuttySession> GetSessions(string groupId, bool recurse = false);
    public abstract void SaveGroup(IPuttySessionGroup group);
    public abstract void UpdateSession(IPuttySession session);
  }
}
