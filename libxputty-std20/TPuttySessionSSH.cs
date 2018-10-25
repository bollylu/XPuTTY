using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BLTools;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionSSH : TPuttySession, IDisposable {

    protected const string REG_HOSTNAME = "HostName";
    protected const string REG_PORT = "PortNumber";
    protected const string REG_SSH_REMOTE_COMMAND = "RemoteCommand";

    protected const string XML_ATTRIBUTE_HOSTNAME = "HostName";
    protected const string XML_ATTRIBUTE_PORT = "PortNumber";
    protected const string XML_ATTRIBUTE_SSH_REMOTE_COMMAND = "RemoteCommand";

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    public string RemoteCommand { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSSH() : base() {
      Protocol = TPuttyProtocol.SSH;
    }
    public TPuttySessionSSH(string name) : base(name) {
      Protocol = TPuttyProtocol.SSH;
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

    public override XElement ToXml() {
      XElement RetVal = base.ToXml();
      RetVal.SetAttributeValue(XML_ATTRIBUTE_HOSTNAME, HostName);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_PORT, Port);
      RetVal.SetElementValue(XML_ATTRIBUTE_SSH_REMOTE_COMMAND, RemoteCommand);
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    public override IPuttySession LoadFromRegistry() {

      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Name) ) {
        return null;
      }
      #endregion === Validate parameters ===

      using ( RegistryKey PuttySessionKey = GetRegistryKey(Name) ) {
        HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string;
        Port = (int)PuttySessionKey.GetValue(REG_PORT, 0);
        RemoteCommand = PuttySessionKey.GetValue(REG_SSH_REMOTE_COMMAND, "") as string;
      }

      return this;
    }

    public override void SaveToRegistry() {
      using ( RegistryKey SessionKey = GetRegistryKeyRW(Name) ) {
        try {
          SessionKey.SetValue(REG_SSH_REMOTE_COMMAND, RemoteCommand);
          SessionKey.SetValue(REG_HOSTNAME, HostName);
          SessionKey.SetValue(REG_PORT, Port);
          SessionKey.Close();
        } catch (Exception ex) {
          Log.Write($"Unable to save value to registry key {SessionKey.Name} : {ex.Message}");
        }
      }
    }
  }
}
