using System;
using System.Collections.Generic;
using System.Linq;

using BLTools;

using libxputty_std20.Interfaces;

using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionSourceRegistry : TPuttySessionSource {

    #region --- Constants --------------------------------------------
    public const string REG_KEYNAME_BASE = @"Software\SimonTatham\PuTTY\Sessions";

    public const string GROUP_SEPARATOR = "//";
    protected const string REG_PROTOCOL_TYPE = "Protocol";
    protected const string REG_USERNAME = "UserName";

    protected const string REG_HOSTNAME = "HostName";
    protected const string REG_PORT = "PortNumber";

    protected const string REG_REMOTE_COMMAND = "RemoteCommand";

    protected const string REG_SERIAL_LINE = "SerialLine";
    protected const string REG_SERIAL_SPEED = "SerialSpeed";
    protected const string REG_SERIAL_DATA_BITS = "SerialDataBits";
    protected const string REG_SERIAL_PARITY = "SerialParity";
    protected const string REG_SERIAL_STOP_BITS = "SerialStopHalfBits";
    protected const string REG_SERIAL_FLOW_CONTROL = "SerialFlowControl";

    public const string DATASOURCE_PREFIX = "reg";
    #endregion --- Constants --------------------------------------------

    public override string DataSourceName => $"{DATASOURCE_PREFIX}://{Location.Replace('\\', '/')}";

    #region --- Constructor(s) --------------------------------------------
    public TPuttySessionSourceRegistry() : base() {
      SourceType = ESourceType.Registry;
      Location = $@"HKCU\{REG_KEYNAME_BASE}";
    }
    #endregion --- Constructor(s) --------------------------------------------

    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {
      using ( RegistryKey BaseKey = Registry.CurrentUser.OpenSubKey(REG_KEYNAME_BASE) ) {
        foreach ( string KeyNameItem in BaseKey.GetSubKeyNames() ) {
          TPuttyProtocol Protocol = BaseKey.OpenSubKey(KeyNameItem).GetValue(REG_PROTOCOL_TYPE, "Unknown") as string;
          yield return (KeyNameItem, Protocol);
        }
      }
      yield break;
    }

    protected override IPuttySession _ReadSession(string name, TPuttyProtocol protocol) {

      IPuttySession BaseSession;
      IPuttySession RetVal;

      using ( RegistryKey PuttySessionKey = _GetRegistryKey(name) ) {

        string CleanedName = name.Replace("%20", " ");

        #region --- Read common data --------------------------------------------
        BaseSession = new TPuttySession(CleanedName) {
          RemoteCommand = PuttySessionKey.GetValue(REG_REMOTE_COMMAND, "") as string,
          Section = CleanedName.Between("{", "}")
        };

        BaseSession.SetLocalCredential(TCredential.Create(PuttySessionKey.GetValue(REG_USERNAME, "") as string, ""));

        string Group = CleanedName.Between("[", "]");
        if ( Group.Contains(GROUP_SEPARATOR) ) {
          BaseSession.GroupLevel1 = Group.Before(GROUP_SEPARATOR);
          BaseSession.GroupLevel2 = Group.After(GROUP_SEPARATOR);
        } else {
          BaseSession.GroupLevel1 = Group;
        }
        #endregion --- Read common data -----------------------------------------

        #region --- Create RetVal based on protocol type --------------------------------------------
        switch ( protocol.Value ) {
          case EPuttyProtocol.SSH:
            RetVal = new TPuttySessionSSH(BaseSession);
            break;

          case EPuttyProtocol.Serial:
            RetVal = new TPuttySessionSerial(BaseSession) {
              SerialLine = PuttySessionKey.GetValue(REG_SERIAL_LINE, "") as string,
              SerialSpeed = (int)PuttySessionKey.GetValue(REG_SERIAL_SPEED, 0),
              SerialParity = PuttySessionKey.GetValue(REG_SERIAL_PARITY, "") as string,
              SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_STOP_BITS, 0)),
              SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_DATA_BITS, 0)),
              SerialFlowControl = PuttySessionKey.GetValue(REG_SERIAL_FLOW_CONTROL, "") as string
            };
            break;

          case EPuttyProtocol.Telnet:
            RetVal = new TPuttySessionTelnet(BaseSession);
            break;

          case EPuttyProtocol.RLogin:
            RetVal = new TPuttySessionRLogin(BaseSession);
            break;

          case EPuttyProtocol.Raw:
            RetVal = new TPuttySessionRaw(BaseSession);
            break;

          default:
            Log.Write("Unable to read session from registry : Protocol type is invalid");
            return TPuttySession.Empty;
        }
        #endregion --- Create RetVal based on protocol type -----------------------------------------

        #region --- Add specific data --------------------------------------------
        if ( RetVal is IHostAndPort SessionHAP ) {
          SessionHAP.HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string;
          SessionHAP.Port = (int)PuttySessionKey.GetValue(REG_PORT, 0);
        }
        #endregion --- Add specific data -----------------------------------------

      }
      return RetVal;
    }

    protected override IEnumerable<IPuttySession> _ReadSessions() {
      IEnumerable<(string, TPuttyProtocol)> SessionsWithProtocol = _GetSessionList();
      if ( !SessionsWithProtocol.Any() ) {
        yield break;
      }

      foreach ( (string SessionNameItem, TPuttyProtocol SessionProtocolItem) in SessionsWithProtocol ) {
        yield return _ReadSession(SessionNameItem, SessionProtocolItem);
      }
    }

    protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
      throw new NotImplementedException();
    }

    protected override void _SaveSession(IPuttySession session) {

      string GL1 = session.GroupLevel1 == "" ? "" : $"[{session.GroupLevel1}]";
      string GL2 = session.GroupLevel2 == "" ? "" : $"[{session.GroupLevel2}]";
      string SCT = session.Section == "" ? "" : $"[{session.Section}]";

      string KeyName = $"{GL1}{GL2}{SCT}{session.Name.Replace(" ", "%20")}";

      using ( RegistryKey PuttySessionKey = _GetRegistryKeyRW(KeyName) ) {

        try {
          PuttySessionKey.SetValue(REG_REMOTE_COMMAND, session.RemoteCommand);

        } catch ( Exception ex ) {
          Log.Write($"Unable to save value to registry key {KeyName} : {ex.Message}");
        }

        if ( session is IHostAndPort SessionHAP ) {
          try {
            PuttySessionKey.SetValue(REG_REMOTE_COMMAND, session.RemoteCommand);
            PuttySessionKey.SetValue(REG_HOSTNAME, SessionHAP.HostName);
            PuttySessionKey.SetValue(REG_PORT, SessionHAP.Port);

          } catch ( Exception ex ) {
            Log.Write($"Unable to save value to registry key {KeyName} : {ex.Message}");
          }
        }

        switch ( session ) {
          case TPuttySessionSSH PuttySessionSSH:
            break;

          case TPuttySessionSerial PuttySessionSerial:
            try {
              PuttySessionKey.SetValue(REG_SERIAL_LINE, PuttySessionSerial.SerialLine);
              PuttySessionKey.SetValue(REG_SERIAL_SPEED, PuttySessionSerial.SerialSpeed);
              PuttySessionKey.SetValue(REG_SERIAL_PARITY, PuttySessionSerial.SerialParity);
              PuttySessionKey.SetValue(REG_SERIAL_STOP_BITS, PuttySessionSerial.SerialStopBits);
              PuttySessionKey.SetValue(REG_SERIAL_DATA_BITS, PuttySessionSerial.SerialDataBits);
              PuttySessionKey.SetValue(REG_SERIAL_FLOW_CONTROL, PuttySessionSerial.SerialFlowControl);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySessionSerial.Name} : {ex.Message}");
            }
            break;

          case TPuttySessionTelnet PuttySessionTelnet:

            break;

          case TPuttySessionRLogin PuttySessionRLogin:

            break;

          case TPuttySessionRaw PuttySessionRaw:

            break;

          default:
            Log.Write("Unable to read session from registry : Protocol type is invalid");
            break;
        }

        PuttySessionKey.Close();
      }
    }

    protected override void _SaveSessions(IEnumerable<IPuttySession> sessions) {
      try {
        foreach ( IPuttySession PuttySessionItem in sessions ) {
          _SaveSession(PuttySessionItem);
        }
      } catch ( Exception ex ) {
        Log.Write($"Unable to save sessions to registry : {ex.Message}");
      }
    }

    private RegistryKey _GetRegistryKey(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.OpenSubKey(PuttySessionKeyName);
    }

    private RegistryKey _GetRegistryKeyRW(string keyName) {
      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
      return Registry.CurrentUser.CreateSubKey(PuttySessionKeyName, true);
    }

    private TPuttyProtocol _GetSessionProtocol(string sessionKeyName = "") {
      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(sessionKeyName) ) {
        return TPuttyProtocol.Unknown;
      }
      #endregion === Validate parameters ===

      string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{sessionKeyName}";
      using ( RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName) ) {
        return PuttySessionKey.GetValue(REG_PROTOCOL_TYPE, "") as string;
      }
    }

  }
}
