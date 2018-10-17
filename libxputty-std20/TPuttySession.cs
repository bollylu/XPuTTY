using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySession : IDisposable {

    #region --- Constants --------------------------------------------
    public const string REG_BASE = @"Software\SimonTatham\PuTTY\Sessions";

    public const string VAL_HOSTNAME = "HostName";
    public const string VAL_PORT = "PortNumber";
    public const string VAL_PROTOCOL_TYPE = "Protocol";

    public const string VAL_PROTOCOL_SERIAL_LINE = "SerialLine";
    public const string VAL_PROTOCOL_SERIAL_SPEED = "SerialSpeed";
    public const string VAL_PROTOCOL_SERIAL_DATA_BITS = "SerialDataBits";
    public const string VAL_PROTOCOL_SERIAL_PARITY = "SerialParity";
    public const string VAL_PROTOCOL_SERIAL_STOP_BITS = "SerialStopHalfBits";
    public const string VAL_PROTOCOL_SERIAL_FLOW_CONTROL = "SerialFlowControl";

    public const string VAL_SSH_REMOTE_COMMAND = "RemoteCommand";
    #endregion --- Constants --------------------------------------------

    #region --- Public properties ------------------------------------------------------------------------------
    public string Name { get; set; }
    public string CleanName => Name.Replace("%20", " ");

    public string HostName { get; set; }
    public int Port { get; set; }
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();

    public string SerialLine { get; set; }
    public int SerialSpeed { get; set; }
    public byte SerialDataBits { get; set; }
    public byte SerialStopBits { get; set; }
    public byte SerialParity { get; set; }

    public string SSH_RemoteCommand { get; set; }

    public int PID { get; set; }
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

    public void Dispose() {
      if ( WatchTask != null ) {
        WatchTaskCancellation.Cancel();
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");

      if ( Protocol.IsSSH || Protocol.IsTelnet || Protocol.IsRaw || Protocol.IsRLogin ) {
        RetVal.Append(Protocol.ToString().PadRight(8, '.'));
        if ( !string.IsNullOrWhiteSpace(HostName) ) {
          RetVal.Append($", {HostName}");
        } else {
          RetVal.Append(", N/A");
        }
        RetVal.Append($":{Port}");
        if ( !string.IsNullOrWhiteSpace(SSH_RemoteCommand) ) {
          RetVal.Append($", {SSH_RemoteCommand}");
        }
      }

      if ( Protocol.IsSerial ) {
        RetVal.Append("Serial".PadRight(8, '.'));
        RetVal.Append($", {SerialLine}");
        RetVal.Append($", {SerialSpeed}");
        RetVal.Append($", {SerialDataBits}");
        RetVal.Append($", {SerialStopBits}");
        RetVal.Append($", {SerialParity}");
      }

      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region Public methods
    public TPuttySession ReadFromRegistry(string sessionKeyName = "") {

      if ( !string.IsNullOrWhiteSpace(sessionKeyName) ) {
        Name = sessionKeyName;
      }

      string PuttySessionKeyName = $@"{REG_BASE}\{Name}";
      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {
        HostName = PuttySessionKey.GetValue(VAL_HOSTNAME, "") as string;
        Port = (int)PuttySessionKey.GetValue(VAL_PORT, 0);
        Protocol = PuttySessionKey.GetValue(VAL_PROTOCOL_TYPE, "") as string;
        SerialLine = PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_LINE, "") as string;
        SerialSpeed = (int)PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_SPEED, 0);
        SerialParity = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_PARITY, 0));
        SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_STOP_BITS, 0));
        SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_DATA_BITS, 0));

        SSH_RemoteCommand = PuttySessionKey.GetValue(VAL_SSH_REMOTE_COMMAND, "") as string;
        return this;
      }
    }

    public void Start() {
      Process PuttyProcess = new Process();
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = "putty.exe",
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
            await Task.Delay(50);
          } catch {
            PID = 0;
          }
        }
        if ( OnExit != null ) {
          OnExit(this, EventArgs.Empty);
        }
      }, WatchTaskCancellation.Token);

    }

    public static bool CheckIsRunning(int pid) {
      return Process.GetProcesses().Any(x => x.Id == pid);
    }

    #endregion Public methods

    #region --- Static fields --------------------------------------------
    public static TPuttySession EmptySession {
      get {
        if ( _EmptySesssion == null ) {
          _EmptySesssion = new TPuttySession() { Name = "<Empty>" };
        }
        return _EmptySesssion;
      }
    }
    private static TPuttySession _EmptySesssion;
    #endregion --- Static fields --------------------------------------------

  }
}
