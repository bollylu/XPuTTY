using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20 {
  public class TPuttyProtocol {

    private enum EPuttyProtocol {
      Unknown,
      SSH,
      Serial,
      Telnet,
      Raw,
      RLogin
    }

    private EPuttyProtocol _Value { get; set; }

    public bool IsUnknown => _Value == EPuttyProtocol.Unknown;
    public bool IsSSH => _Value == EPuttyProtocol.SSH;
    public bool IsSerial => _Value == EPuttyProtocol.Serial;
    public bool IsTelnet => _Value == EPuttyProtocol.Telnet;
    public bool IsRaw => _Value == EPuttyProtocol.Raw;
    public bool IsRLogin => _Value == EPuttyProtocol.RLogin;

    public TPuttyProtocol() {
      _Value = EPuttyProtocol.Unknown;
    }

    public static TPuttyProtocol Unknown {
      get {
        if ( _Unknown == null ) {
          _Unknown = new TPuttyProtocol() { _Value = EPuttyProtocol.Unknown };
        }
        return _Unknown;
      }
    }
    private static TPuttyProtocol _Unknown;
    public static TPuttyProtocol SSH {
      get {
        if ( _SSH == null ) {
          _SSH = new TPuttyProtocol() { _Value = EPuttyProtocol.SSH };
        }
        return _SSH;
      }
    }
    private static TPuttyProtocol _SSH;
    public static TPuttyProtocol Serial {
      get {
        if ( _Serial == null ) {
          _Serial = new TPuttyProtocol() { _Value = EPuttyProtocol.Serial };
        }
        return _Serial;
      }
    }
    private static TPuttyProtocol _Serial;
    public static TPuttyProtocol Telnet {
      get {
        if ( _Telnet == null ) {
          _Telnet = new TPuttyProtocol() { _Value = EPuttyProtocol.Telnet };
        }
        return _Telnet;
      }
    }
    private static TPuttyProtocol _Telnet;
    public static TPuttyProtocol Raw {
      get {
        if ( _Raw == null ) {
          _Raw = new TPuttyProtocol() { _Value = EPuttyProtocol.Raw };
        }
        return _Raw;
      }
    }
    private static TPuttyProtocol _Raw;
    public static TPuttyProtocol RLogin {
      get {
        if ( _RLogin == null ) {
          _RLogin = new TPuttyProtocol() { _Value = EPuttyProtocol.RLogin };
        }
        return _RLogin;
      }
    }
    private static TPuttyProtocol _RLogin;

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      string Text = this;
      RetVal.Append(Text);
      return RetVal.ToString();
    }

    public static implicit operator TPuttyProtocol(string source) {
      if ( string.IsNullOrWhiteSpace(source) ) {
        return Unknown;
      }

      switch ( source.ToUpper() ) {
        case "SSH":
          return SSH;
        case "SERIAL":
          return Serial;
        case "TELNET":
          return Telnet;
        case "RAW":
          return Raw;
        case "RLOGIN":
          return RLogin;
      }
      return Unknown;
    }

    public static implicit operator string(TPuttyProtocol source) {
      if ( source == null ) {
        return default;
      }

      switch ( source._Value ) {
        case EPuttyProtocol.Unknown:
          return "Unknown";
        case EPuttyProtocol.SSH:
          return "SSH";
        case EPuttyProtocol.Serial:
          return "Serial";
        case EPuttyProtocol.Telnet:
          return "Telnet";
        case EPuttyProtocol.Raw:
          return "Raw";
        case EPuttyProtocol.RLogin:
          return "RLogin";
      }
      return default;
    }
  }
}
