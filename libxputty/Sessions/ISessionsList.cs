using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BLTools.Json;

namespace libxputty {
  public interface ISessionList {

    List<ISession> Items { get; }
    IJsonValue ToJson();

  }
}
