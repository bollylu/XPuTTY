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
  public class TPuttySessionSerial : TPuttySession, IDisposable {

    #region --- Public properties ------------------------------------------------------------------------------
    public string SerialLine { get; set; }
    public int SerialSpeed { get; set; }
    public byte SerialDataBits { get; set; }
    public byte SerialStopBits { get; set; }
    public byte SerialParity { get; set; }
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionSerial() : base() {
      Protocol = TPuttyProtocol.Serial;
    }
    public TPuttySessionSerial(string name) : base(name) {
      Protocol = TPuttyProtocol.Serial;
    }

    public TPuttySessionSerial(TPuttySession session) : base(session) {
      Protocol = TPuttyProtocol.Serial;
      if ( session is TPuttySessionSerial SessionSerial ) {
        SerialLine = SessionSerial.SerialLine;
        SerialSpeed = SessionSerial.SerialSpeed;
        SerialDataBits = SessionSerial.SerialDataBits;
        SerialStopBits = SessionSerial.SerialStopBits;
        SerialParity = SessionSerial.SerialParity;
      }
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
      return RetVal.ToString();
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    #region Public methods
    public override void Stop() {
      throw new NotImplementedException();
    }

    #endregion Public methods

  }
}
