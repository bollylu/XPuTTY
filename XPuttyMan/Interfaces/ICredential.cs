using BLTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace XPuttyMan {
  public interface ICredential : IParent, IToXml {
    string Username { get; }
    string Domain { get; }
    string UsernameWithoutDomain { get; }
    bool XmlSecure { get;}
    string EncryptionKey { set; }
    bool HasValue { get; }
    bool Inherited { get; }
    SecureString SecurePassword { get; }
    PSCredential PsCredential { get; }

    void SetSecure(bool value);
    void Dispose();
  }
}
