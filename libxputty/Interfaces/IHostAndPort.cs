﻿using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty {
  public interface IHostAndPort {
    string HostName { get; set; }
    int Port { get; set; }
  }
}
