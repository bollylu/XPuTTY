using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySession {

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

    public string Name { get; set; }
    public string DisplayName => Name.Replace("%20", " ");

    public string HostName { get; set; }
    public int Port { get; set; }
    public TPuttyProtocol Protocol { get; set; } = new TPuttyProtocol();

    public string SerialLine { get; set; }
    public int SerialSpeed { get; set; }
    public byte SerialDataBits { get; set; }
    public byte SerialStopBits { get; set; }
    public byte SerialParity { get; set; }

    public string SSH_RemoteCommand { get; set; }

    public TPuttySession() { }


    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {DisplayName.PadRight(80, '.')} : ");

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
      ProcessStartInfo StartInfo = new ProcessStartInfo();
      StartInfo.FileName = "putty.exe";
      StartInfo.Arguments = $"-load {"\"" + DisplayName + "\""}";
      Console.WriteLine($"Loading {StartInfo.Arguments} ...");
      PuttyProcess.StartInfo = StartInfo;

      PuttyProcess.Start();
    }
  }
}
