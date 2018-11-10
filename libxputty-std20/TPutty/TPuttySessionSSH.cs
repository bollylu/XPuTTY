using System;
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

    #region --- Constants --------------------------------------------
    protected const string REG_HOSTNAME = "HostName";
    protected const string REG_PORT = "PortNumber";
    protected const string REG_SSH_REMOTE_COMMAND = "RemoteCommand";

    protected const string JSON_HOSTNAME = "HostName";
    protected const string JSON_PORT = "PortNumber";
    protected const string JSON_SSH_REMOTE_COMMAND = "RemoteCommand"; 
    #endregion --- Constants --------------------------------------------

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
      Session.Add(JSON_HOSTNAME, HostName);
      Session.Add(JSON_PORT, Port);
      Session.Add(JSON_SSH_REMOTE_COMMAND, Convert.ToBase64String(RemoteCommand.ToByteArray()));
      RetVal.Clear();
      RetVal.Add(TPuttySession.JSON_THIS_ELEMENT, Session);
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public async override void Start(string arguments="") {
      TempFileForRemoteCommand = Path.GetTempFileName();
      Log.Write($"Created Tempfile {TempFileForRemoteCommand}");
      File.WriteAllText(TempFileForRemoteCommand, RemoteCommand);
      base.Start($"-ssh -l root -pw NN2003Pass -P {Port} {HostName} -t -m \"{TempFileForRemoteCommand}\"");
      await Task.Delay(500);
      SetProcessTitle($"SSH {HostName}:{Port} \"{RemoteCommand}\"");
    }

    public async override void StartPlink(string arguments = "") {
      base.StartPlink($"-t -l root -pw NN2003Pass -P {Port} {HostName} \"{RemoteCommand.Replace(@"""", @"\""")}\"");
      await Task.Delay(500);
      SetProcessTitle($"SSH {HostName}:{Port} \"{RemoteCommand}\"");
    }
  }
}
