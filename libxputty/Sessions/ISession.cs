using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLTools;

namespace libxputty {
  public interface ISession : IParent, IDisposable, IName, ICredentialContainer {

    /// <summary>
    /// The PID of the executed process for the session
    /// </summary>
    int PID { get; }

    /// <summary>
    /// The command line to start the process of the session
    /// </summary>
    string CommandLine { get; }

    /// <summary>
    /// Is the session process running ?
    /// </summary>
    bool IsRunning { get; }

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

    /// <summary>
    /// Duplicate the session in memory
    /// </summary>
    /// <returns></returns>
    ISession Duplicate();

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
    
  }
}
