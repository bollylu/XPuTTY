using BLTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace libxputty.Interfaces {
  public interface ICredential : IToXml {
    string Username { get; set; }
    string Domain { get; }
    string UsernameWithoutDomain { get; }
    bool XmlSecure { get;}
    string EncryptionKey { get; }
    bool HasValue { get; }
    bool Inherited { get; }
    SecureString SecurePassword { get; set; }
    IParent Parent { get; }

    void SetSecure(bool value);
    void Dispose();
  }
}
