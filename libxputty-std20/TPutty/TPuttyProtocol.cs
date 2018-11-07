using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20 {

  public enum EPuttyProtocol {
    Unknown,
    SSH,
    Serial,
    Telnet,
    Raw,
    RLogin
  }

  public class TPuttyProtocol {

    public EPuttyProtocol Value { get; set; }

    public bool IsUnknown => Value == EPuttyProtocol.Unknown;
    public bool IsSSH => Value == EPuttyProtocol.SSH;
    public bool IsSerial => Value == EPuttyProtocol.Serial;
    public bool IsTelnet => Value == EPuttyProtocol.Telnet;
    public bool IsRaw => Value == EPuttyProtocol.Raw;
    public bool IsRLogin => Value == EPuttyProtocol.RLogin;

    public TPuttyProtocol() {
      Value = EPuttyProtocol.Unknown;
    }

    public static TPuttyProtocol Unknown {
      get {
        if ( _Unknown == null ) {
          _Unknown = new TPuttyProtocol() { Value = EPuttyProtocol.Unknown };
        }
        return _Unknown;
      }
    }
    private static TPuttyProtocol _Unknown;
    public static TPuttyProtocol SSH {
      get {
        if ( _SSH == null ) {
          _SSH = new TPuttyProtocol() { Value = EPuttyProtocol.SSH };
        }
        return _SSH;
      }
    }
    private static TPuttyProtocol _SSH;
    public static TPuttyProtocol Serial {
      get {
        if ( _Serial == null ) {
          _Serial = new TPuttyProtocol() { Value = EPuttyProtocol.Serial };
        }
        return _Serial;
      }
    }
    private static TPuttyProtocol _Serial;
    public static TPuttyProtocol Telnet {
      get {
        if ( _Telnet == null ) {
          _Telnet = new TPuttyProtocol() { Value = EPuttyProtocol.Telnet };
        }
        return _Telnet;
      }
    }
    private static TPuttyProtocol _Telnet;
    public static TPuttyProtocol Raw {
      get {
        if ( _Raw == null ) {
          _Raw = new TPuttyProtocol() { Value = EPuttyProtocol.Raw };
        }
        return _Raw;
      }
    }
    private static TPuttyProtocol _Raw;
    public static TPuttyProtocol RLogin {
      get {
        if ( _RLogin == null ) {
          _RLogin = new TPuttyProtocol() { Value = EPuttyProtocol.RLogin };
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

      switch ( source.Value ) {
        case EPuttyProtocol.Unknown:
          return "UNKNOWN";
        case EPuttyProtocol.SSH:
          return "SSH";
        case EPuttyProtocol.Serial:
          return "SERIAL";
        case EPuttyProtocol.Telnet:
          return "TELNET";
        case EPuttyProtocol.Raw:
          return "RAW";
        case EPuttyProtocol.RLogin:
          return "RLOGIN";
      }
      return default;
    }
  }
}
