using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty {
  interface ISupportContacts {
    IList<ISupportContact> SupportContacts { get; }
  }
}
