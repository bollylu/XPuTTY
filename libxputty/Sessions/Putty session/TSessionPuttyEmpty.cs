using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty {
  public class TSessionPuttyEmpty : ASessionPutty {

    public TSessionPuttyEmpty() {
      Name = "<empty>";
    }

    public TSessionPuttyEmpty(ISessionPutty session) : base(session) {
      Protocol = TPuttyProtocol.Telnet;
    }
    public override ISession Duplicate() {
      return new TSessionPuttyEmpty(this);
    }
  }
}
