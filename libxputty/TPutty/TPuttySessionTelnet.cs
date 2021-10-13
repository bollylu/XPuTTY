using System;
using System.Text;

using libxputty.Interfaces;

namespace libxputty {
  public class TPuttySessionTelnet : TPuttySession, IHostAndPort {

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
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");

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
