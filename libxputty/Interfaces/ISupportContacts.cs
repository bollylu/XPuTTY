using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty.Interfaces {
  interface ISupportContacts {
    IList<ISupportContact> SupportContacts { get; }
  }
}
