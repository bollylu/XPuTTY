using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty.Interfaces {
  public interface ISerial {

    string SerialLine { get; set; }
    int SerialSpeed { get; set; }
    byte SerialDataBits { get; set; }
    byte SerialStopBits { get; set; }
    string SerialParity { get; set; }
    string SerialFlowControl { get; set; }

  }
}
