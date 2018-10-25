using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BLTools;
using Microsoft.Win32;

namespace libxputty_std20 {
  public class TPuttySessionSerial : TPuttySession, IDisposable {

    protected const string VAL_PROTOCOL_SERIAL_LINE = "SerialLine";
    protected const string VAL_PROTOCOL_SERIAL_SPEED = "SerialSpeed";
    protected const string VAL_PROTOCOL_SERIAL_DATA_BITS = "SerialDataBits";
    protected const string VAL_PROTOCOL_SERIAL_PARITY = "SerialParity";
    protected const string VAL_PROTOCOL_SERIAL_STOP_BITS = "SerialStopHalfBits";
    protected const string VAL_PROTOCOL_SERIAL_FLOW_CONTROL = "SerialFlowControl";

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
    public override IPuttySession LoadFromRegistry() {

      #region === Validate parameters ===
      if ( string.IsNullOrWhiteSpace(Name) ) {
        return null;
      }
      #endregion === Validate parameters ===

      using ( RegistryKey PuttySessionKey = TPuttySession.GetRegistryKey(Name) ) {
        SerialLine = PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_LINE, "") as string;
        SerialSpeed = (int)PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_SPEED, 0);
        SerialParity = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_PARITY, 0));
        SerialStopBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_STOP_BITS, 0));
        SerialDataBits = Convert.ToByte(PuttySessionKey.GetValue(VAL_PROTOCOL_SERIAL_DATA_BITS, 0));
      }

      return this;
    }

    public override void SaveToRegistry() {
      using ( RegistryKey SessionKey = GetRegistryKeyRW(Name) ) {
        try {
          SessionKey.SetValue(VAL_PROTOCOL_SERIAL_LINE, SerialLine);
          SessionKey.SetValue(VAL_PROTOCOL_SERIAL_SPEED, SerialSpeed);
          SessionKey.SetValue(VAL_PROTOCOL_SERIAL_PARITY, SerialParity);
          SessionKey.SetValue(VAL_PROTOCOL_SERIAL_STOP_BITS, SerialStopBits);
          SessionKey.SetValue(VAL_PROTOCOL_SERIAL_DATA_BITS, SerialDataBits);
          SessionKey.Close();
        } catch ( Exception ex ) {
          Log.Write($"Unable to save value to registry key {SessionKey.Name} : {ex.Message}");
        }
      }
    }
    public override void Stop() {
      throw new NotImplementedException();
    }

    #endregion Public methods

  }
}
