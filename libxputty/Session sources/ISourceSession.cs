using System;
using System.Collections.Generic;
using System.Text;

using BLTools.Diagnostic.Logging;

namespace libxputty {

  /// <summary>
  /// Describe a data source for sessions
  /// </summary>
  public interface ISourceSession : ILoggable {

    /// <summary>
    /// Name of the source
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Type of the source
    /// </summary>
    ESourceType SourceType { get; }

    /// <summary>
    /// Source location
    /// </summary>
    string Location { get; }

    /// <summary>
    /// Complete name for the data source
    /// </summary>
    string DataSourceName { get; }

    /// <summary>
    /// Retrieve sessions from the source
    /// </summary>
    /// <returns></returns>
    IEnumerable<ISession> GetSessions();

    /// <summary>
    /// Retrieve one specific session from the source
    /// </summary>
    /// <param name="name">The name of the source to look for (case insensitive)</param>
    /// <returns>The requested session or null if not existing</returns>
    ISession GetSession(string name);

    /// <summary>
    /// Save a session to the source
    /// </summary>
    /// <param name="session">The session to save</param>
    void SaveSession(ISession session);

    /// <summary>
    /// Save multiple sessions to the source
    /// </summary>
    /// <param name="sessions">The sessions to save</param>
    void SaveSessions(IEnumerable<ISession> sessions);
    

    IEnumerable<(string, TPuttyProtocol)> GetSessionsList();

  }
}
