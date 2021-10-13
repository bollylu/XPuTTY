using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty {

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

    #region --- Tests --------------------------------------------
    public bool IsUnknown => Value == EPuttyProtocol.Unknown;
    public bool IsSSH => Value == EPuttyProtocol.SSH;
    public bool IsSerial => Value == EPuttyProtocol.Serial;
    public bool IsTelnet => Value == EPuttyProtocol.Telnet;
    public bool IsRaw => Value == EPuttyProtocol.Raw;
    public bool IsRLogin => Value == EPuttyProtocol.RLogin; 
    #endregion --- Tests --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttyProtocol() {
      Value = EPuttyProtocol.Unknown;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Static builders --------------------------------------------
    public static TPuttyProtocol Unknown
    {
      get
      {
        if (_Unknown is null) {
          _Unknown = new TPuttyProtocol() { Value = EPuttyProtocol.Unknown };
        }
        return _Unknown;
      }
    }
    private static TPuttyProtocol _Unknown;
    public static TPuttyProtocol SSH
    {
      get
      {
        if (_SSH is null) {
          _SSH = new TPuttyProtocol() { Value = EPuttyProtocol.SSH };
        }
        return _SSH;
      }
    }
    private static TPuttyProtocol _SSH;
    public static TPuttyProtocol Serial
    {
      get
      {
        if (_Serial is null) {
          _Serial = new TPuttyProtocol() { Value = EPuttyProtocol.Serial };
        }
        return _Serial;
      }
    }
    private static TPuttyProtocol _Serial;
    public static TPuttyProtocol Telnet
    {
      get
      {
        if (_Telnet is null) {
          _Telnet = new TPuttyProtocol() { Value = EPuttyProtocol.Telnet };
        }
        return _Telnet;
      }
    }
    private static TPuttyProtocol _Telnet;
    public static TPuttyProtocol Raw
    {
      get
      {
        if (_Raw is null) {
          _Raw = new TPuttyProtocol() { Value = EPuttyProtocol.Raw };
        }
        return _Raw;
      }
    }
    private static TPuttyProtocol _Raw;
    public static TPuttyProtocol RLogin
    {
      get
      {
        if (_RLogin is null) {
          _RLogin = new TPuttyProtocol() { Value = EPuttyProtocol.RLogin };
        }
        return _RLogin;
      }
    }
    private static TPuttyProtocol _RLogin;
    #endregion --- Static builders --------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      string Text = this;
      RetVal.Append(Text);
      return RetVal.ToString();
    } 
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region --- Operators --------------------------------------------
    public static implicit operator TPuttyProtocol(string source) {
      if (string.IsNullOrWhiteSpace(source)) {
        return Unknown;
      }

      return source.ToUpper() switch {
        "SSH" => SSH,
        "SERIAL" => Serial,
        "TELNET" => Telnet,
        "RAW" => Raw,
        "RLOGIN" => RLogin,
        _ => Unknown,
      };
    }

    public static implicit operator string(TPuttyProtocol source) {
      if (source is null) {
        return default;
      }

      return source.Value switch {
        EPuttyProtocol.Unknown => EPuttyProtocol.Unknown.ToString(),
        EPuttyProtocol.SSH => EPuttyProtocol.SSH.ToString(),
        EPuttyProtocol.Serial => EPuttyProtocol.Serial.ToString(),
        EPuttyProtocol.Telnet => EPuttyProtocol.Telnet.ToString(),
        EPuttyProtocol.Raw => EPuttyProtocol.Raw.ToString(),
        EPuttyProtocol.RLogin => EPuttyProtocol.RLogin.ToString(),
        _ => default,
      };
    } 
    #endregion --- Operators --------------------------------------------
  }
}
