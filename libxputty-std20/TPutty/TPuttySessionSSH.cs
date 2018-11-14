using System;
using System.Collections.Generic;
using System.IO;
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
      SetLocalCredential(session.Credential);
    }

    public override void Dispose() {
      base.Dispose();
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

    public override IJsonValue ToJson() {
      JsonObject RetVal = base.ToJson() as JsonObject;
      JsonObject Session = RetVal.SafeGetValueFirst<JsonObject>(TPuttySession.JSON_THIS_ELEMENT);
      RetVal.Clear();
      RetVal.Add(TPuttySession.JSON_THIS_ELEMENT, Session);
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public async override void Start(string arguments = "") {
      TempFileForRemoteCommand = Path.GetTempFileName();
      Log.Write($"Created Tempfile {TempFileForRemoteCommand}");
      File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);

      List<string> Params = new List<string> {
        "-t",
        "-ssh",
        $"-P {Port}",
        $"-m \"{TempFileForRemoteCommand}\"",
        HostName
      };
      if ( Credential != null ) {
        Params.Add($"-l {Credential.Username}");
        Params.Add($"-pw {Credential.SecurePassword.ConvertToUnsecureString()}");
      }

      base.Start(Params);

      await Task.Delay(500);
      SetProcessTitle($"SSH {HostName}:{Port} \"{RemoteCommand}\"");
    }

    public async override void StartPlink(string arguments = "") {
      TempFileForRemoteCommand = Path.GetTempFileName();
      Log.Write($"Created Tempfile {TempFileForRemoteCommand}");
      File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      Log.Write(File.ReadAllText(TempFileForRemoteCommand));

      List<string> Params = new List<string> {
        "-t",
        "-ssh",
        $"-P {Port}",
        $"-m \"{TempFileForRemoteCommand}\"",
        HostName
      };
      if ( Credential != null ) {
        Params.Add($"-l {Credential.UsernameWithoutDomain}");
        Params.Add($"-pw {Credential.SecurePassword.ConvertToUnsecureString()}");
      }

      base.StartPlink(Params);

      await Task.Delay(500);
      SetProcessTitle($"SSH {HostName}:{Port} \"{RemoteCommand}\"");
    }
  }
}
