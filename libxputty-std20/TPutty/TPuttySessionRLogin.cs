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
  public class TPuttySessionRLogin : TPuttySession, ISessionTypeNetwork {

    #region --- Public properties ------------------------------------------------------------------------------
    public string HostName { get; set; }
    public int Port { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionRLogin() : base() {
      Protocol = TPuttyProtocol.RLogin;
    }
    public TPuttySessionRLogin(string name) : base(name) {
      Protocol = TPuttyProtocol.RLogin;
    }

    public TPuttySessionRLogin(IPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.RLogin;
      if ( session is ISessionTypeNetwork SessionHAP ) {
        HostName = SessionHAP.HostName;
        Port = SessionHAP.Port;
      }
    }

    public override void Dispose() {
      base.Dispose();
    }
    public override IPuttySession Duplicate() {
      TPuttySessionRLogin RetVal = new TPuttySessionRLogin(base.Duplicate());
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
