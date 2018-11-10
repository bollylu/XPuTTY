using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20.Interfaces {
  public interface IHostAndPort {
    string HostName { get; set; }
    int Port { get; set; }
  }
}
