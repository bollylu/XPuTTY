using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

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

    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Session {CleanName.PadRight(80, '.')} : ");
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
