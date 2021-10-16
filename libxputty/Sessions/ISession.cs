using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLTools;
using BLTools.Diagnostic.Logging;

namespace libxputty {
  public interface ISession : IParent, IDisposable, IName, ICredentialContainer, ILoggable {

    /// <summary>
    /// The type of the session
    /// </summary>
    ESessionType SessionType { get; }

    /// <summary>
    /// The location where to save or load session information
    /// </summary>
    string StorageLocation { get; set; }

    /// <summary>
    /// Start the session process, passing an array of arguments
    /// </summary>
    /// <param name="arguments">The list of process arguments</param>
    void Start(IEnumerable<string> arguments);

    /// <summary>
    /// Start the session process, with arguments as a string (like command line)
    /// </summary>
    /// <param name="arguments"></param>
    void Start(string arguments = "");

    /// <summary>
    /// Stop the session process
    /// </summary>
    void Stop();

    #region --- Process --------------------------------------------
    /// <summary>
    /// The process to be executed
    /// </summary>
    TRunProcess RunProcess { get; }

    /// <summary>
    /// Is the underlying process running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// The PID of the underlying process
    /// </summary>
    int PID { get; }

    /// <summary>
    /// The command line info used to start the underlying process
    /// </summary>
    string CommandLine { get; } 
    #endregion --- Process --------------------------------------------

    /// <summary>
    /// Duplicate the session in memory
    /// </summary>
    /// <returns></returns>
    ISession Duplicate();

    #region --- Event handlers --------------------------------------------
    /// <summary>
    /// Event risen before starting the session
    /// </summary>
    event EventHandler OnStart;
    /// <summary>
    /// Event risen after the session is started (successfully or not)
    /// </summary>
    event EventHandler OnStarted;
    /// <summary>
    /// Event risen before stopping the session
    /// </summary>
    event EventHandler OnExit;
    /// <summary>
    /// Event risen after the session is stopped (successfully or not)
    /// </summary>
    event EventHandler OnExited; 
    #endregion --- Event handlers --------------------------------------------

  }
}
