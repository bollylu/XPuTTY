using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionRaw : TPuttySession, ISessionTypeNetwork {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionRaw(ISessionManager sessionManager) : base(sessionManager) {
      Protocol = TPuttyProtocol.Raw;
    }
    public TPuttySessionRaw(string name, ISessionManager sessionManager) : base(name, sessionManager) {
      Protocol = TPuttyProtocol.Raw;
    }

    public TPuttySessionRaw(IPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.Raw;
      if ( session is ISessionTypeNetwork SessionNetwork ) {
        HostName = SessionNetwork.HostName;
        Port = SessionNetwork.Port;
      }
    }

    public override void Dispose() {
      base.Dispose();
    }

    public override IPuttySession Duplicate() {
      TPuttySessionRaw RetVal = new TPuttySessionRaw(base.Duplicate());
      RetVal.HostName = HostName;
      RetVal.Port = Port;
      return RetVal;
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
