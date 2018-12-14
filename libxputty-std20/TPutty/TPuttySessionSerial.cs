using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLTools;
using Microsoft.Win32;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionSerial : TPuttySession, ISessionTypeSerial, IDisposable {

    #region --- Public properties ------------------------------------------------------------------------------
    public string SerialLine { get; set; }
    public int SerialSpeed { get; set; }
    public byte SerialDataBits { get; set; }
    public byte SerialStopBits { get; set; }
    public string SerialParity { get; set; }
    public string SerialFlowControl { get; set; }

    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSerial(ISessionManager sessionManager) : base(sessionManager) {
      Protocol = TPuttyProtocol.Serial;
    }

    public TPuttySessionSerial(string name, ISessionManager sessionManager) : base(name, sessionManager) {
      Protocol = TPuttyProtocol.Serial;
    }

    public TPuttySessionSerial(IPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.Serial;
      if ( session is ISessionTypeSerial SessionSerial ) {
        SerialLine = SessionSerial.SerialLine;
        SerialSpeed = SessionSerial.SerialSpeed;
        SerialDataBits = SessionSerial.SerialDataBits;
        SerialStopBits = SessionSerial.SerialStopBits;
        SerialParity = SessionSerial.SerialParity;
        SerialFlowControl = SessionSerial.SerialFlowControl;
      }
    }

    public override IPuttySession Duplicate() {
      TPuttySessionSerial RetVal = new TPuttySessionSerial(base.Duplicate());
      RetVal.SerialLine = SerialLine;
      RetVal.SerialSpeed = SerialSpeed;
      RetVal.SerialDataBits = SerialDataBits;
      RetVal.SerialStopBits = SerialStopBits;
      RetVal.SerialParity = SerialParity;
      RetVal.SerialFlowControl = SerialFlowControl;
      return RetVal;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder(base.ToString());
      RetVal.Append("Serial".PadRight(8, '.'));
      RetVal.Append($", {SerialLine}");
      RetVal.Append($", {SerialSpeed}");
      RetVal.Append($", {SerialDataBits}");
      RetVal.Append($", {SerialStopBits}");
      RetVal.Append($", {SerialParity}");
      RetVal.Append($", {SerialFlowControl}");
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region Public methods
    public override void Stop() {
      throw new NotImplementedException();
    }
    #endregion Public methods

    public static TPuttySessionSerial DemoPuttySessionSerial1 {
      get {
        if ( _DemoPuttySessionSerial1 == null ) {
          _DemoPuttySessionSerial1 = new TPuttySessionSerial("Serial Demo1", TSessionManager.DEFAULT_SESSION_MANAGER) {
            SerialLine = "COM4",
            SerialSpeed = 9600,
            SerialDataBits = 8,
            SerialStopBits = 1,
            SerialParity = "None",
            RemoteCommand = "tail -n 200 -f /var/log/syslog"
          };
        }
        return _DemoPuttySessionSerial1;
      }
    }
    private static TPuttySessionSerial _DemoPuttySessionSerial1;
  }
}
