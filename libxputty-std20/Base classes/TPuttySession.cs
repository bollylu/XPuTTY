using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySession : IPuttySession, IDisposable {

    #region --- Constants --------------------------------------------
    internal const string REG_BASE = @"Software\SimonTatham\PuTTY\Sessions";
    internal const string EXECUTABLE_PUTTY = "putty.exe";

    internal const string VAL_PROTOCOL_TYPE = "Protocol";

    public const string VAL_PROTOCOL_SERIAL_LINE = "SerialLine";
    public const string VAL_PROTOCOL_SERIAL_SPEED = "SerialSpeed";
    public const string VAL_PROTOCOL_SERIAL_DATA_BITS = "SerialDataBits";
    public const string VAL_PROTOCOL_SERIAL_PARITY = "SerialParity";
    public const string VAL_PROTOCOL_SERIAL_STOP_BITS = "SerialStopHalfBits";
    public const string VAL_PROTOCOL_SERIAL_FLOW_CONTROL = "SerialFlowControl";

    public const string VAL_HOSTNAME = "HostName";
    public const string VAL_PORT = "PortNumber";
    public const string VAL_SSH_REMOTE_COMMAND = "RemoteCommand";
    #endregion --- Constants --------------------------------------------

    #region --- Public properties ------------------------------------------------------------------------------
    public string Name { get; set; }
    public string CleanName => Name.Replace("%20", " ");
    public int PID { get; set; }

    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Event handlers --------------------------------------------
    public event EventHandler OnStart;
    public event EventHandler OnExit;
    #endregion --- Event handlers --------------------------------------------

    private Task WatchTask;
    private CancellationTokenSource WatchTaskCancellation = new CancellationTokenSource();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySession() {
      Name = "<no name>";
    }
    public TPuttySession(string name) {
      Name = name;
    }

    public virtual void Dispose() {
      if ( WatchTask != null ) {
        WatchTaskCancellation.Cancel();
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public static TPuttyProtocol GetSessionProtocolFromRegistry(string sessionKeyName = "") {

      if ( string.IsNullOrWhiteSpace(sessionKeyName) ) {
        return TPuttyProtocol.Unknown;
      }

      string PuttySessionKeyName = $@"{REG_BASE}\{sessionKeyName}";
      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {
        return PuttySessionKey.GetValue(VAL_PROTOCOL_TYPE, "") as string;
      }
    }

    public static IPuttySession GetSessionFromRegistry(string sessionKeyName = "") {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(sessionKeyName) ) {
        return null;
      }
      #endregion === Validate parameters ===

      string PuttySessionKeyName = $@"{REG_BASE}\{sessionKeyName}";

      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {

        if ( GetSessionProtocolFromRegistry(sessionKeyName).IsSSH ) {
          TPuttySessionSSH NewSessionSSH = new TPuttySessionSSH(sessionKeyName) {
            HostName = PuttySessionKey.GetValue(VAL_HOSTNAME, "") as string,
            Port = (int)PuttySessionKey.GetValue(VAL_PORT, 0),
            SSH_RemoteCommand = PuttySessionKey.GetValue(VAL_SSH_REMOTE_COMMAND, "") as string
          };
          return NewSessionSSH;
        }

        if ( GetSessionProtocolFromRegistry(sessionKeyName).IsSerial ) {
          TPuttySessionSerial NewSessionSerial = new TPuttySessionSerial(sessionKeyName) {
            SerialLine = PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_LINE, "") as string,
            SerialSpeed = (int)PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_SPEED, 0),
            SerialParity = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_PARITY, 0)),
            SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_STOP_BITS, 0)),
            SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_DATA_BITS, 0))
          };
          return NewSessionSerial;
        }

        if ( GetSessionProtocolFromRegistry(sessionKeyName).IsRLogin ) {
          return TPuttySession.Empty;
        }

        if ( GetSessionProtocolFromRegistry(sessionKeyName).IsTelnet ) {
          return TPuttySession.Empty;
        }
        if ( GetSessionProtocolFromRegistry(sessionKeyName).IsRaw ) {
          return TPuttySession.Empty;
        }

        return TPuttySession.Empty;
      }
    }

    public static IPuttySession Empty {
      get {
        if ( _Empty == null ) {
          _Empty = new TPuttySession("<empty>");
        }
        return _Empty;
      }
    }
    private static IPuttySession _Empty;

    public virtual void Start() {
      Process PuttyProcess = new Process();
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = $"-load {"\"" + CleanName + "\""}"
      };
      Console.WriteLine($"Loading {StartInfo.Arguments} ...");
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
      PID = PuttyProcess.Id;

      if ( OnStart != null ) {
        OnStart(this, EventArgs.Empty);
      }

      WatchTask = Task.Run(async () => {
        while ( PID != 0 || WatchTaskCancellation.IsCancellationRequested ) {
          try {
            Process WatchedProcess = Process.GetProcessById(PID);
            await Task.Delay(200);
          } catch {
            PID = 0;
          }
        }
        if ( OnExit != null ) {
          OnExit(this, EventArgs.Empty);
        }
      }, WatchTaskCancellation.Token);

    }

    public virtual void Stop() { }

    public bool CheckIsRunning() {
      return Process.GetProcesses().Any(x => x.Id == PID);
    }

    public static IEnumerable<IPuttySession> GetAllRunningSessions() {
      using ( ManagementClass WMI = new ManagementClass("Win32_Process") ) {
        foreach ( ManagementObject ProcessItem in WMI.GetInstances() ) {
          if ( ProcessItem["name"].ToString().ToLowerInvariant() == EXECUTABLE_PUTTY ) {
            yield return new TPuttySession(ProcessItem["CommandLine"].ToString()) { PID = Convert.ToInt32(ProcessItem["ProcessId"]) };
          }
        }
      }
    }
  }
}
