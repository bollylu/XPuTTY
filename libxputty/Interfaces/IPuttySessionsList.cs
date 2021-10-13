using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BLTools.Json;
using libxputty;

namespace libxputty.Interfaces {
  public interface IPuttySessionsList {

    List<IPuttySession> Items { get; }
    IJsonValue ToJson();

  }
}
