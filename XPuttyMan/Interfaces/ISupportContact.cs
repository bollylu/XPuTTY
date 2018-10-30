using BLTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XPuttyMan {
  public interface ISupportContact {
    string SupportMessage { get; }
    string Name { get; }
    string Description { get; }
    string Email { get; }
    string Phone { get; }
    string HelpUri { get; }
    string Message { get; }

    void DisplaySupportMessage();

  }
}
