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

    protected const string REG_PROTOCOL_TYPE = "Protocol";

    protected const string REG_HOSTNAME = "HostName";
    protected const string REG_PORT = "PortNumber";

    protected const string REG_REMOTE_COMMAND = "RemoteCommand";

    protected const string REG_SERIAL_LINE = "SerialLine";
    protected const string REG_SERIAL_SPEED = "SerialSpeed";
    protected const string REG_SERIAL_DATA_BITS = "SerialDataBits";
    protected const string REG_SERIAL_PARITY = "SerialParity";
    protected const string REG_SERIAL_STOP_BITS = "SerialStopHalfBits";
    protected const string REG_SERIAL_FLOW_CONTROL = "SerialFlowControl";
    #endregion --- Constants --------------------------------------------

    public override string DataSourceName => $"reg://{Location.Replace('\\', '/')}";

    public TPuttySessionSourceRegistry() : base() {
      SourceType = ESourceType.Registry;
      Location = $@"HKCU\{REG_KEYNAME_BASE}";
    }

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

      string CleanName = name.Replace("%20", "");

      switch ( protocol.Value ) {
        case EPuttyProtocol.SSH:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(name) ) {
            TPuttySessionSSH NewSession = new TPuttySessionSSH(name) {
              HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string,
              Port = (int)PuttySessionKey.GetValue(REG_PORT, 0),
              RemoteCommand = PuttySessionKey.GetValue(REG_REMOTE_COMMAND, "") as string
            };
            return NewSession;
          }

        case EPuttyProtocol.Serial:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(Name) ) {
            TPuttySessionSerial NewSession = new TPuttySessionSerial(name) {
              SerialLine = PuttySessionKey.GetValue(REG_SERIAL_LINE, "") as string,
              SerialSpeed = (int)PuttySessionKey.GetValue(REG_SERIAL_SPEED, 0),
              SerialParity = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_PARITY, 0)),
              SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_STOP_BITS, 0)),
              SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(REG_SERIAL_DATA_BITS, 0))
            };
            return NewSession;
          }

        case EPuttyProtocol.Telnet:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(name) ) {
            TPuttySessionTelnet NewSession = new TPuttySessionTelnet(name) {
              HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string,
              Port = (int)PuttySessionKey.GetValue(REG_PORT, 0),
            };
            return NewSession;
          }

        case EPuttyProtocol.RLogin:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(name) ) {
            TPuttySessionRLogin NewSession = new TPuttySessionRLogin(name) {
              HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string,
              Port = (int)PuttySessionKey.GetValue(REG_PORT, 0),
            };
            return NewSession;
          }

        case EPuttyProtocol.Raw:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(name) ) {
            TPuttySessionRaw NewSession = new TPuttySessionRaw(name) {
              HostName = PuttySessionKey.GetValue(REG_HOSTNAME, "") as string,
              Port = (int)PuttySessionKey.GetValue(REG_PORT, 0),
            };
            return NewSession;
          }

        default:
          Log.Write("Unable to read session from registry : Protocol type is invalid");
          return TPuttySession.Empty;
      }
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

    protected override void _SaveSession(IPuttySession session) {

      switch ( session ) {
        case TPuttySessionSSH PuttySession:
          using ( RegistryKey PuttySessionKey = _GetRegistryKeyRW(PuttySession.Name) ) {
            try {
              PuttySessionKey.SetValue(REG_REMOTE_COMMAND, PuttySession.RemoteCommand);
              PuttySessionKey.SetValue(REG_HOSTNAME, PuttySession.HostName);
              PuttySessionKey.SetValue(REG_PORT, PuttySession.Port);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySession.Name} : {ex.Message}");
            }
          };
          return;

        case TPuttySessionSerial PuttySession:
          using ( RegistryKey PuttySessionKey = _GetRegistryKey(Name) ) {
            try {
              PuttySessionKey.SetValue(REG_SERIAL_LINE, PuttySession.SerialLine);
              PuttySessionKey.SetValue(REG_SERIAL_SPEED, PuttySession.SerialSpeed);
              PuttySessionKey.SetValue(REG_SERIAL_PARITY, PuttySession.SerialParity);
              PuttySessionKey.SetValue(REG_SERIAL_STOP_BITS, PuttySession.SerialStopBits);
              PuttySessionKey.SetValue(REG_SERIAL_DATA_BITS, PuttySession.SerialDataBits);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySession.Name} : {ex.Message}");
            }
            return;
          }

        case TPuttySessionTelnet PuttySession:
          using ( RegistryKey PuttySessionKey = _GetRegistryKeyRW(PuttySession.Name) ) {
            try {
              PuttySessionKey.SetValue(REG_HOSTNAME, PuttySession.HostName);
              PuttySessionKey.SetValue(REG_PORT, PuttySession.Port);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySession.Name} : {ex.Message}");
            }
          };
          return;

        case TPuttySessionRLogin PuttySession:
          using ( RegistryKey PuttySessionKey = _GetRegistryKeyRW(PuttySession.Name) ) {
            try {
              PuttySessionKey.SetValue(REG_HOSTNAME, PuttySession.HostName);
              PuttySessionKey.SetValue(REG_PORT, PuttySession.Port);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySession.Name} : {ex.Message}");
            }
          };
          return;

        case TPuttySessionRaw PuttySession:
          using ( RegistryKey PuttySessionKey = _GetRegistryKeyRW(PuttySession.Name) ) {
            try {
              PuttySessionKey.SetValue(REG_HOSTNAME, PuttySession.HostName);
              PuttySessionKey.SetValue(REG_PORT, PuttySession.Port);
              PuttySessionKey.Close();
            } catch ( Exception ex ) {
              Log.Write($"Unable to save value to registry key {PuttySession.Name} : {ex.Message}");
            }
          };
          return;

        default:
          Log.Write("Unable to read session from registry : Protocol type is invalid");
          return;
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
