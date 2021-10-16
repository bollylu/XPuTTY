using System;
using System.Text;

namespace libxputty {
  public class TSessionPuttyTelnet : ASessionPutty, IHostAndPort {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TSessionPuttyTelnet() : base() {
      Protocol = TPuttyProtocol.Telnet;
    }
    public TSessionPuttyTelnet(string name) : base(name) {
      Protocol = TPuttyProtocol.Telnet;
    }

    public TSessionPuttyTelnet(ISessionPutty session) : base(session) {
      Protocol = TPuttyProtocol.Telnet;
      if ( session is IHostAndPort SessionHAP ) {
        HostName = SessionHAP.HostName;
        Port = SessionHAP.Port;
      }
    }

    public override void Dispose() {
      base.Dispose();
    }

    public override ISession Duplicate() {
      return new TSessionPuttyTelnet(this);
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
