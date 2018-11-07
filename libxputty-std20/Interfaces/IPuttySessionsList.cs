using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BLTools.Json;
using libxputty_std20;

namespace libxputty_std20.Interfaces {
  public interface IPuttySessionsList {

    List<IPuttySession> Items { get; }
    IJsonValue ToJson();

  }
}
