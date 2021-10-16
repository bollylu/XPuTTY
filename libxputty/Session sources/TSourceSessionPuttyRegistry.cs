using System;
using System.Collections.Generic;
using System.Linq;

using BLTools;

using Microsoft.Win32;

namespace libxputty {
  public class TSourceSessionPuttyRegistry : ASourceSession {

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
    public TSourceSessionPuttyRegistry() : base() {
      if (!OperatingSystem.IsWindows()) {
        throw new ApplicationException("Unable to create registry source session : registry is only available on Windows");
      }

      SourceType = ESourceType.Registry;
      Location = $@"HKCU\{REG_KEYNAME_BASE}";
    }
    #endregion --- Constructor(s) --------------------------------------------

    protected override IEnumerable<(string, TPuttyProtocol)> _GetSessionList() {
      if (OperatingSystem.IsWindows()) {
        using (RegistryKey BaseKey = Registry.CurrentUser.OpenSubKey(REG_KEYNAME_BASE)) {
          foreach (string KeyNameItem in BaseKey.GetSubKeyNames()) {
            TPuttyProtocol Protocol = BaseKey.OpenSubKey(KeyNameItem).GetValue(REG_PROTOCOL_TYPE, "Unknown") as string;
            yield return (KeyNameItem, Protocol);
          }
        }
      }
      yield break;
    }

    protected override ISessionPutty _ReadSession(string name, TPuttyProtocol protocol) {

      if (!OperatingSystem.IsWindows()) {
        LogError("Unable to read session : registry is only available on Windows");
        return null;
      }

      ISessionPutty RetVal;

      using (RegistryKey PuttySessionKey = _GetRegistryKey(name)) {

        string CleanedName = name.Replace("%20", " ");

        #region --- Read common data --------------------------------------------
        RetVal = ASessionPutty.BuildSessionPutty(protocol.Value, Logger);

        RetVal.Name = CleanedName;
        RetVal.RemoteCommand = PuttySessionKey.GetValue(REG_REMOTE_COMMAND, "") as string;
        RetVal.Section = CleanedName.Between("{", "}");

        RetVal.SetLocalCredential(TCredential.Create(PuttySessionKey.GetValue(REG_USERNAME, "") as string, ""));

        string Group = CleanedName.Between("[", "]");
        if (Group.Contains(GROUP_SEPARATOR)) {
          RetVal.GroupLevel1 = Group.Before(GROUP_SEPARATOR);
          RetVal.GroupLevel2 = Group.After(GROUP_SEPARATOR);
        } else {
          RetVal.GroupLevel1 = Group;
          RetVal.GroupLevel2 = LocalExtensions.EMPTY;
        }
        #endregion --- Read common data -----------------------------------------

        #region --- Create RetVal based on protocol type --------------------------------------------
        switch (RetVal.Protocol.Value) {
          case EPuttyProtocol.Serial when RetVal is TSessionPuttySerial Serial:
            Serial.SerialLine = PuttySessionKey.GetValue(REG_SERIAL_LINE, "") as string;
            Serial.SerialSpeed = (int)PuttySessionKey.GetValue(REG_SERIAL_SPEED, 0);
            Serial.SerialParity = PuttySessionKey.GetValue(REG_SERIAL_PARITY, "") as string;
            Serial.SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_STOP_BITS, 0));
            Serial.SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_DATA_BITS, 0));
            Serial.SerialFlowControl = PuttySessionKey.GetValue(REG_SERIAL_FLOW_CONTROL, "") as string;
            break;

          case EPuttyProtocol.SSH when RetVal is TSessionPuttySsh SSH:
          case EPuttyProtocol.Telnet when RetVal is TSessionPuttyTelnet Telnet:
          case EPuttyProtocol.RLogin when RetVal is TSessionPuttyRLogin RLogin:
          case EPuttyProtocol.Raw when RetVal is TSessionPuttyRaw Raw:
            break;

          default:
            LogError("Unable to read session from registry : Protocol type is invalid");
            return ASessionPutty.Empty;
        }
        #endregion --- Create RetVal based on protocol type -----------------------------------------

        #region --- Add specific data --------------------------------------------
        if (RetVal is IHostAndPort SessionHAP) {
          SessionHAP.HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string;
          SessionHAP.Port = (int)PuttySessionKey.GetValue(REG_PORT, 0);
        }
        #endregion --- Add specific data -----------------------------------------

      }
      return RetVal;
    }

    protected override IEnumerable<ISessionPutty> _ReadSessions() {
      IEnumerable<(string, TPuttyProtocol)> SessionsWithProtocol = _GetSessionList();
      if (!SessionsWithProtocol.Any()) {
        yield break;
      }

      foreach ((string SessionNameItem, TPuttyProtocol SessionProtocolItem) in SessionsWithProtocol) {
        yield return _ReadSession(SessionNameItem, SessionProtocolItem);
      }
    }

    protected override IEnumerable<TPuttySessionGroup> _ReadGroups() {
      throw new NotImplementedException();
    }

    protected override void _SaveSession(ISession session) {

      if (session is not ISessionPutty PuttySession) {
        LogError("Unable to save session : not an IPuttySession");
        return;
      }

      if (!OperatingSystem.IsWindows()) {
        LogError("Unable to save session : Registry is only available on Windows");
        return;
      }

      string GL1 = PuttySession.GroupLevel1.IsEmpty() ? "" : $"[{PuttySession.GroupLevel1}]";
      string GL2 = PuttySession.GroupLevel2.IsEmpty() ? "" : $"[{PuttySession.GroupLevel2}]";
      string SCT = PuttySession.Section.IsEmpty() ? "" : $"[{PuttySession.Section}]";

      string KeyName = $"{GL1}{GL2}{SCT}{session.Name.Replace(" ", "%20")}";

      using (RegistryKey PuttySessionKey = _GetRegistryKeyRW(KeyName)) {

        try {
          PuttySessionKey.SetValue(REG_REMOTE_COMMAND, PuttySession.RemoteCommand);

        } catch (Exception ex) {
          LogError($"Unable to save value to registry key {KeyName} : {ex.Message}");
        }

        if (session is IHostAndPort SessionHAP) {
          try {
            PuttySessionKey.SetValue(REG_REMOTE_COMMAND, PuttySession.RemoteCommand);
            PuttySessionKey.SetValue(REG_HOSTNAME, SessionHAP.HostName);
            PuttySessionKey.SetValue(REG_PORT, SessionHAP.Port);

          } catch (Exception ex) {
            LogError($"Unable to save value to registry key {KeyName} : {ex.Message}");
          }
        }

        switch (session) {
          case TSessionPuttySsh PuttySessionSSH:
            break;

          case TSessionPuttySerial PuttySessionSerial:
            try {
              PuttySessionKey.SetValue(REG_SERIAL_LINE, PuttySessionSerial.SerialLine);
              PuttySessionKey.SetValue(REG_SERIAL_SPEED, PuttySessionSerial.SerialSpeed);
              PuttySessionKey.SetValue(REG_SERIAL_PARITY, PuttySessionSerial.SerialParity);
              PuttySessionKey.SetValue(REG_SERIAL_STOP_BITS, PuttySessionSerial.SerialStopBits);
              PuttySessionKey.SetValue(REG_SERIAL_DATA_BITS, PuttySessionSerial.SerialDataBits);
              PuttySessionKey.SetValue(REG_SERIAL_FLOW_CONTROL, PuttySessionSerial.SerialFlowControl);
              PuttySessionKey.Close();
            } catch (Exception ex) {
              LogError($"Unable to save value to registry key {PuttySessionSerial.Name} : {ex.Message}");
            }
            break;

          case TSessionPuttyTelnet PuttySessionTelnet:

            break;

          case TSessionPuttyRLogin PuttySessionRLogin:

            break;

          case TSessionPuttyRaw PuttySessionRaw:

            break;

          default:
            LogError("Unable to read session from registry : Protocol type is invalid");
            break;
        }

        PuttySessionKey.Close();
      }
    }

    protected override void _SaveSessions(IEnumerable<ISession> sessions) {
      try {
        foreach (ISession SessionItem in sessions) {
          _SaveSession(SessionItem);
        }
      } catch (Exception ex) {
        LogError($"Unable to save sessions to registry : {ex.Message}");
      }
    }

    private RegistryKey _GetRegistryKey(string keyName) {
      if (OperatingSystem.IsWindows()) {
        string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
        return Registry.CurrentUser.OpenSubKey(PuttySessionKeyName);
      }

      return null;
    }

    private RegistryKey _GetRegistryKeyRW(string keyName) {
      if (OperatingSystem.IsWindows()) {
        string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{keyName}";
        return Registry.CurrentUser.CreateSubKey(PuttySessionKeyName, true);
      }

      return null;
    }

    private TPuttyProtocol _GetSessionProtocol(string sessionKeyName = "") {
      if (OperatingSystem.IsWindows()) {
        #region === Validate parameters ===
        if (string.IsNullOrWhiteSpace(sessionKeyName)) {
          return TPuttyProtocol.Unknown;
        }
        #endregion === Validate parameters ===

        string PuttySessionKeyName = $@"{REG_KEYNAME_BASE}\{sessionKeyName}";
        using (RegistryKey PuttySessionKey = Registry.CurrentUser.OpenSubKey(PuttySessionKeyName)) {
          return PuttySessionKey.GetValue(REG_PROTOCOL_TYPE, "") as string;
        }
      }

      return null;
    }

  }
}
