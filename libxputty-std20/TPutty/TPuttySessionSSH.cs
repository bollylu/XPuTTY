using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using BLTools;
using BLTools.Json;

using libxputty_std20.Interfaces;

using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionSSH : TPuttySession, IHostAndPort {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSSH() : base() {
      Protocol = TPuttyProtocol.SSH;
    }
    public TPuttySessionSSH(string name) : base(name) {
      Protocol = TPuttyProtocol.SSH;
    }

    public TPuttySessionSSH(IPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.SSH;
      if ( session is IHostAndPort SessionHAP ) {
        HostName = SessionHAP.HostName;
        Port = SessionHAP.Port;
      }
      
    }

    public override void Dispose() {
      base.Dispose();
    }

    public override IPuttySession Duplicate() {
      TPuttySessionSSH RetVal = new TPuttySessionSSH(base.Duplicate());
      RetVal.HostName = HostName;
      RetVal.Port = Port;
      return RetVal;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder(base.ToString());
      RetVal.Append(Protocol.ToString().PadRight(8, '.'));
      if ( !string.IsNullOrWhiteSpace(HostName) ) {
        RetVal.Append($", {HostName}");
      } else {
        RetVal.Append(", N/A");
      }
      RetVal.Append($":{Port}");
      if ( !string.IsNullOrWhiteSpace(RemoteCommand) ) {
        RetVal.Append($", {RemoteCommand}");
      }

      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public override void Start(string arguments = "") {

      if ( RemoteCommand != "" ) {
        TempFileForRemoteCommand = Path.GetTempFileName();
        Log.Write($"Created Tempfile {TempFileForRemoteCommand}");
        File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      }

      string Args;
      if ( arguments == "" ) {
        IList<string> BaseArguments = CommandLineBuilder.BuildSSHCommandLine()
                                                        .AddCredentialsToCommandLine(Credential)
                                                        .AddHostnameAndPort(HostName, Port)
                                                        .AddRemoteCommandToCommandLine(TempFileForRemoteCommand);

        Args = string.Join(" ", BaseArguments);
      } else {
        Args = arguments;
      }

      switch ( SessionType ) {

        case ESessionType.Putty:
          _StartPutty(Args);
          break;

        case ESessionType.Plink:
          _StartPlink(Args);
          break;

        case ESessionType.Auto:
        default: {
            if ( RemoteCommand != "" ) {
              _StartPlink(Args);
            } else {
              _StartPutty(Args);
            }
            break;
          }

      }

    }

    protected override void _StartPutty(string arguments) {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PUTTY,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start(true);

    }
    protected async override void _StartPlink(string arguments = "") {
      ProcessStartInfo StartInfo = new ProcessStartInfo {
        FileName = EXECUTABLE_PLINK,
        Arguments = arguments == "" ? $"-load {"\"" + CleanName + "\""}" : arguments,
        UseShellExecute = false
      };
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();

      await Task.Delay(500);
      SetProcessTitle($"SSH {HostName}:{Port} \"{RemoteCommand}\"");
    }

  }
}
