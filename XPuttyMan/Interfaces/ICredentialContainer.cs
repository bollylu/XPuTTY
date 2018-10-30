using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPuttyMan {
  public interface ICredentialContainer {
    ICredential Credential { get; }
    void SetLocalCredential(ICredential credential);
    bool HasUnsecuredPassword { get; }
    void SetSecure(bool value, bool recurse = true);
  }
}
