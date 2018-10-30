using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty_std20.Interfaces {
  interface ISupportContacts {
    IList<ISupportContact> SupportContacts { get; }
  }
}
