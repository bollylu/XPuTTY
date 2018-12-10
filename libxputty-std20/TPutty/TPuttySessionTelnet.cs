using System;
using System.Text;

using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionTelnet : TPuttySession, ISessionTypeNetwork {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionTelnet() : base() {
      Protocol = TPuttyProtocol.Telnet;
    }
    public TPuttySessionTelnet(string name) : base(name) {
      Protocol = TPuttyProtocol.Telnet;
    }

    public TPuttySessionTelnet(IPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.Telnet;
      if ( session is ISessionTypeNetwork SessionHAP ) {
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
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {Name.PadRight(80, '.')} : ");

      RetVal.Append(Protocol.ToString().PadRight(8, '.'));
      if ( !string.IsNullOrWhiteSpace(HostName) ) {
        RetVal.Append($", {HostName}");
      } else {
        RetVal.Append(", N/A");
      }
      RetVal.Append($":{Port}");

      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

  }
}
