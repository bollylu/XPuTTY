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

using Microsoft.Win32;

namespace libxputty {
  public class TSessionPuttySsh : ASessionPutty, IHostAndPort {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSessionPuttySsh() : base() {
      Protocol = TPuttyProtocol.SSH;
    }
    public TSessionPuttySsh(string name) : base(name) {
      Protocol = TPuttyProtocol.SSH;
    }

    public TSessionPuttySsh(ISessionPutty session) : base(session) {
      Protocol = TPuttyProtocol.SSH;
      if ( session is IHostAndPort SessionHAP ) {
        HostName = SessionHAP.HostName;
        Port = SessionHAP.Port;
      }
      
    }

    public override void Dispose() {
      base.Dispose();
    }

    public override ISession Duplicate() {
      return new TSessionPuttySsh(this);
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
        Log($"Created Tempfile {TempFileForRemoteCommand}");
        File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      }

      string Args;
      if ( arguments == "" ) {
        TCommandLineBuilderPutty Builder = new TCommandLineBuilderPutty()
                                                 .AddCredentials(Credential)
                                                 .AddHostnameAndPort(HostName, Port)
                                                 .AddRemoteCommand(TempFileForRemoteCommand);
        Args = Builder.Build();
      } else {
        Args = arguments;
      }

      switch ( SessionType ) {

        case ESessionPuttyType.Putty:
          _StartPutty(Args);
          break;

        case ESessionPuttyType.Plink:
          _StartPlink(Args);
          break;

        case ESessionPuttyType.Auto:
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
